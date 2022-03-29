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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

            var regulars = new List<Player>();
            var nonRegulars = new List<Player>();

            foreach (var player in players)
            { 
                if(player.Percentage >= 30)
                {
                    regulars.Add(player);
                }
                else
                {
                    nonRegulars.Add(player);
                }
            }

            IOrderedEnumerable<Player> sortedPlayers;
            IOrderedEnumerable<Player> sortedNonRegularPlayers;
            switch (sortOrder)
            {
                case "summer":
                    sortedPlayers = regulars.OrderByDescending(p => p.Elos.SummerElo);
                    sortedNonRegularPlayers = nonRegulars.OrderByDescending(p => p.Elos.SummerElo);
                    break;
                case "winter":
                    sortedPlayers = regulars.OrderByDescending(p => p.Elos.WinterElo);
                    sortedNonRegularPlayers = nonRegulars.OrderByDescending(p => p.Elos.WinterElo);
                    break;
                default:
                    sortedPlayers = regulars.OrderByDescending(p => p.Elo);
                    sortedNonRegularPlayers = nonRegulars.OrderByDescending(p => p.Elo);
                    break;
            }

            var screen = new HomeScreenModel()
            {
                Matches = matches.OrderByDescending(m => m.Date),
                Players = sortedPlayers,
                NonRegulars = sortedNonRegularPlayers
            };

            return View(screen);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AddMatch()
        {
            var players = await this.blobClient.GetPlayers();

            players.Add(new Player() {Id = Guid.Empty, Name = "---"});
            var selectedList = new SelectList(players.OrderBy(p => p.Name), "Id", "Name");

            ViewData["Players"] = selectedList;
            return View("AddMatch");
        }

        [HttpPost]
        public async Task<IActionResult> AddMatchAndCalculateElo(string WinnerAmount, string LooserAmount, string Weight, string season, string hero)
        {
            var enumSeason = Enum.Parse<Season>(season);

            Request.Form.TryGetValue("winner", out var winners);
            Request.Form.TryGetValue("looser", out var loosers);

            var winnTeam = await this.modelCreator.CreateTeam(winners.Where(v => Guid.Parse(v) != Guid.Empty), enumSeason);
            var loosTeam = await this.modelCreator.CreateTeam(loosers.Where(v => Guid.Parse(v) != Guid.Empty), enumSeason);

            var heroName = Guid.Empty.ToString() != hero
                ? (await this.blobClient.GetPlayers()).First(p => p.Id.Equals(Guid.Parse(hero))).Name
                : string.Empty;

            var match = new Match()
            {
                Date = DateTime.Now,
                WinnerAmount = int.Parse(WinnerAmount),
                LooserAmount = int.Parse(LooserAmount),
                Winner = winnTeam,
                Looser = loosTeam,
                Weight = Weight == "BigMatch" ? 30 : 10,
                Season = enumSeason,
                Hero = heroName
            };

            await this.blobClient.AddMatch(match);

            var eloResult = this.eloCalulator.CalculateFifaElo(match);
            
            await this.UpdatePlayersElo(eloResult, match);
            await this.PunishNonCommers(match);
            await this.RecalculatePercentage();

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
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
                AmountOfMissedGames = 0,
                Percentage = 0
            };

            await this.blobClient.AddPlayer(player);

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

            var substitudeList = new SelectList(players.OrderBy(p => p.Name), "Id", "Name");
            players.Add(new Player() { Id = Guid.Empty, Name = "---" });
            players.Add(new Player() {Id = Guid.Empty, Name = "Náhradník"});
            var playersList = new SelectList(players.OrderBy(p => p.Name), "Id", "Name");

            ViewData["Players"] = playersList;
            ViewData["Substitutes"] = substitudeList;
            return View("ShowTeams");
        }

        [HttpPost]
        public async Task<IActionResult> GenerateTeamsResult(string Season)
        {
            Request.Form.TryGetValue("player", out var players);
            Request.Form.TryGetValue("substitude", out var substitues);
            
            var playerIds = players.Where(v => Guid.Parse(v) != Guid.Empty);
            var subsitudeIds = substitues.Where(v => Guid.Parse(v) != Guid.Empty);

            var enumSeason = Enum.Parse<Season>(Season);

            var dummyTeamPlayers = await this.modelCreator.CreateTeam(playerIds, enumSeason);
            var dummyTeamSubstitudes = await this.modelCreator.CreateTeam(subsitudeIds, enumSeason);

            dummyTeamSubstitudes.Players.ForEach(s => s.Name += "-Náhradník");
            dummyTeamPlayers.Players.AddRange(dummyTeamSubstitudes.Players);

            var generatorResults = this.teamGenerator.GenerateTeams(dummyTeamPlayers.Players, enumSeason);

            ViewData["GeneratedResults"] = generatorResults;
            return View("ShowTeamsResult");
        }

        [HttpGet]
        public async Task<IActionResult> GetPlayerStats(Guid playerId)
        {
            var player = (await this.blobClient.GetPlayers()).First(p => p.Id == playerId);
            var matches = await this.blobClient.GetMatches();

            var elos = new List<ChartModel>();
            int matchCount = 0, minElo = int.MaxValue, maxElo = int.MinValue;

            var dateTimeValue = matches.First().Date.AddDays(-1);
            foreach (var match in matches)
            {
                var old = match.Winner.Players.FirstOrDefault(p => p.Equals(player));
                old = old ?? match.Looser.Players.FirstOrDefault(p => p.Equals(player));
                
                if(old != null)
                {
                    this.AddEloStat(elos, dateTimeValue, old.Elo, ref minElo, ref maxElo);
                    matchCount++;
                    dateTimeValue = match.Date;
                }
            }

            this.AddEloStat(elos, dateTimeValue, player.Elo, ref minElo, ref maxElo);

            var stats = new PlayerStats()
            {
                Player = player,
                Elos = elos,
                MaxElo = maxElo,
                MinElo = minElo,
                PlayedMatches = matchCount,
                Attandance = player.Percentage
            };

            return View("PlayerStats", stats);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DeleteMatch(string date, string score)
        {
            var matches = await this.blobClient.GetMatches();
            var players = await this.blobClient.GetPlayers();

            var match = matches.First(m => m.Date.ToString() == date && m.Score == score);

            foreach (var matchPlayer in match.Winner.Players)
            {
                players.Remove(players.First(x => x.Id == matchPlayer.Id));
                players.Add(matchPlayer);
            }

            foreach (var matchPlayer in match.Looser.Players)
            {
                players.Remove(players.First(x => x.Id == matchPlayer.Id));
                players.Add(matchPlayer);
            }

            await this.blobClient.UpdatePlayers(players);
            await this.blobClient.RemoveMatch(match);

            return RedirectToAction("Index");
        }

        private void AddEloStat(List<ChartModel> elos, DateTime dateTime, int elo, ref int minElo, ref int maxElo)
        {
            if (elo < minElo)
            {   
                minElo = elo;
            }

            if (elo > maxElo)
            {
                maxElo = elo;
            }

            elos.Add(new ChartModel() { DateTime = dateTime, Elo = elo });
        }

        private async Task UpdatePlayersElo(FifaEloResult eloResult, Match match)
        {
            var players = await this.blobClient.GetPlayers();

            foreach (var player in match.Winner.Players)
            {
                var newPlayerWinner = players.First(np => np.Id == player.Id);
                newPlayerWinner.UpdateElo((int)eloResult.WinnerPointChange, match.Season);
                newPlayerWinner.Elo = Util.CountGeneralElo(newPlayerWinner.Elos);
                
                if (match.WinnerAmount == match.LooserAmount)
                {
                    newPlayerWinner.Trend = this.trendCalculator.CalculateTrend(player.Trend, match.Date, 0);

                    if(newPlayerWinner.AmountOfTies == null)
                    {
                        newPlayerWinner.AmountOfTies = new MatchCounter();
                    }

                    if (match.Weight == 30)
                    {
                        newPlayerWinner.AmountOfTies.BigMatches++;
                    }
                    else
                    {
                        newPlayerWinner.AmountOfTies.SmallMatches++;
                    }
                }
                else
                {
                    newPlayerWinner.Trend = this.trendCalculator.CalculateTrend(player.Trend, match.Date, 1);

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
                }
                newPlayerWinner.AmountOfMissedGames = 0;
            }

            foreach (var player in match.Looser.Players)
            {
                var newPlayerLooser = players.First(np => np.Id == player.Id);
                newPlayerLooser.UpdateElo((int)eloResult.LooserPointChange, match.Season);
                newPlayerLooser.Elo = Utils.Util.CountGeneralElo(newPlayerLooser.Elos);

                if(match.WinnerAmount == match.LooserAmount)
                {
                    newPlayerLooser.Trend = this.trendCalculator.CalculateTrend(player.Trend, match.Date, 0);

                    if (newPlayerLooser.AmountOfTies == null)
                    {
                        newPlayerLooser.AmountOfTies = new MatchCounter();
                    }

                    if (match.Weight == 30)
                    {
                        newPlayerLooser.AmountOfTies.BigMatches++;
                    }
                    else
                    {
                        newPlayerLooser.AmountOfTies.SmallMatches++;
                    }
                }
                else
                {
                    newPlayerLooser.Trend = this.trendCalculator.CalculateTrend(player.Trend, match.Date, -1);

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

        private async Task RecalculatePercentage()
        {
            var players = await this.blobClient.GetPlayers();
            var matches = await this.blobClient.GetMatches();

            foreach (var player in players)
            {
                var totalAmountOfPlayedMatches = (player.AmountOfLooses?.TotalAmount() ?? 0) + (player.AmountOfWins?.TotalAmount() ?? 0);
                player.Percentage = (int)(((double)totalAmountOfPlayedMatches / (double)matches.Count) * 100);
            }

            await this.blobClient.UpdatePlayers(players);
        }



    }
}
