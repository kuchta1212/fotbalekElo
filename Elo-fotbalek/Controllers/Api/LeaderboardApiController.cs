using Elo_fotbalek.Configuration;
using Elo_fotbalek.Models;
using Elo_fotbalek.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Elo_fotbalek.Controllers.Api
{
    [Route("api/leaderboards")]
    public class LeaderboardApiController : BaseApiController
    {
        private readonly IBlobClient blobClient;
        private readonly IOptions<AppConfigurationOptions> appConfiguration;

        public LeaderboardApiController(
            IBlobClient blobClient,
            IOptions<AppConfigurationOptions> appConfiguration)
        {
            this.blobClient = blobClient;
            this.appConfiguration = appConfiguration;
        }

        [HttpGet]
        public async Task<IActionResult> GetLeaderboard([FromQuery] string? season)
        {
            try
            {
                var players = await blobClient.GetPlayers();
                var matches = await blobClient.GetMatches(
                    DateTime.Now.AddMonths(-appConfiguration.Value.AmountOfMonthsToBeCounted));

                // Split into regulars (>=30% attendance) and non-regulars
                var regulars = players.Where(p => p.Percentage >= 30).ToList();
                var nonRegulars = players.Where(p => p.Percentage < 30).ToList();

                // Sort based on season parameter
                IEnumerable<Player> sortedRegulars;
                IEnumerable<Player> sortedNonRegulars;

                switch (season?.ToLower())
                {
                    case "summer":
                        sortedRegulars = regulars.OrderByDescending(p => p.Elos?.SummerElo ?? p.Elo);
                        sortedNonRegulars = nonRegulars.OrderByDescending(p => p.Elos?.SummerElo ?? p.Elo);
                        break;
                    case "winter":
                        sortedRegulars = regulars.OrderByDescending(p => p.Elos?.WinterElo ?? p.Elo);
                        sortedNonRegulars = nonRegulars.OrderByDescending(p => p.Elos?.WinterElo ?? p.Elo);
                        break;
                    default: // "overall" or null
                        sortedRegulars = regulars.OrderByDescending(p => p.Elo);
                        sortedNonRegulars = nonRegulars.OrderByDescending(p => p.Elo);
                        break;
                }

                // Map to response DTOs
                var response = new
                {
                    regulars = sortedRegulars.Select((p, index) => MapPlayerToLeaderboardEntry(p, index + 1, season)).ToList(),
                    nonRegulars = sortedNonRegulars.Select((p, index) => MapPlayerToLeaderboardEntry(p, index + 1, season)).ToList(),
                    recentMatches = matches.OrderByDescending(m => m.Date).Take(10).Select(m => new
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
                    }).ToList(),
                    season = season ?? "overall",
                    isSeasoningSupported = appConfiguration.Value.IsSeasoningSupported,
                    nonRegularsTitle = appConfiguration.Value.NonRegularsTitle
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return ServerError($"Failed to get leaderboard: {ex.Message}");
            }
        }

        private object MapPlayerToLeaderboardEntry(Player player, int rank, string? season)
        {
            var currentElo = season?.ToLower() switch
            {
                "summer" => player.Elos?.SummerElo ?? player.Elo,
                "winter" => player.Elos?.WinterElo ?? player.Elo,
                _ => player.Elo
            };

            return new
            {
                rank,
                id = player.Id.ToString(),
                name = player.Name,
                elo = currentElo,
                overallElo = player.Elo,
                winterElo = player.Elos?.WinterElo ?? player.Elo,
                summerElo = player.Elos?.SummerElo ?? player.Elo,
                trend = player.Trend?.Trend.ToString().ToLower() ?? "stay",
                wins = player.AmountOfWins?.TotalAmount() ?? 0,
                losses = player.AmountOfLooses?.TotalAmount() ?? 0,
                ties = player.AmountOfTies?.TotalAmount() ?? 0,
                percentage = player.Percentage,
                totalPercentage = player.TotalPercentage
            };
        }
    }
}
