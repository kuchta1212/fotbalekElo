using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elo_fotbalek.EloCounter;
using Elo_fotbalek.Models;
using Elo_fotbalek.Storage;
using Elo_fotbalek.TrendCalculator;
using Microsoft.AspNetCore.Mvc;

namespace Elo_fotbalek.Controllers
{
    public class AdminController : Controller
    {
        private readonly IBlobClient blobClient;
        private readonly IEloCalulator eloCalulator;
        private readonly ITrendCalculator trendCalculator;


        public AdminController(IBlobClient blobClient, IEloCalulator eloCalulator, ITrendCalculator trendCalculator)
        {
            this.blobClient = blobClient;
            this.eloCalulator = eloCalulator;
            this.trendCalculator = trendCalculator;
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
                    newPlayerWinner.Elo += (int)eloResult.WinnerPointChange;
                }

                foreach (var player in match.Looser.Players)
                {
                    var newPlayerLooser = newPlayers.First(np => np.Id == player.Id);
                    newPlayerLooser.Elo += (int)eloResult.LooserPointChange;
                }

                await this.blobClient.UpdatePlayers(newPlayers);
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> CountWictoriesAndLooses()
        {
            var players = await this.blobClient.GetPlayers();

            var matches = await this.blobClient.GetMatches();

            foreach (var match in matches)
            {
                foreach (var player in players)
                {
                    if (match.Winner.Players.Contains(player))
                    {
                        if (player.AmountOfWins == null)
                        {
                            player.AmountOfWins = new MatchCounter();
                        }
                        if (match.Weight == 30)
                        {
                            player.AmountOfWins.BigMatches++;
                        }
                        else
                        {
                            player.AmountOfWins.SmallMatches++;
                        }
                        
                        
                    }
                    else if (match.Looser.Players.Contains(player))
                    {
                        if (player.AmountOfLooses == null)
                        {
                            player.AmountOfLooses = new MatchCounter();
                        }
                        if (match.Weight == 30)
                        {
                            player.AmountOfLooses.BigMatches++;
                        }
                        else
                        {
                            player.AmountOfLooses.SmallMatches++;
                        }
                    }
                }
                
            }

            await this.blobClient.UpdatePlayers(players);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> AddTrendData()
        {
            var matches = await this.blobClient.GetMatches();
            var matchesOrdered = matches.OrderByDescending(m => m.Date);

            var players = await this.blobClient.GetPlayers();

            foreach (var player in players)
            {
                player.Trend = new TrendData()
                {
                    Data = new Dictionary<DateTime, int>(),
                    Trend = Trend.STAY
                };

                var matchCount = 0;

                foreach (var match in matchesOrdered)
                {
                    if (matchCount < 5)
                    {
                        if (match.Winner.Players.Contains(player))
                        {
                            player.Trend = this.trendCalculator.CalculateTrend(player.Trend, match.Date, true);
                            matchCount++;
                        }
                        else if (match.Looser.Players.Contains(player))
                        {
                            player.Trend = this.trendCalculator.CalculateTrend(player.Trend, match.Date, false);
                            matchCount++;
                        }
                    }                   
                }
            }

            await this.blobClient.UpdatePlayers(players);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> IntroduceSeasonalElo()
        {
            var players = await this.blobClient.GetPlayers();

            foreach (var player in players)
            {
                player.Elos = new SeasonalElos
                {
                    SummerElo = player.Elo,
                    WinterElo = 1000
                };
            }

            await this.blobClient.UpdatePlayers(players);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> CalculateMissingMatches()
        {
            var players = await this.blobClient.GetPlayers();
            var matches = await this.blobClient.GetMatches();

            var sortedMatches = matches.OrderBy(x => x.Date);
            foreach (var match in sortedMatches)
            {
                foreach (var player in players)
                {
                    if (!match.Winner.Players.Contains(player) && !match.Looser.Players.Contains(player))
                    {
                        player.AmountOfMissedGames++;
                    }
                    else
                    {
                        player.AmountOfMissedGames = 0;
                    }
                }    
            }

            await this.blobClient.UpdatePlayers(players);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> ReCalculateTrend()
        {
            var players = await this.blobClient.GetPlayers();

            foreach (var player in players)
            {
                if (player.AmountOfMissedGames >= 5)
                {
                    player.Trend.Trend = Trend.STAY;
                }
            }

            await this.blobClient.UpdatePlayers(players);

            return RedirectToAction("Index", "Home");
        }
    }
}