using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elo_fotbalek.Models;
using Elo_fotbalek.TeamGenerator;
using Moq;
using NUnit.Framework;

namespace Tests
{
    public class TeamGeneratorTests
    {
        private static Player MakePlayer(int i)
        {
            return new Player
            {
                Id = Guid.NewGuid(),
                Name = $"P{i}",
                Elo = 1000 + i * 10,
                Elos = new SeasonalElos { SummerElo = 1000 + i * 10, WinterElo = 1000 + i * 10 },
                Percentage = 80,
                TotalPercentage = 80,
            };
        }

        private static (TeamGenerator generator, List<string> ids) BuildGeneratorForPlayers(int count)
        {
            var players = Enumerable.Range(0, count).Select(MakePlayer).ToList();
            var ids = players.Select(p => p.Id.ToString()).ToList();

            var modelCreatorMock = new Mock<IModelCreator>();
            modelCreatorMock
                .Setup(m => m.CreateTeam(It.Is<IEnumerable<string>>(passed => passed.Count() == count), It.IsAny<Season>()))
                .ReturnsAsync(new Team { Players = players, TeamElo = 0 });
            modelCreatorMock
                .Setup(m => m.CreateTeam(It.Is<IEnumerable<string>>(passed => !passed.Any()), It.IsAny<Season>()))
                .ReturnsAsync(new Team { Players = new List<Player>(), TeamElo = 0 });

            return (new TeamGenerator(modelCreatorMock.Object), ids);
        }

        [Test]
        public async Task GenerateTeams_With11Players_Produces5vs6Splits()
        {
            var (generator, ids) = BuildGeneratorForPlayers(11);

            var results = await generator.GenerateTeams(ids, Season.Summer);

            Assert.That(results, Is.Not.Empty, "Expected at least one team variant for 11 players");

            foreach (var result in results)
            {
                var sizes = new[] { result.TeamOne.Players.Count, result.TeamTwo.Players.Count };
                Array.Sort(sizes);
                Assert.That(sizes, Is.EqualTo(new[] { 5, 6 }),
                    "Each variant must split 11 players into 5 vs 6");

                var allIds = result.TeamOne.Players.Concat(result.TeamTwo.Players)
                    .Select(p => p.Id)
                    .ToHashSet();
                Assert.That(allIds.Count, Is.EqualTo(11), "All 11 players must appear exactly once");
            }
        }

        [Test]
        public async Task GenerateTeams_With7Players_Produces3vs4Splits()
        {
            var (generator, ids) = BuildGeneratorForPlayers(7);

            var results = await generator.GenerateTeams(ids, Season.Summer);

            Assert.That(results, Is.Not.Empty);
            foreach (var result in results)
            {
                var sizes = new[] { result.TeamOne.Players.Count, result.TeamTwo.Players.Count };
                Array.Sort(sizes);
                Assert.That(sizes, Is.EqualTo(new[] { 3, 4 }));
            }
        }

        [Test]
        public async Task GenerateTeams_With10Players_Produces5vs5Splits()
        {
            var (generator, ids) = BuildGeneratorForPlayers(10);

            var results = await generator.GenerateTeams(ids, Season.Summer);

            Assert.That(results, Is.Not.Empty);
            foreach (var result in results)
            {
                Assert.That(result.TeamOne.Players.Count, Is.EqualTo(5));
                Assert.That(result.TeamTwo.Players.Count, Is.EqualTo(5));
            }
        }
    }
}
