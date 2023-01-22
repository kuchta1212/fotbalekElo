namespace Elo_fotbalek.Controllers
{
    using Elo_fotbalek.Configuration;
    using Elo_fotbalek.Models;
    using Elo_fotbalek.Storage;
    using Elo_fotbalek.TeamGenerator;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    [Route("doodle")]
    public class DoodleController : Controller
    {
        private readonly IBlobClient blobClient;
        private readonly ITeamGenerator teamGenerator;
        private readonly IOptions<AppConfigurationOptions> appConfiguration;

        public DoodleController(IBlobClient blobClient, ITeamGenerator teamGenerator, IOptions<AppConfigurationOptions> appConfiguration)
        {
            this.blobClient = blobClient;
            this.teamGenerator = teamGenerator;
            this.appConfiguration = appConfiguration;
        }

        public async Task<IActionResult> Index()
        {
            var doodles = await this.blobClient.GetDoodle();

            var model = new DoodleModel()
            {
                Doodle = await this.CheckForNewPlayersAndUpdate(doodles),
                Stats = doodles.Count > 0 ? this.CreateStats(doodles) : new DoodleStats(),
                AppConfiguration = this.appConfiguration.Value
            };

            return View(model);
        }

        [HttpPost]
        [Route("vote")]
        public async Task<IActionResult> Vote(string name, string date, string value)
        {
            var doodles = await this.blobClient.GetDoodle();

            Enum.TryParse(value, out DoodleValue valueInEnum);
            var dateTime = DateTime.ParseExact(date, "dd.MM", CultureInfo.InvariantCulture);

            if (valueInEnum == DoodleValue.Accept)
            {
                var amountOfAccepted = doodles.Count(d => d.PlayersPoll[dateTime] == DoodleValue.Accept);
                if (amountOfAccepted >= this.appConfiguration.Value.PlayerLimit)
                {
                    return BadRequest(); 
                }
            }


            var playersDoodle = doodles.Where(d => d.Player == name).FirstOrDefault();
            if(playersDoodle != null)
            {
                playersDoodle.PlayersPoll[dateTime] = valueInEnum;

                await this.blobClient.SaveDoodle(doodles);
            }

            return new OkResult();
        }

        [HttpPost]
        [Route("generate-teams")]
        public async Task<IActionResult> GenerateTeams(string season)
        {
            var doodles = await this.blobClient.GetDoodle();
            var players = await this.blobClient.GetPlayers();

            var currentPlayersName = doodles.Where(d => d.PlayersPoll[d.GetSortedPlayersPoll().Keys.First()] == DoodleValue.Accept).Select(d => d.Player).ToList();
            var currentPlayersIds = players.Where(p => currentPlayersName.Contains(p.Name)).Select(p => p.Id.ToString());

            var enumSeason = string.IsNullOrEmpty(season) ? Season.Summer : Enum.Parse<Season>(season);

            var generatorResults = await this.teamGenerator.GenerateTeams(currentPlayersIds.ToList(), enumSeason);

            ViewData["GeneratedResults"] = generatorResults;
            return View("ShowTeamsResult", new BaseModel() { AppConfiguration = this.appConfiguration.Value});
        }

        [HttpPost]
        [Authorize]
        [Route("new-poll")]
        public async Task<IActionResult> RemoveOldAndCreateNew()
        {
            var doodles = await this.blobClient.GetDoodle();

            var min = doodles[0].PlayersPoll.Keys.Min<DateTime>();
            if (min.AddDays(1) > DateTime.UtcNow)
            {
                return RedirectToAction("Index");
            }

            var max = doodles[0].PlayersPoll.Keys.Max<DateTime>();
            var newMax = max.AddDays(7);

            foreach(var doodle in doodles)
            {
                doodle.PlayersPoll.Remove(min);
                doodle.PlayersPoll.Add(newMax, DoodleValue.NoAnswer);
            }

            await this.blobClient.SaveDoodle(doodles);

            return RedirectToAction("Index");
        }

        private DoodleStats CreateStats(List<Doodle> doodle)
        {
            var stats = new DoodleStats();
            foreach(var player in doodle)
            {
                switch (player.PlayersPoll[player.GetSortedPlayersPoll().Keys.First()])
                {
                    case DoodleValue.Accept:
                        stats.Coming++;
                        break;
                    case DoodleValue.Maybe:
                        stats.Maybe++;
                        break;
                    case DoodleValue.Refused:
                        stats.Refused++;
                        break;
                }
            }

            return stats;
        }

        private async Task<List<Doodle>> CheckForNewPlayersAndUpdate(List<Doodle> doodles)
        {
            var players = await this.blobClient.GetPlayers();
            if(doodles.Count == players.Count)
            {
                return doodles;
            }

            var playersFromDoodle = doodles.Select(d => d.Player).ToList();
            var playersName = players.Select(p => p.Name).ToList();

            var missingPlayers = playersName.Except(playersFromDoodle).ToList();

            var newDoodle = this.AddPlayersToDoodle(missingPlayers, doodles);

            await this.blobClient.SaveDoodle(newDoodle);

            return newDoodle;
        }

        private List<Doodle> AddPlayersToDoodle(List<string> missingPlayers, List<Doodle> doodles)
        {
            foreach(var missingPlayer in missingPlayers)
            {
                var newDoodle = new Doodle()
                {
                    Player = missingPlayer,
                    PlayersPoll = new Dictionary<DateTime, DoodleValue>()
                };

                if(doodles.Count > 0)
                {
                    foreach (var date in doodles[0].PlayersPoll.Keys)
                    {
                        newDoodle.PlayersPoll.Add(date, DoodleValue.NoAnswer);
                    }
                }

                doodles.Add(newDoodle);
            }

            return doodles;
        }
    }
}
