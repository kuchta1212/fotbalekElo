namespace Elo_fotbalek.Controllers.Api
{
    using Elo_fotbalek.Configuration;
    using Elo_fotbalek.EloCounter;
    using Elo_fotbalek.Models;
    using Elo_fotbalek.Storage;
    using Elo_fotbalek.TrendCalculator;
    using Elo_fotbalek.Utils;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/matches")]
    public class MatchesApiController : BaseApiController
    {
        private readonly IBlobClient blobClient;
        private readonly IModelCreator modelCreator;
        private readonly IEloCalulator eloCalculator;
        private readonly ITrendCalculator trendCalculator;
        private readonly IOptions<AppConfigurationOptions> appConfiguration;

        public MatchesApiController(
            IBlobClient blobClient,
            IModelCreator modelCreator,
            IEloCalulator eloCalculator,
            ITrendCalculator trendCalculator,
            IOptions<AppConfigurationOptions> appConfiguration)
        {
            this.blobClient = blobClient;
            this.modelCreator = modelCreator;
            this.eloCalculator = eloCalculator;
            this.trendCalculator = trendCalculator;
            this.appConfiguration = appConfiguration;
        }

        [HttpGet("players")]
        public async Task<IActionResult> GetPlayersForMatch()
        {
            var players = await this.blobClient.GetPlayers();

            var playerDtos = players
                .OrderBy(p => p.Name)
                .Select(p => new MatchPlayerOptionDto
                {
                    Id = p.Id.ToString(),
                    Name = p.Name
                })
                .ToList();

            return Ok(new { players = playerDtos });
        }

        [HttpPost]
        [Authorize(policy: "MyPolicy")]
        public async Task<IActionResult> AddMatch([FromBody] AddMatchRequestDto request)
        {
            if (request.WinnerPlayerIds == null || request.WinnerPlayerIds.Count == 0)
            {
                return BadRequest("At least one winner player is required");
            }

            if (request.LoserPlayerIds == null || request.LoserPlayerIds.Count == 0)
            {
                return BadRequest("At least one loser player is required");
            }

            if (request.WinnerScore < 0 || request.LoserScore < 0)
            {
                return BadRequest("Scores must be non-negative");
            }

            if (!Enum.TryParse<Season>(request.Season, out var season))
            {
                season = Season.Summer;
            }

            try
            {
                var enumSeason = this.appConfiguration.Value.IsSeasoningSupported
                    ? season
                    : Season.Summer;

                var winnerTeam = await this.modelCreator.CreateTeam(
                    request.WinnerPlayerIds.Where(id => Guid.Parse(id) != Guid.Empty),
                    enumSeason);

                var loserTeam = await this.modelCreator.CreateTeam(
                    request.LoserPlayerIds.Where(id => Guid.Parse(id) != Guid.Empty),
                    enumSeason);

                var heroName = string.Empty;
                if (!string.IsNullOrEmpty(request.HeroId) && Guid.Parse(request.HeroId) != Guid.Empty)
                {
                    var players = await this.blobClient.GetPlayers();
                    var heroPlayer = players.FirstOrDefault(p => p.Id == Guid.Parse(request.HeroId));
                    heroName = heroPlayer?.Name ?? string.Empty;
                }

                var weight = request.Weight == "SmallMatch" ? 10 : 30;

                var match = new Match
                {
                    Date = DateTime.Now,
                    WinnerAmount = request.WinnerScore,
                    LooserAmount = request.LoserScore,
                    Winner = winnerTeam,
                    Looser = loserTeam,
                    Weight = weight,
                    Season = enumSeason,
                    Hero = heroName
                };

                await this.blobClient.AddMatch(match);

                var eloResult = this.eloCalculator.CalculateFifaElo(match);

                await this.UpdatePlayersElo(eloResult, match);
                await this.PunishNonCommers(match);
                await this.RecalculatePercentage();

                return Ok(new
                {
                    message = "Match added successfully",
                    score = match.Score,
                    winnerEloChange = (int)eloResult.WinnerPointChange,
                    loserEloChange = (int)eloResult.LooserPointChange
                });
            }
            catch (Exception ex)
            {
                return ServerError($"Failed to add match: {ex.Message}");
            }
        }

        private async Task UpdatePlayersElo(FifaEloResult eloResult, Match match)
        {
            var players = await this.blobClient.GetPlayers();

            foreach (var player in match.Winner.Players)
            {
                var current = players.First(np => np.Id == player.Id);
                current.UpdateElo((int)eloResult.WinnerPointChange, match.Season);
                current.Elo = Util.CountGeneralElo(current.Elos);

                if (match.WinnerAmount == match.LooserAmount)
                {
                    current.Trend = this.trendCalculator.CalculateTrend(player.Trend, match.Date, 0);
                    current.AmountOfTies ??= new MatchCounter();
                    if (match.Weight == 30) current.AmountOfTies.BigMatches++;
                    else current.AmountOfTies.SmallMatches++;
                }
                else
                {
                    current.Trend = this.trendCalculator.CalculateTrend(player.Trend, match.Date, 1);
                    current.AmountOfWins ??= new MatchCounter();
                    if (match.Weight == 30) current.AmountOfWins.BigMatches++;
                    else current.AmountOfWins.SmallMatches++;
                }

                current.AmountOfMissedGames = 0;
            }

            foreach (var player in match.Looser.Players)
            {
                var current = players.First(np => np.Id == player.Id);
                current.UpdateElo((int)eloResult.LooserPointChange, match.Season);
                current.Elo = Util.CountGeneralElo(current.Elos);

                if (match.WinnerAmount == match.LooserAmount)
                {
                    current.Trend = this.trendCalculator.CalculateTrend(player.Trend, match.Date, 0);
                    current.AmountOfTies ??= new MatchCounter();
                    if (match.Weight == 30) current.AmountOfTies.BigMatches++;
                    else current.AmountOfTies.SmallMatches++;
                }
                else
                {
                    current.Trend = this.trendCalculator.CalculateTrend(player.Trend, match.Date, -1);
                    current.AmountOfLooses ??= new MatchCounter();
                    if (match.Weight == 30) current.AmountOfLooses.BigMatches++;
                    else current.AmountOfLooses.SmallMatches++;
                }

                current.AmountOfMissedGames = 0;
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

            var countedMatches = matches
                .Where(m => m.Date >= DateTime.Now.AddMonths(-this.appConfiguration.Value.AmountOfMonthsToBeCounted))
                .ToList();

            var playersPercInSelectedPeriod = new Dictionary<string, int>();
            foreach (var countedMatch in countedMatches)
            {
                foreach (var player in countedMatch.GetAllPlayers())
                {
                    var key = player.Id.ToString();
                    if (playersPercInSelectedPeriod.ContainsKey(key))
                        playersPercInSelectedPeriod[key]++;
                    else
                        playersPercInSelectedPeriod.Add(key, 1);
                }
            }

            foreach (var player in players)
            {
                var totalAmountOfPlayedMatches = (player.AmountOfLooses?.TotalAmount() ?? 0) + (player.AmountOfWins?.TotalAmount() ?? 0);
                player.TotalPercentage = (int)((double)totalAmountOfPlayedMatches / matches.Count * 100);
                player.Percentage = playersPercInSelectedPeriod.ContainsKey(player.Id.ToString())
                    ? (int)((double)playersPercInSelectedPeriod[player.Id.ToString()] / countedMatches.Count * 100)
                    : 0;
            }

            await this.blobClient.UpdatePlayers(players);
        }
    }

    public class AddMatchRequestDto
    {
        public List<string> WinnerPlayerIds { get; set; }
        public List<string> LoserPlayerIds { get; set; }
        public int WinnerScore { get; set; }
        public int LoserScore { get; set; }
        public string Weight { get; set; } = "BigMatch";
        public string Season { get; set; } = "Summer";
        public string HeroId { get; set; }
    }

    public class MatchPlayerOptionDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
