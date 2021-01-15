using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EloCalculator;
using Elo_fotbalek.EloCounter;
using Microsoft.AspNetCore.Mvc;
using Elo_fotbalek.Models;
using Elo_fotbalek.Storage;
using Elo_fotbalek.TeamGenerator;
using Elo_fotbalek.TrendCalculator;
using Elo_fotbalek.Utils;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace Elo_fotbalek.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBlobClient blobClient;
        private readonly IModelCreator modelCreator;
        private readonly IEloCalulator eloCalulator;
        private readonly ITeamGenerator teamGenerator;
        private readonly ITrendCalculator trendCalculator;

        public HomeController(IBlobClient blobClient, IModelCreator modelCreator, IEloCalulator eloCalulator, ITeamGenerator teamGenerator, ITrendCalculator trendCalculator)
        {
            this.blobClient = blobClient;
            this.modelCreator = modelCreator;
            this.eloCalulator = eloCalulator;
            this.teamGenerator = teamGenerator;
            this.trendCalculator = trendCalculator;
        }

        public async Task<IActionResult> Index(string sortOrder)
        {
            var players = await this.blobClient.GetPlayers();
            var matches = await this.blobClient.GetMatches();

            var regulars = players.Where(x => x.AmountOfMissedGames < 5).ToList();
            var nonRegulars = players.Where(x => x.AmountOfMissedGames >= 5).ToList();

            IOrderedEnumerable<Player> sortedPlayers;
            switch (sortOrder)
            {
                case "summer":
                    sortedPlayers = regulars.OrderByDescending(p => p.Elos.SummerElo);
                    break;
                case "winter":
                    sortedPlayers = regulars.OrderByDescending(p => p.Elos.WinterElo);
                    break;
                default:
                    sortedPlayers = regulars.OrderByDescending(p => p.Elo);
                    break;
            }
            
            var screen = new HomeScreenModel()
            {
                Matches = matches.OrderByDescending(m => m.Date),
                Players = sortedPlayers,
                NonRegulars = nonRegulars.OrderByDescending(p => p.AmountOfMissedGames)
            };

            return View(screen);
        }

        [HttpGet]
        public async Task<IActionResult> AddMatch()
        {
            var players = await this.blobClient.GetPlayers();

            players.Add(new Player() {Id = Guid.Empty, Name = "---"});
            var selectedList = new SelectList(players.OrderBy(p => p.Name), "Id", "Name");

            ViewData["Players"] = selectedList;
            return View("AddMatch");
        }

        [HttpPost]
        public async Task<IActionResult> AddMatchAndCalculateElo(string WinnerAmount, string LooserAmount, string Weight, string season)
        {
            var enumSeason = Enum.Parse<Season>(season);

            Request.Form.TryGetValue("winner", out var winners);
            Request.Form.TryGetValue("looser", out var loosers);

            var winnTeam = await this.modelCreator.CreateTeam(winners, enumSeason);
            var loosTeam = await this.modelCreator.CreateTeam(loosers, enumSeason);

            var match = new Match()
            {
                Date = DateTime.Now,
                WinnerAmount = int.Parse(WinnerAmount),
                LooserAmount = int.Parse(LooserAmount),
                Winner = winnTeam,
                Looser = loosTeam,
                Weight = Weight == "BigMatch" ? 30 : 10,
                Season = enumSeason
            };

            await this.blobClient.AddMatch(match);

            var eloResult = this.eloCalulator.CalculateFifaElo(match);
            
            await this.UpdatePlayersElo(eloResult, match);
            await this.PunishNonCommers(match);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult AddPlayer()
        {
            return View("AddPlayer");
        }

        [HttpPost]
        public async Task<IActionResult> AddPlayer(string Name)
        {
            var player = new Player()
            {
                Id = Guid.NewGuid(),
                Name = Name,
                Elo = 1000,
                Elos = new SeasonalElos()
                {
                    SummerElo = 1000,
                    WinterElo = 1000
                },
                Trend = new TrendData()
                {
                    Data = new Dictionary<DateTime, int>(),
                    Trend = Trend.STAY
                },
               AmountOfMissedGames = 0
            };

            await this.blobClient.AddPlayer(player);

            return RedirectToAction("Index");
        }

        public IActionResult Details(Guid id)
        {
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> GenerateTeams()
        {
            var players = await this.blobClient.GetPlayers();

            players.Add(new Player() { Id = Guid.Empty, Name = "---" });
            var selectedList = new SelectList(players.OrderBy(p => p.Name), "Id", "Name");

            ViewData["Players"] = selectedList;
            return View("ShowTeams");
        }

        [HttpPost]
        public async Task<IActionResult> GenerateTeamsResult(string Season)
        {
            var enumSeason = Enum.Parse<Season>(Season);
            Request.Form.TryGetValue("player", out var players);
            var dummyTeam = await this.modelCreator.CreateTeam(players, enumSeason);
            var generatorResults = this.teamGenerator.GenerateTeams(dummyTeam.Players, enumSeason);

            ViewData["GeneratedResults"] = generatorResults;
            return View("ShowTeamsResult");
        }

        private async Task UpdatePlayersElo(FifaEloResult eloResult, Match match)
        {
            var players = await this.blobClient.GetPlayers();

            foreach (var player in match.Winner.Players)
            {
                var newPlayerWinner = players.First(np => np.Id == player.Id);
                newPlayerWinner.UpdateElo((int)eloResult.WinnerPointChange, match.Season);
                newPlayerWinner.Elo = Utils.Util.CountGeneralElo(newPlayerWinner.Elos);
                newPlayerWinner.Trend = this.trendCalculator.CalculateTrend(player.Trend, match.Date, true);

                if (newPlayerWinner.AmountOfWins == null)
                {
                    newPlayerWinner.AmountOfWins = new MatchCounter();
                }

                if (match.Weight == 30)
                {
                    newPlayerWinner.AmountOfWins.BigMatches++;
                }
                else
                {
                    newPlayerWinner.AmountOfWins.SmallMatches++;
                }

                newPlayerWinner.AmountOfMissedGames = 0;
            }

            foreach (var player in match.Looser.Players)
            {
                var newPlayerLooser = players.First(np => np.Id == player.Id);
                newPlayerLooser.UpdateElo((int)eloResult.LooserPointChange, match.Season);
                newPlayerLooser.Elo = Utils.Util.CountGeneralElo(newPlayerLooser.Elos);
                newPlayerLooser.Trend = this.trendCalculator.CalculateTrend(player.Trend, match.Date, false);


                if (newPlayerLooser.AmountOfLooses == null)
                {
                    newPlayerLooser.AmountOfLooses = new MatchCounter();
                }

                if (match.Weight == 30)
                {
                    newPlayerLooser.AmountOfLooses.BigMatches++;
                }
                else
                {
                    newPlayerLooser.AmountOfLooses.SmallMatches++;
                }

                newPlayerLooser.AmountOfMissedGames = 0;
            }

            await this.blobClient.UpdatePlayers(players);
        }

        private async Task PunishNonCommers(Match match)
        {
            var players = await this.blobClient.GetPlayers();
            var todaysPlayers = match.Looser.Players.Union(match.Winner.Players);

            var nonCommers = players.Except(todaysPlayers);
            foreach (var nonCommer in nonCommers)
            {
                nonCommer.AmountOfMissedGames++;
                nonCommer.Trend = this.trendCalculator.RemoveLatest(nonCommer.Trend);
            }

            await this.blobClient.UpdatePlayers(players);
        }


    }
}
