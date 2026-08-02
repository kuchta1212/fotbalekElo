using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elo_fotbalek.Controllers.Api;
using Elo_fotbalek.Models;
using Elo_fotbalek.Storage;
using Match = Elo_fotbalek.Models.Match;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace Tests
{
    public class BillingApiTests
    {
        private Mock<IBlobClient> mockBlobClient;
        private BillingApiController controller;

        private static readonly Guid RegularId = Guid.NewGuid();
        private static readonly Guid GuestId = Guid.NewGuid();
        private static readonly Guid OccasionalId = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            mockBlobClient = new Mock<IBlobClient>();
            controller = new BillingApiController(mockBlobClient.Object);
        }

        private static Match MatchOn(DateTime date, (Guid Id, string Name) winner, (Guid Id, string Name) looser)
        {
            return new Match
            {
                Date = date,
                WinnerAmount = 5,
                LooserAmount = 3,
                Weight = 30,
                Season = Season.Summer,
                Winner = new Team { Players = new List<Player> { new Player { Id = winner.Id, Name = winner.Name } }, TeamElo = 1000 },
                Looser = new Team { Players = new List<Player> { new Player { Id = looser.Id, Name = looser.Name } }, TeamElo = 1000 },
            };
        }

        private static CalculateBillingResponseDto ExtractResponse(IActionResult result)
        {
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var value = ((OkObjectResult)result).Value;
            var dataProp = value.GetType().GetProperty("data");
            Assert.That(dataProp, Is.Not.Null);
            return (CalculateBillingResponseDto)dataProp.GetValue(value);
        }

        [Test]
        public async Task Calculate_BillsRegulars_And_SingleAppearanceOnlyAboveTenPercentAttendance()
        {
            var day1 = new DateTime(2025, 6, 5);
            var day2 = new DateTime(2025, 6, 12);

            // Regular plays both days (2 appearances). Guest and Occasional each play once.
            var matches = new List<Match>
            {
                MatchOn(day1, (RegularId, "Regular"), (GuestId, "Guest")),
                MatchOn(day2, (RegularId, "Regular"), (OccasionalId, "Occasional")),
            };

            // Stored last-6-month attendance: Guest is below the 10% threshold, Occasional above it.
            var players = new List<Player>
            {
                new Player { Id = RegularId, Name = "Regular", Percentage = 55 },
                new Player { Id = GuestId, Name = "Guest", Percentage = 0 },
                new Player { Id = OccasionalId, Name = "Occasional", Percentage = 20 },
            };

            mockBlobClient.Setup(x => x.GetMatches(It.IsAny<DateTime>())).ReturnsAsync(matches);
            mockBlobClient.Setup(x => x.GetPlayers()).ReturnsAsync(players);

            var result = await controller.Calculate(new CalculateBillingRequestDto
            {
                FromYear = 2025, FromMonth = 6, ToYear = 2025, ToMonth = 6, TotalAmount = 300m,
            });

            var response = ExtractResponse(result);
            var billedIds = response.Rows.Select(r => r.PlayerId).ToList();

            Assert.That(billedIds, Does.Contain(RegularId.ToString()));     // 2 appearances -> always billed
            Assert.That(billedIds, Does.Contain(OccasionalId.ToString()));  // 1 appearance, 20% -> billed
            Assert.That(billedIds, Does.Not.Contain(GuestId.ToString()));   // 1 appearance, 0% -> excluded
            Assert.That(response.TotalAppearances, Is.EqualTo(3));
        }

        [Test]
        public async Task Calculate_SingleAppearanceAtExactlyTenPercent_IsExcluded()
        {
            var day1 = new DateTime(2025, 6, 5);
            var matches = new List<Match>
            {
                MatchOn(day1, (RegularId, "Player"), (GuestId, "Border")),
            };

            var players = new List<Player>
            {
                new Player { Id = RegularId, Name = "Player", Percentage = 55 },
                new Player { Id = GuestId, Name = "Border", Percentage = 10 },
            };

            mockBlobClient.Setup(x => x.GetMatches(It.IsAny<DateTime>())).ReturnsAsync(matches);
            mockBlobClient.Setup(x => x.GetPlayers()).ReturnsAsync(players);

            var result = await controller.Calculate(new CalculateBillingRequestDto
            {
                FromYear = 2025, FromMonth = 6, ToYear = 2025, ToMonth = 6, TotalAmount = 300m,
            });

            var response = ExtractResponse(result);
            var billedIds = response.Rows.Select(r => r.PlayerId).ToList();

            Assert.That(billedIds, Does.Contain(RegularId.ToString()));   // single appearance, 55% -> billed
            Assert.That(billedIds, Does.Not.Contain(GuestId.ToString())); // exactly 10% is not > 10% -> excluded
        }
    }
}
