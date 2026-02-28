namespace Elo_fotbalek.Controllers.Api
{
    using Elo_fotbalek.Models;
    using Elo_fotbalek.Storage;
    using Elo_fotbalek.TeamGenerator;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/teams")]
    public class TeamsApiController : BaseApiController
    {
        private readonly IBlobClient blobClient;
        private readonly ITeamGenerator teamGenerator;

        public TeamsApiController(IBlobClient blobClient, ITeamGenerator teamGenerator)
        {
            this.blobClient = blobClient;
            this.teamGenerator = teamGenerator;
        }

        [HttpGet("generate")]
        public async Task<IActionResult> GenerateTeamsFromDoodle([FromQuery] string date, [FromQuery] string season)
        {
            var enumSeason = string.IsNullOrEmpty(season) ? Season.Summer : Enum.Parse<Season>(season);

            var doodles = await this.blobClient.GetDoodle();
            var players = await this.blobClient.GetPlayers();

            if (doodles.Count == 0)
            {
                return NotFound("No doodle data found");
            }

            // Resolve target date
            DateTime targetDate;
            if (string.IsNullOrEmpty(date) || date == "first")
            {
                targetDate = doodles[0].GetSortedPlayersPoll().Keys.First();
            }
            else if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out targetDate))
            {
                return BadRequest("Invalid date format. Use yyyy-MM-dd");
            }

            // Get accepted player names from doodle for this date
            var acceptedPlayerNames = doodles
                .Where(d =>
                {
                    var matchingDate = d.PlayersPoll.Keys.FirstOrDefault(k => k.Date == targetDate.Date);
                    return matchingDate != default(DateTime) && d.PlayersPoll[matchingDate] == DoodleValue.Accept;
                })
                .Select(d => d.Player)
                .ToList();

            if (acceptedPlayerNames.Count < 2)
            {
                return BadRequest("Not enough accepted players to generate teams (minimum 2)");
            }

            if (acceptedPlayerNames.Count % 2 != 0)
            {
                return BadRequest($"Need even number of players, currently {acceptedPlayerNames.Count} accepted");
            }

            // Resolve player IDs
            var playerIds = players
                .Where(p => acceptedPlayerNames.Contains(p.Name))
                .Select(p => p.Id.ToString())
                .ToList();

            try
            {
                var generatorResults = await this.teamGenerator.GenerateTeams(playerIds, enumSeason);

                var responseDtos = generatorResults.Select(r => new GeneratorResultDto
                {
                    TeamOne = MapTeam(r.TeamOne, enumSeason),
                    TeamTwo = MapTeam(r.TeamTwo, enumSeason),
                    EloDiff = r.EloDiff,
                    Season = r.Season.ToString()
                }).ToList();

                var response = new
                {
                    results = responseDtos,
                    count = responseDtos.Count,
                    season = enumSeason.ToString(),
                    date = targetDate.ToString("yyyy-MM-dd"),
                    displayDate = targetDate.ToString("dd.MM"),
                    playerCount = acceptedPlayerNames.Count
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return ServerError($"Failed to generate teams: {ex.Message}");
            }
        }

        private static TeamDto MapTeam(Team team, Season season)
        {
            return new TeamDto
            {
                TeamElo = team.TeamElo,
                Players = team.Players.Select(p => new TeamPlayerDto
                {
                    Id = p.Id.ToString(),
                    Name = p.Name,
                    Elo = p.GetSeasonalElo(season),
                    OverallElo = p.Elo
                }).ToList()
            };
        }
    }

    // DTOs
    public class GeneratorResultDto
    {
        public TeamDto TeamOne { get; set; }
        public TeamDto TeamTwo { get; set; }
        public int EloDiff { get; set; }
        public string Season { get; set; }
    }

    public class TeamDto
    {
        public int TeamElo { get; set; }
        public List<TeamPlayerDto> Players { get; set; }
    }

    public class TeamPlayerDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Elo { get; set; }
        public int OverallElo { get; set; }
    }
}
