using Elo_fotbalek.Models;
using Elo_fotbalek.Storage;
using Microsoft.AspNetCore.Mvc;

namespace Elo_fotbalek.Controllers.Api
{
    [Route("api/players")]
    public class PlayersApiController : BaseApiController
    {
        private readonly IBlobClient blobClient;

        public PlayersApiController(IBlobClient blobClient)
        {
            this.blobClient = blobClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetPlayers()
        {
            try
            {
                var players = await blobClient.GetPlayers();
                
                var response = new
                {
                    players = players.Select(p => new
                    {
                        id = p.Id.ToString(),
                        name = p.Name,
                        elo = p.Elo,
                        winterElo = p.Elos?.WinterElo ?? p.Elo,
                        summerElo = p.Elos?.SummerElo ?? p.Elo,
                        overallElo = p.Elo,
                        matchesPlayed = (p.AmountOfWins?.TotalAmount() ?? 0) + (p.AmountOfLooses?.TotalAmount() ?? 0) + (p.AmountOfTies?.TotalAmount() ?? 0),
                        wins = p.AmountOfWins?.TotalAmount() ?? 0,
                        losses = p.AmountOfLooses?.TotalAmount() ?? 0,
                        isRegular = p.Percentage > 0
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return ServerError($"Failed to get players: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlayer(string id)
        {
            try
            {
                if (!Guid.TryParse(id, out var playerId))
                {
                    return BadRequest("Invalid player ID format");
                }

                var players = await blobClient.GetPlayers();
                var player = players.FirstOrDefault(p => p.Id == playerId);

                if (player == null)
                {
                    return NotFound($"Player with ID {id} not found");
                }

                // Get all matches for this player
                var allMatches = await blobClient.GetMatches();
                var playerMatches = allMatches
                    .Where(m => m.Winner.Players.Any(p => p.Id == playerId) || m.Looser.Players.Any(p => p.Id == playerId))
                    .OrderBy(m => m.Date)
                    .ToList();

                // Build Elo history
                var eloHistory = new List<object>();
                var maxElo = player.Elo;
                var minElo = player.Elo;

                if (playerMatches.Any())
                {
                    // Add initial point (before first match)
                    var firstMatch = playerMatches.First();
                    var firstMatchPlayer = firstMatch.Winner.Players.FirstOrDefault(p => p.Id == playerId) 
                                         ?? firstMatch.Looser.Players.FirstOrDefault(p => p.Id == playerId);
                    
                    if (firstMatchPlayer != null)
                    {
                        eloHistory.Add(new
                        {
                            date = firstMatch.Date.AddDays(-1),
                            elo = firstMatchPlayer.Elo
                        });
                        minElo = Math.Min(minElo, firstMatchPlayer.Elo);
                        maxElo = Math.Max(maxElo, firstMatchPlayer.Elo);
                    }

                    // Add point for each match
                    foreach (var match in playerMatches)
                    {
                        var matchPlayer = match.Winner.Players.FirstOrDefault(p => p.Id == playerId) 
                                       ?? match.Looser.Players.FirstOrDefault(p => p.Id == playerId);
                        
                        if (matchPlayer != null)
                        {
                            eloHistory.Add(new
                            {
                                date = match.Date,
                                elo = matchPlayer.Elo
                            });
                            minElo = Math.Min(minElo, matchPlayer.Elo);
                            maxElo = Math.Max(maxElo, matchPlayer.Elo);
                        }
                    }

                    // Add current point
                    eloHistory.Add(new
                    {
                        date = DateTime.Now,
                        elo = player.Elo
                    });
                }

                var response = new
                {
                    player = new
                    {
                        id = player.Id.ToString(),
                        name = player.Name,
                        elo = player.Elo,
                        winterElo = player.Elos?.WinterElo ?? player.Elo,
                        summerElo = player.Elos?.SummerElo ?? player.Elo,
                        overallElo = player.Elo,
                        matchesPlayed = playerMatches.Count,
                        wins = player.AmountOfWins?.TotalAmount() ?? 0,
                        losses = player.AmountOfLooses?.TotalAmount() ?? 0,
                        ties = player.AmountOfTies?.TotalAmount() ?? 0,
                        percentage = player.Percentage,
                        totalPercentage = player.TotalPercentage,
                        isRegular = player.Percentage >= 30
                    },
                    stats = new
                    {
                        highestElo = maxElo,
                        lowestElo = minElo,
                        currentElo = new
                        {
                            winter = player.Elos?.WinterElo ?? player.Elo,
                            summer = player.Elos?.SummerElo ?? player.Elo,
                            overall = player.Elo
                        },
                        eloHistory = eloHistory
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return ServerError($"Failed to get player: {ex.Message}");
            }
        }
    }
}
