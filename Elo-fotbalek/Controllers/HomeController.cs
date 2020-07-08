using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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

        public HomeController(IBlobClient blobClient)
        {
            this.blobClient = blobClient;
        }

        public async Task<IActionResult> Index()
        {
            var players = await this.blobClient.GetPlayers();

            return View(players);
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
        public IActionResult AddMatchAndCalculateElo(string WinnerAmount, string LooserAmount)
        {
            Request.Form.TryGetValue("winner", out var winners);
            Request.Form.TryGetValue("looser", out var loosers);
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
    }
}
