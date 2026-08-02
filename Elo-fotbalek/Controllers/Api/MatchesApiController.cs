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

        [HttpGet]
        public async Task<IActionResult> GetMatches([FromQuery] int? year, [FromQuery] int? month)
        {
            try
            {
                var allMatches = await this.blobClient.GetMatches();
                if (allMatches == null || allMatches.Count == 0)
                {
                    return Ok(new
                    {
                        matches = new List<object>(),
                        year = year ?? DateTime.Now.Year,
                        month = month ?? DateTime.Now.Month,
                        hasMore = false
                    });
                }

                var ordered = allMatches.OrderByDescending(m => m.Date).ToList();

                int targetYear;
                int targetMonth;

                if (year.HasValue && month.HasValue)
                {
                    targetYear = year.Value;
                    targetMonth = month.Value;
                }
                else
                {
                    // Default to the most recent month that has matches
                    targetYear = ordered.First().Date.Year;
                    targetMonth = ordered.First().Date.Month;
                }

                var monthMatches = ordered
                    .Where(m => m.Date.Year == targetYear && m.Date.Month == targetMonth)
                    .ToList();

                var hasMore = ordered.Any(m =>
                    m.Date.Year < targetYear ||
                    (m.Date.Year == targetYear && m.Date.Month < targetMonth));

                var matchDtos = monthMatches.Select(m => new
                {
                    id = $"{m.Date:yyyy-MM-dd}_{m.Score}",
                    date = m.Date,
                    score = m.Score,
                    season = m.Season.ToString(),
                    isSmallMatch = m.Weight == 10,
                    winner = new
                    {
                        teamElo = m.Winner.TeamElo,
                        players = m.Winner.Players.Select(wp => new
                        {
                            id = wp.Id.ToString(),
                            name = wp.Name,
                            elo = wp.Elo
                        }).ToList()
                    },
                    loser = new
                    {
                        teamElo = m.Looser.TeamElo,
                        players = m.Looser.Players.Select(lp => new
                        {
                            id = lp.Id.ToString(),
                            name = lp.Name,
                            elo = lp.Elo
                        }).ToList()
                    },
                    jirkaLunak = !string.IsNullOrEmpty(m.Hero) ? m.Hero : null
                }).ToList();

                return Ok(new
                {
                    matches = matchDtos,
                    year = targetYear,
                    month = targetMonth,
                    hasMore
                });
            }
            catch (Exception ex)
            {
                return ServerError($"Failed to get matches: {ex.Message}");
            }
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
                    Date = request.Date ?? DateTime.Now,
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

        [HttpDelete("last")]
        [Authorize(policy: "MyPolicy")]
        public async Task<IActionResult> DeleteLastMatch()
        {
            try
            {
                var matches = await this.blobClient.GetMatches();
                if (matches == null || matches.Count == 0)
                {
                    return BadRequest("No matches to delete");
                }

                var lastMatch = matches.OrderByDescending(m => m.Date).First();
                var eloResult = this.eloCalculator.CalculateFifaElo(lastMatch);

                await this.ReversePlayersElo(eloResult, lastMatch);
                await this.ReverseNonCommersPunishment(lastMatch);
                await this.blobClient.RemoveMatch(lastMatch);
                await this.RecalculatePercentage();

                return Ok(new
                {
                    message = "Last match deleted successfully",
                    deletedMatchDate = lastMatch.Date.ToString("o"),
                    deletedMatchScore = lastMatch.Score
                });
            }
            catch (Exception ex)
            {
                return ServerError($"Failed to delete last match: {ex.Message}");
            }
        }

        private async Task ReversePlayersElo(FifaEloResult eloResult, Match match)
        {
            var players = await this.blobClient.GetPlayers();

            foreach (var player in match.Winner.Players)
            {
                var current = players.First(np => np.Id == player.Id);
                current.UpdateElo(-(int)eloResult.WinnerPointChange, match.Season);
                current.Elo = Util.CountGeneralElo(current.Elos);

                if (match.WinnerAmount == match.LooserAmount)
                {
                    current.AmountOfTies ??= new MatchCounter();
                    if (match.Weight == 30) current.AmountOfTies.BigMatches--;
                    else current.AmountOfTies.SmallMatches--;
                }
                else
                {
                    current.AmountOfWins ??= new MatchCounter();
                    if (match.Weight == 30) current.AmountOfWins.BigMatches--;
                    else current.AmountOfWins.SmallMatches--;
                }

                this.RemoveTrendEntry(current, match.Date);
                this.trendCalculator.RecalculateTrend(current.Trend);
            }

            foreach (var player in match.Looser.Players)
            {
                var current = players.First(np => np.Id == player.Id);
                current.UpdateElo(-(int)eloResult.LooserPointChange, match.Season);
                current.Elo = Util.CountGeneralElo(current.Elos);

                if (match.WinnerAmount == match.LooserAmount)
                {
                    current.AmountOfTies ??= new MatchCounter();
                    if (match.Weight == 30) current.AmountOfTies.BigMatches--;
                    else current.AmountOfTies.SmallMatches--;
                }
                else
                {
                    current.AmountOfLooses ??= new MatchCounter();
                    if (match.Weight == 30) current.AmountOfLooses.BigMatches--;
                    else current.AmountOfLooses.SmallMatches--;
                }

                this.RemoveTrendEntry(current, match.Date);
                this.trendCalculator.RecalculateTrend(current.Trend);
            }

            await this.blobClient.UpdatePlayers(players);
        }

        private async Task ReverseNonCommersPunishment(Match match)
        {
            var players = await this.blobClient.GetPlayers();
            var todaysPlayers = match.Looser.Players.Union(match.Winner.Players);

            var nonCommers = players.Except(todaysPlayers);
            foreach (var nonCommer in nonCommers)
            {
                nonCommer.AmountOfMissedGames = Math.Max(0, nonCommer.AmountOfMissedGames - 1);
                this.trendCalculator.RecalculateTrend(nonCommer.Trend);
            }

            await this.blobClient.UpdatePlayers(players);
        }

        private void RemoveTrendEntry(Player player, DateTime matchDate)
        {
            if (player.Trend?.Data == null || player.Trend.Data.Count == 0) return;

            // Find the trend entry matching the match date (within 10 min window for collision offsets)
            var matchingKey = player.Trend.Data.Keys
                .Where(k => Math.Abs((k - matchDate).TotalMinutes) <= 10)
                .OrderBy(k => Math.Abs((k - matchDate).TotalMinutes))
                .FirstOrDefault();

            if (matchingKey != default)
            {
                player.Trend.Data.Remove(matchingKey);
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
        public string? HeroId { get; set; }
        public DateTime? Date { get; set; }
    }

    public class MatchPlayerOptionDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
