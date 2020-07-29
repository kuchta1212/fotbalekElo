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
        public async Task<IActionResult> AddMatchAndCalculateElo(string WinnerAmount, string LooserAmount)
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
                Looser = loosTeam
            };

            await this.blobClient.AddMatch(match);

            var eloResult = this.eloCalulator.CalculateElo(match);

            await this.UpdatePlayersElo(eloResult);


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
    }
}
