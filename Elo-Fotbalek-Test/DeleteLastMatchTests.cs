using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elo_fotbalek.Configuration;
using Elo_fotbalek.Controllers.Api;
using Elo_fotbalek.EloCounter;
using Elo_fotbalek.Models;
using Elo_fotbalek.Storage;
using Match = Elo_fotbalek.Models.Match;
using Elo_fotbalek.TrendCalculator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Tests
{
    public class DeleteLastMatchTests
    {
        private Mock<IBlobClient> mockBlobClient;
        private Mock<IEloCalulator> mockEloCalculator;
        private Mock<ITrendCalculator> mockTrendCalculator;
        private Mock<IModelCreator> mockModelCreator;
        private Mock<IOptions<AppConfigurationOptions>> mockAppConfig;
        private MatchesApiController controller;

        private static readonly Guid Player1Id = Guid.NewGuid();
        private static readonly Guid Player2Id = Guid.NewGuid();
        private static readonly Guid Player3Id = Guid.NewGuid();
        private static readonly Guid NonPlayerId = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            mockBlobClient = new Mock<IBlobClient>();
            mockEloCalculator = new Mock<IEloCalulator>();
            mockTrendCalculator = new Mock<ITrendCalculator>();
            mockModelCreator = new Mock<IModelCreator>();
            mockAppConfig = new Mock<IOptions<AppConfigurationOptions>>();

            mockAppConfig.Setup(x => x.Value).Returns(new AppConfigurationOptions
            {
                AmountOfMonthsToBeCounted = 6
            });

            mockTrendCalculator
                .Setup(x => x.RecalculateTrend(It.IsAny<TrendData>()))
                .Returns((TrendData td) => td);

            controller = new MatchesApiController(
                mockBlobClient.Object,
                mockModelCreator.Object,
                mockEloCalculator.Object,
                mockTrendCalculator.Object,
                mockAppConfig.Object);
        }

        private Match CreateTestMatch(
            int winnerScore = 5,
            int loserScore = 3,
            int weight = 30,
            Season season = Season.Summer,
            DateTime? date = null)
        {
            return new Match
            {
                Date = date ?? DateTime.Now,
                WinnerAmount = winnerScore,
                LooserAmount = loserScore,
                Weight = weight,
                Season = season,
                Winner = new Team
                {
                    Players = new List<Player>
                    {
                        new Player { Id = Player1Id, Name = "P1", Elo = 1050, Elos = new SeasonalElos { SummerElo = 1050 }, Trend = new TrendData { Data = new Dictionary<DateTime, int>(), Trend = Trend.STAY }, AmountOfWins = new MatchCounter(), AmountOfLooses = new MatchCounter(), AmountOfTies = new MatchCounter() }
                    },
                    TeamElo = 1050
                },
                Looser = new Team
                {
                    Players = new List<Player>
                    {
                        new Player { Id = Player2Id, Name = "P2", Elo = 950, Elos = new SeasonalElos { SummerElo = 950 }, Trend = new TrendData { Data = new Dictionary<DateTime, int>(), Trend = Trend.STAY }, AmountOfWins = new MatchCounter(), AmountOfLooses = new MatchCounter(), AmountOfTies = new MatchCounter() }
                    },
                    TeamElo = 950
                }
            };
        }

        private List<Player> CreatePlayersFromMatch(Match match, Player nonPlayer = null)
        {
            var players = new List<Player>();
            players.AddRange(match.Winner.Players.Select(p => new Player
            {
                Id = p.Id, Name = p.Name, Elo = p.Elo,
                Elos = new SeasonalElos { SummerElo = p.Elos.SummerElo, WinterElo = p.Elos.WinterElo },
                Trend = new TrendData { Data = new Dictionary<DateTime, int>(p.Trend.Data), Trend = p.Trend.Trend },
                AmountOfWins = new MatchCounter { BigMatches = p.AmountOfWins.BigMatches, SmallMatches = p.AmountOfWins.SmallMatches },
                AmountOfLooses = new MatchCounter { BigMatches = p.AmountOfLooses.BigMatches, SmallMatches = p.AmountOfLooses.SmallMatches },
                AmountOfTies = new MatchCounter { BigMatches = p.AmountOfTies.BigMatches, SmallMatches = p.AmountOfTies.SmallMatches },
                AmountOfMissedGames = p.AmountOfMissedGames
            }));
            players.AddRange(match.Looser.Players.Select(p => new Player
            {
                Id = p.Id, Name = p.Name, Elo = p.Elo,
                Elos = new SeasonalElos { SummerElo = p.Elos.SummerElo, WinterElo = p.Elos.WinterElo },
                Trend = new TrendData { Data = new Dictionary<DateTime, int>(p.Trend.Data), Trend = p.Trend.Trend },
                AmountOfWins = new MatchCounter { BigMatches = p.AmountOfWins.BigMatches, SmallMatches = p.AmountOfWins.SmallMatches },
                AmountOfLooses = new MatchCounter { BigMatches = p.AmountOfLooses.BigMatches, SmallMatches = p.AmountOfLooses.SmallMatches },
                AmountOfTies = new MatchCounter { BigMatches = p.AmountOfTies.BigMatches, SmallMatches = p.AmountOfTies.SmallMatches },
                AmountOfMissedGames = p.AmountOfMissedGames
            }));
            if (nonPlayer != null) players.Add(nonPlayer);
            return players;
        }

        private void SetupMocks(Match match, FifaEloResult eloResult, List<Player> players)
        {
            mockBlobClient.Setup(x => x.GetMatches()).ReturnsAsync(new List<Match> { match });
            mockEloCalculator.Setup(x => x.CalculateFifaElo(match)).Returns(eloResult);
            mockBlobClient.Setup(x => x.GetPlayers()).ReturnsAsync(players);
            mockBlobClient.Setup(x => x.UpdatePlayers(It.IsAny<List<Player>>())).Returns(Task.CompletedTask);
            mockBlobClient.Setup(x => x.RemoveMatch(match)).Returns(Task.CompletedTask);
        }

        [Test]
        public async Task DeleteLastMatch_ReversesWinnerAndLoserElo()
        {
            var match = CreateTestMatch();
            var eloResult = new FifaEloResult { WinnerPointChange = 15, LooserPointChange = -15 };
            var players = CreatePlayersFromMatch(match);
            players.First(p => p.Id == Player1Id).Elos.SummerElo = 1065; // after match
            players.First(p => p.Id == Player2Id).Elos.SummerElo = 935;  // after match
            SetupMocks(match, eloResult, players);

            var result = await controller.DeleteLastMatch();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            mockBlobClient.Verify(x => x.UpdatePlayers(It.Is<List<Player>>(pl =>
                pl.First(p => p.Id == Player1Id).Elos.SummerElo == 1050 &&
                pl.First(p => p.Id == Player2Id).Elos.SummerElo == 950
            )), Times.AtLeastOnce());
        }

        [Test]
        public async Task DeleteLastMatch_DecrementsWinCounters_BigMatch()
        {
            var match = CreateTestMatch(winnerScore: 5, loserScore: 3, weight: 30);
            var eloResult = new FifaEloResult { WinnerPointChange = 10, LooserPointChange = -10 };
            var players = CreatePlayersFromMatch(match);
            players.First(p => p.Id == Player1Id).AmountOfWins.BigMatches = 3;
            SetupMocks(match, eloResult, players);

            await controller.DeleteLastMatch();

            mockBlobClient.Verify(x => x.UpdatePlayers(It.Is<List<Player>>(pl =>
                pl.First(p => p.Id == Player1Id).AmountOfWins.BigMatches == 2
            )), Times.AtLeastOnce());
        }

        [Test]
        public async Task DeleteLastMatch_DecrementsLossCounters_BigMatch()
        {
            var match = CreateTestMatch(winnerScore: 5, loserScore: 3, weight: 30);
            var eloResult = new FifaEloResult { WinnerPointChange = 10, LooserPointChange = -10 };
            var players = CreatePlayersFromMatch(match);
            players.First(p => p.Id == Player2Id).AmountOfLooses.BigMatches = 5;
            SetupMocks(match, eloResult, players);

            await controller.DeleteLastMatch();

            mockBlobClient.Verify(x => x.UpdatePlayers(It.Is<List<Player>>(pl =>
                pl.First(p => p.Id == Player2Id).AmountOfLooses.BigMatches == 4
            )), Times.AtLeastOnce());
        }

        [Test]
        public async Task DeleteLastMatch_DecrementsTieCounters()
        {
            var match = CreateTestMatch(winnerScore: 3, loserScore: 3, weight: 30);
            var eloResult = new FifaEloResult { WinnerPointChange = 0, LooserPointChange = 0 };
            var players = CreatePlayersFromMatch(match);
            players.First(p => p.Id == Player1Id).AmountOfTies.BigMatches = 2;
            players.First(p => p.Id == Player2Id).AmountOfTies.BigMatches = 2;
            SetupMocks(match, eloResult, players);

            await controller.DeleteLastMatch();

            mockBlobClient.Verify(x => x.UpdatePlayers(It.Is<List<Player>>(pl =>
                pl.First(p => p.Id == Player1Id).AmountOfTies.BigMatches == 1 &&
                pl.First(p => p.Id == Player2Id).AmountOfTies.BigMatches == 1
            )), Times.AtLeastOnce());
        }

        [Test]
        public async Task DeleteLastMatch_ReversesNonCommersPunishment()
        {
            var match = CreateTestMatch();
            var eloResult = new FifaEloResult { WinnerPointChange = 10, LooserPointChange = -10 };
            var nonPlayer = new Player
            {
                Id = NonPlayerId, Name = "NonPlayer", Elo = 1000,
                Elos = new SeasonalElos { SummerElo = 1000 },
                Trend = new TrendData { Data = new Dictionary<DateTime, int>(), Trend = Trend.STAY },
                AmountOfWins = new MatchCounter(), AmountOfLooses = new MatchCounter(), AmountOfTies = new MatchCounter(),
                AmountOfMissedGames = 3
            };
            var players = CreatePlayersFromMatch(match, nonPlayer);
            SetupMocks(match, eloResult, players);

            await controller.DeleteLastMatch();

            mockBlobClient.Verify(x => x.UpdatePlayers(It.Is<List<Player>>(pl =>
                pl.First(p => p.Id == NonPlayerId).AmountOfMissedGames == 2
            )), Times.AtLeastOnce());
        }

        [Test]
        public async Task DeleteLastMatch_RemovesTrendEntry()
        {
            var matchDate = new DateTime(2024, 6, 15, 14, 0, 0);
            var match = CreateTestMatch(date: matchDate);
            var eloResult = new FifaEloResult { WinnerPointChange = 10, LooserPointChange = -10 };
            var players = CreatePlayersFromMatch(match);
            // Add a trend entry at the match date
            players.First(p => p.Id == Player1Id).Trend.Data[matchDate] = 1;
            players.First(p => p.Id == Player2Id).Trend.Data[matchDate] = -1;
            SetupMocks(match, eloResult, players);

            await controller.DeleteLastMatch();

            mockBlobClient.Verify(x => x.UpdatePlayers(It.Is<List<Player>>(pl =>
                pl.First(p => p.Id == Player1Id).Trend.Data.Count == 0 &&
                pl.First(p => p.Id == Player2Id).Trend.Data.Count == 0
            )), Times.AtLeastOnce());
        }

        [Test]
        public async Task DeleteLastMatch_NoMatches_ReturnsBadRequest()
        {
            mockBlobClient.Setup(x => x.GetMatches()).ReturnsAsync(new List<Match>());

            var result = await controller.DeleteLastMatch();

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task DeleteLastMatch_RemovesMatchFromStorage()
        {
            var match = CreateTestMatch();
            var eloResult = new FifaEloResult { WinnerPointChange = 10, LooserPointChange = -10 };
            var players = CreatePlayersFromMatch(match);
            SetupMocks(match, eloResult, players);

            await controller.DeleteLastMatch();

            mockBlobClient.Verify(x => x.RemoveMatch(match), Times.Once());
        }
    }
}
