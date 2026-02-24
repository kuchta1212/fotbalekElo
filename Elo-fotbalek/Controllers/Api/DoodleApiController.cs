namespace Elo_fotbalek.Controllers.Api
{
    using Elo_fotbalek.Configuration;
    using Elo_fotbalek.Models;
    using Elo_fotbalek.Storage;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/doodle")]
    public class DoodleApiController : BaseApiController
    {
        private readonly IBlobClient blobClient;
        private readonly IOptions<AppConfigurationOptions> appConfiguration;

        public DoodleApiController(IBlobClient blobClient, IOptions<AppConfigurationOptions> appConfiguration)
        {
            this.blobClient = blobClient;
            this.appConfiguration = appConfiguration;
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcoming([FromQuery] int count = 5)
        {
            var doodles = await this.blobClient.GetDoodle();
            var updatedDoodles = await this.CheckForNewPlayersAndUpdate(doodles);

            if (updatedDoodles.Count == 0)
            {
                var emptyResponse = new
                {
                    dates = new List<DoodleDateDto>(),
                    stats = new DoodleStatsDto { Coming = 0, Maybe = 0, Refused = 0 },
                    playerLimit = this.appConfiguration.Value.PlayerLimit,
                    overLimitMessage = this.appConfiguration.Value.OverLimitMessage,
                    isSeasoningSupported = this.appConfiguration.Value.IsSeasoningSupported
                };

                return Ok(emptyResponse);
            }

            var sortedDates = updatedDoodles[0].GetSortedPlayersPoll().Keys.Take(count).ToList();
            var dateDtos = new List<DoodleDateDto>();

            foreach (var date in sortedDates)
            {
                var players = updatedDoodles.Select(d => new DoodlePlayerDto
                {
                    Name = d.Player,
                    Status = d.PlayersPoll[date].ToString()
                }).OrderBy(p => p.Name).ToList();

                dateDtos.Add(new DoodleDateDto
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    DisplayDate = date.ToString("dd.MM"),
                    Players = players
                });
            }

            var stats = this.CreateStats(updatedDoodles, sortedDates.First());

            var response = new
            {
                dates = dateDtos,
                stats = new DoodleStatsDto
                {
                    Coming = stats.Coming,
                    Maybe = stats.Maybe,
                    Refused = stats.Refused
                },
                playerLimit = this.appConfiguration.Value.PlayerLimit,
                overLimitMessage = this.appConfiguration.Value.OverLimitMessage,
                isSeasoningSupported = this.appConfiguration.Value.IsSeasoningSupported
            };

            return Ok(response);
        }

        [HttpGet("{dateStr}")]
        public async Task<IActionResult> GetDoodleForDate(string dateStr)
        {
            var doodles = await this.blobClient.GetDoodle();
            
            if (doodles.Count == 0)
            {
                return NotFound("No doodle data found");
            }

            if (!DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var requestedDate))
            {
                return BadRequest("Invalid date format. Use yyyy-MM-dd");
            }

            var firstDoodle = doodles[0];
            var availableDate = firstDoodle.PlayersPoll.Keys.FirstOrDefault(d => d.Date == requestedDate.Date);
            
            if (availableDate == default(DateTime))
            {
                return NotFound($"Date {dateStr} not found in doodle");
            }

            var players = doodles.Select(d => new DoodlePlayerDto
            {
                Name = d.Player,
                Status = d.PlayersPoll[availableDate].ToString()
            }).OrderBy(p => p.Name).ToList();

            var response = new
            {
                date = availableDate.ToString("yyyy-MM-dd"),
                displayDate = availableDate.ToString("dd.MM"),
                players
            };

            return Ok(response);
        }

        [HttpPut("{dateStr}/availability")]
        public async Task<IActionResult> UpdateAvailability(string dateStr, [FromBody] UpdateAvailabilityRequest request)
        {
            if (string.IsNullOrEmpty(request.PlayerName) || string.IsNullOrEmpty(request.Status))
            {
                return BadRequest("PlayerName and Status are required");
            }

            if (!Enum.TryParse<DoodleValue>(request.Status, out var doodleValue))
            {
                return BadRequest("Invalid status. Must be Accept, Maybe, Refused, or NoAnswer");
            }

            if (!DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var requestedDate))
            {
                return BadRequest("Invalid date format. Use yyyy-MM-dd");
            }

            var doodles = await this.blobClient.GetDoodle();

            if (doodles.Count == 0)
            {
                return NotFound("No doodle data found");
            }

            var firstDoodle = doodles[0];
            var availableDate = firstDoodle.PlayersPoll.Keys.FirstOrDefault(d => d.Date == requestedDate.Date);
            
            if (availableDate == default(DateTime))
            {
                return NotFound($"Date {dateStr} not found in doodle");
            }

            // Check player limit if trying to accept
            if (doodleValue == DoodleValue.Accept)
            {
                var amountOfAccepted = doodles.Count(d => d.PlayersPoll[availableDate] == DoodleValue.Accept);
                if (amountOfAccepted >= this.appConfiguration.Value.PlayerLimit)
                {
                    return BadRequest(new { 
                        error = "Player limit reached", 
                        message = this.appConfiguration.Value.OverLimitMessage 
                    });
                }
            }

            var playerDoodle = doodles.FirstOrDefault(d => d.Player == request.PlayerName);
            if (playerDoodle == null)
            {
                return NotFound($"Player {request.PlayerName} not found");
            }

            playerDoodle.PlayersPoll[availableDate] = doodleValue;
            await this.blobClient.SaveDoodle(doodles);

            // Return updated stats
            var stats = this.CreateStats(doodles, availableDate);

            var response = new
            {
                success = true,
                stats = new DoodleStatsDto
                {
                    Coming = stats.Coming,
                    Maybe = stats.Maybe,
                    Refused = stats.Refused
                }
            };

            return Ok(response);
        }

        [HttpPost("advance-poll")]
        [Authorize(policy: "MyPolicy")]
        public async Task<IActionResult> AdvancePoll()
        {
            var doodles = await this.blobClient.GetDoodle();

            if (doodles.Count == 0)
            {
                return NotFound("No doodle data found");
            }

            var minDate = doodles[0].PlayersPoll.Keys.Min<DateTime>();
            
            // Don't advance if the oldest date is still in the future
            if (minDate.AddDays(1) > DateTime.UtcNow)
            {
                return BadRequest(new { 
                    error = "Cannot advance poll", 
                    message = "The oldest date has not passed yet" 
                });
            }

            var maxDate = doodles[0].PlayersPoll.Keys.Max<DateTime>();
            var newDate = maxDate.AddDays(7); // Add next Tuesday (7 days later)

            // Remove oldest date and add new date for all players
            foreach (var doodle in doodles)
            {
                doodle.PlayersPoll.Remove(minDate);
                doodle.PlayersPoll.Add(newDate, DoodleValue.NoAnswer);
            }

            await this.blobClient.SaveDoodle(doodles);

            var response = new
            {
                success = true,
                message = "Poll advanced successfully",
                removedDate = minDate.ToString("yyyy-MM-dd"),
                addedDate = newDate.ToString("yyyy-MM-dd")
            };

            return Ok(response);
        }

        private DoodleStats CreateStats(List<Doodle> doodle, DateTime date)
        {
            var stats = new DoodleStats();
            foreach (var player in doodle)
            {
                switch (player.PlayersPoll[date])
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
            if (doodles.Count == players.Count)
            {
                return doodles;
            }

            var playersFromDoodle = doodles.Select(d => d.Player).ToList();
            var playersName = players.Select(p => p.Name).ToList();

            var missingPlayers = playersName.Except(playersFromDoodle).ToList();
            var removedPlayers = playersFromDoodle.Except(playersName).ToList();

            var newDoodle = this.AddAndRemovePlayersToDoodle(missingPlayers, removedPlayers, doodles);

            await this.blobClient.SaveDoodle(newDoodle);

            return newDoodle;
        }

        private List<Doodle> AddAndRemovePlayersToDoodle(List<string> missingPlayers, List<string> removedPlayers, List<Doodle> doodles)
        {
            foreach (var missingPlayer in missingPlayers)
            {
                var newDoodle = new Doodle()
                {
                    Player = missingPlayer,
                    PlayersPoll = new Dictionary<DateTime, DoodleValue>()
                };

                if (doodles.Count > 0)
                {
                    foreach (var date in doodles[0].PlayersPoll.Keys)
                    {
                        newDoodle.PlayersPoll.Add(date, DoodleValue.NoAnswer);
                    }
                }

                doodles.Add(newDoodle);
            }

            if (removedPlayers.Count > 0)
            {
                var doodlesToRemove = doodles.Where(d => removedPlayers.Contains(d.Player)).ToList();
                foreach (var doodle in doodlesToRemove)
                {
                    doodles.Remove(doodle);
                }
            }

            return doodles;
        }
    }

    // DTOs
    public class DoodleDateDto
    {
        public string Date { get; set; }
        public string DisplayDate { get; set; }
        public List<DoodlePlayerDto> Players { get; set; }
    }

    public class DoodlePlayerDto
    {
        public string Name { get; set; }
        public string Status { get; set; }
    }

    public class DoodleStatsDto
    {
        public int Coming { get; set; }
        public int Maybe { get; set; }
        public int Refused { get; set; }
    }

    public class UpdateAvailabilityRequest
    {
        public string PlayerName { get; set; }
        public string Status { get; set; }
    }
}
