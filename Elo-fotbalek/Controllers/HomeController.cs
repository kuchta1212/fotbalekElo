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

        public HomeController(IBlobClient blobClient, IModelCreator modelCreator, IEloCalulator eloCalulator)
        {
            this.blobClient = blobClient;
            this.modelCreator = modelCreator;
            this.eloCalulator = eloCalulator;
        }

        public async Task<IActionResult> Index()
        {
            var players = await this.blobClient.GetPlayers();
            var matches = await this.blobClient.GetMatches();

            var screen = new HomeScreenModel()
            {
                Matches = matches.OrderByDescending(m => m.Date),
                Players = players.OrderByDescending(p => p.Elo)
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
        public async Task<IActionResult> AddMatchAndCalculateElo(string WinnerAmount, string LooserAmount, string Weight)
        {
            Request.Form.TryGetValue("winner", out var winners);
            Request.Form.TryGetValue("looser", out var loosers);

            var winnTeam = await this.modelCreator.CreateTeam(winners);
            var loosTeam = await this.modelCreator.CreateTeam(loosers);

            var match = new Match()
            {
                Date = DateTime.Today,
                WinnerAmount = int.Parse(WinnerAmount),
                LooserAmount = int.Parse(LooserAmount),
                Winner = winnTeam,
                Looser = loosTeam,
                Weight = Weight == "BigMatch" ? 30 : 10
            };

            await this.blobClient.AddMatch(match);

            var eloResult = this.eloCalulator.CalculateFifaElo(match);

            await this.UpdatePlayersElo(eloResult, match);


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
                Elo = 1000
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

        private async Task UpdatePlayersElo(EloResult eloResult)
        {
            var players = await this.blobClient.GetPlayers();

            foreach (var individualResult in eloResult.GetResults())
            {
                var player = players.First(p => p.Id == individualResult.PlayerIdentifier.Value);
                player.Elo = individualResult.RatingAfter;
            }

            await this.blobClient.UpdatePlayers(players);
        }

        private async Task UpdatePlayersElo(FifaEloResult eloResult, Match match)
        {
            var players = await this.blobClient.GetPlayers();

            foreach (var player in match.Winner.Players)
            {
                var newPlayerWinner = players.First(np => np.Id == player.Id);
                newPlayerWinner.Elo += (int)eloResult.WinnerPointChange;
            }

            foreach (var player in match.Looser.Players)
            {
                var newPlayerLooser = players.First(np => np.Id == player.Id);
                newPlayerLooser.Elo += (int)eloResult.LooserPointChange;
            }

            await this.blobClient.UpdatePlayers(players);
        }

        public async Task<IActionResult> RecalculateToNewElo()
        {
            //reset to default value
            var players = await this.blobClient.GetPlayers();

            foreach (var player in players)
            {
                player.Elo = 1000;
            }

            await this.blobClient.UpdatePlayers(players);

            var matches = await this.blobClient.GetMatches();

            foreach (var match in matches.OrderBy(m => m.Date))
            {
                await this.blobClient.RemoveMatch(match);

                var newPlayers = await this.blobClient.GetPlayers();

                var teamElo = 0;
                foreach (var player in match.Winner.Players)
                {
                    player.Elo = newPlayers.First(p => p.Id == player.Id).Elo;
                    teamElo += player.Elo;
                }

                match.Winner.TeamElo = teamElo / match.Winner.Players.Count;

                teamElo = 0;
                foreach (var player in match.Looser.Players)
                {
                    player.Elo = newPlayers.First(p => p.Id == player.Id).Elo;
                    teamElo += player.Elo;
                }

                match.Looser.TeamElo = teamElo / match.Looser.Players.Count;

                await this.blobClient.AddMatch(match);

                var eloResult = this.eloCalulator.CalculateFifaElo(match);

                foreach (var player in match.Winner.Players)
                {
                    var newPlayerWinner = newPlayers.First(np => np.Id == player.Id);
                    newPlayerWinner.Elo += (int) eloResult.WinnerPointChange;
                }

                foreach (var player in match.Looser.Players)
                {
                    var newPlayerLooser = newPlayers.First(np => np.Id == player.Id);
                    newPlayerLooser.Elo += (int)eloResult.LooserPointChange;
                }

                await this.blobClient.UpdatePlayers(newPlayers);
            }

            


            return RedirectToAction("Index");
        }
    }
}
