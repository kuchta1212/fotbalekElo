namespace Elo_fotbalek.Controllers.Api
{
    using Elo_fotbalek.Models;
    using Elo_fotbalek.Storage;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/billing")]
    public class BillingApiController : BaseApiController
    {
        private readonly IBlobClient blobClient;

        public BillingApiController(IBlobClient blobClient)
        {
            this.blobClient = blobClient;
        }

        [HttpPost("calculate")]
        [Authorize(policy: "MyPolicy")]
        public async Task<IActionResult> Calculate([FromBody] CalculateBillingRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("Chybí data požadavku");
            }

            if (request.FromMonth < 1 || request.FromMonth > 12 || request.ToMonth < 1 || request.ToMonth > 12)
            {
                return BadRequest("Měsíc musí být v rozsahu 1–12");
            }

            if (request.FromYear < 2000 || request.ToYear < 2000)
            {
                return BadRequest("Neplatný rok");
            }

            var fromKey = request.FromYear * 100 + request.FromMonth;
            var toKey = request.ToYear * 100 + request.ToMonth;
            if (fromKey > toKey)
            {
                return BadRequest("Počáteční měsíc musí být dříve nebo stejný jako koncový");
            }

            if (request.TotalAmount <= 0)
            {
                return BadRequest("Celková částka musí být kladná");
            }

            try
            {
                var since = new DateTime(request.FromYear, request.FromMonth, 1);
                var until = new DateTime(request.ToYear, request.ToMonth, 1)
                    .AddMonths(1)
                    .AddTicks(-1);

                var matches = await this.blobClient.GetMatches(since);
                var matchesInRange = matches.Where(m => m.Date <= until).ToList();

                // Map: PlayerId -> (Player, set of distinct match dates)
                var perPlayer = new Dictionary<Guid, (Player Player, HashSet<DateTime> Days)>();
                foreach (var match in matchesInRange)
                {
                    var day = match.Date.Date;
                    foreach (var player in match.GetAllPlayers())
                    {
                        if (!perPlayer.TryGetValue(player.Id, out var entry))
                        {
                            entry = (player, new HashSet<DateTime>());
                            perPlayer[player.Id] = entry;
                        }
                        entry.Days.Add(day);
                    }
                }

                var included = perPlayer.Values
                    .Where(e => e.Days.Count >= 1)
                    .Select(e => (e.Player, Appearances: e.Days.Count))
                    .ToList();

                var totalAppearances = included.Sum(i => i.Appearances);

                var matchDays = matchesInRange
                    .Select(m => m.Date.Date)
                    .Distinct()
                    .OrderBy(d => d)
                    .Select(d => d.ToString("yyyy-MM-dd"))
                    .ToList();

                var rows = new List<BillingRowDto>();
                decimal pricePerGame = 0m;

                if (totalAppearances > 0)
                {
                    pricePerGame = Math.Round(request.TotalAmount / totalAppearances, 2, MidpointRounding.AwayFromZero);

                    rows = included
                        .Select(i => new BillingRowDto
                        {
                            PlayerId = i.Player.Id.ToString(),
                            PlayerName = i.Player.Name,
                            Appearances = i.Appearances,
                            AmountOwed = (decimal)Math.Ceiling(
                                request.TotalAmount * i.Appearances / (decimal)totalAppearances)
                        })
                        .OrderByDescending(r => r.AmountOwed)
                        .ThenBy(r => r.PlayerName)
                        .ToList();

                    var collected = rows.Sum(r => r.AmountOwed);
                    if (collected < request.TotalAmount)
                    {
                        return ServerError(
                            $"Chyba zaokrouhlení: vybráno {collected} Kč je méně než požadováno {request.TotalAmount} Kč");
                    }
                }

                return Ok(new CalculateBillingResponseDto
                {
                    PricePerGame = pricePerGame,
                    TotalAppearances = totalAppearances,
                    TotalAmount = request.TotalAmount,
                    CollectedTotal = rows.Sum(r => r.AmountOwed),
                    MatchDays = matchDays,
                    Rows = rows
                });
            }
            catch (Exception ex)
            {
                return ServerError($"Vyúčtování selhalo: {ex.Message}");
            }
        }
    }

    public class CalculateBillingRequestDto
    {
        public int FromYear { get; set; }
        public int FromMonth { get; set; }
        public int ToYear { get; set; }
        public int ToMonth { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class CalculateBillingResponseDto
    {
        public decimal PricePerGame { get; set; }
        public int TotalAppearances { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CollectedTotal { get; set; }
        public List<string> MatchDays { get; set; }
        public List<BillingRowDto> Rows { get; set; }
    }

    public class BillingRowDto
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int Appearances { get; set; }
        public decimal AmountOwed { get; set; }
    }
}
