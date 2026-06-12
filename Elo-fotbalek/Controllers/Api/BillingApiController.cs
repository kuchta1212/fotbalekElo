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

        [HttpGet("report")]
        public async Task<IActionResult> GetReport()
        {
            var report = await this.blobClient.GetFinanceReport();
            return Ok(MapReport(report));
        }

        [HttpPost("report")]
        [Authorize(policy: "MyPolicy")]
        public async Task<IActionResult> SaveReport([FromBody] SaveBillingReportRequestDto request)
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

            if (request.Rows == null || request.Rows.Count == 0)
            {
                return BadRequest("Žádná data k uložení");
            }

            var periodId = BuildPeriodId(request.FromYear, request.FromMonth, request.ToYear, request.ToMonth);

            var report = await this.blobClient.GetFinanceReport();

            var period = report.Periods.FirstOrDefault(p => p.Id == periodId);
            if (period == null)
            {
                report.Periods.Add(new FinanceReportPeriod
                {
                    Id = periodId,
                    FromYear = request.FromYear,
                    FromMonth = request.FromMonth,
                    ToYear = request.ToYear,
                    ToMonth = request.ToMonth
                });
            }

            // Full overwrite of this period: clear existing column on all entries,
            // then set values from the new save.
            foreach (var entry in report.Entries)
            {
                entry.AmountsPerPeriod.Remove(periodId);
            }

            foreach (var row in request.Rows)
            {
                if (!Guid.TryParse(row.PlayerId, out var playerId)) continue;

                var entry = report.Entries.FirstOrDefault(e => e.PlayerId == playerId);
                if (entry == null)
                {
                    entry = new FinanceReportEntry
                    {
                        PlayerId = playerId,
                        PlayerName = row.PlayerName
                    };
                    report.Entries.Add(entry);
                }
                else
                {
                    entry.PlayerName = row.PlayerName;
                }

                entry.AmountsPerPeriod[periodId] = row.AmountOwed;
            }

            // Recompute totals; drop entries with no remaining amounts.
            foreach (var entry in report.Entries)
            {
                entry.Total = entry.AmountsPerPeriod.Values.Sum();
            }
            report.Entries.RemoveAll(e => e.AmountsPerPeriod.Count == 0 || e.Total == 0m);

            // Drop periods that no entries reference any more.
            report.Periods.RemoveAll(p => !report.Entries.Any(e => e.AmountsPerPeriod.ContainsKey(p.Id)));

            await this.blobClient.SaveFinanceReport(report);

            return Ok(MapReport(report));
        }

        [HttpDelete("report/players/{playerId}")]
        [Authorize(policy: "MyPolicy")]
        public async Task<IActionResult> SettlePlayer(string playerId)
        {
            if (!Guid.TryParse(playerId, out var id))
            {
                return BadRequest("Neplatné ID hráče");
            }

            var report = await this.blobClient.GetFinanceReport();
            var removed = report.Entries.RemoveAll(e => e.PlayerId == id);

            if (removed == 0)
            {
                return NotFound("Hráč nenalezen v přehledu");
            }

            // Drop periods that no entries reference any more.
            report.Periods.RemoveAll(p => !report.Entries.Any(e => e.AmountsPerPeriod.ContainsKey(p.Id)));

            await this.blobClient.SaveFinanceReport(report);

            return Ok(MapReport(report));
        }

        private static string BuildPeriodId(int fromYear, int fromMonth, int toYear, int toMonth)
        {
            return $"{fromYear:0000}-{fromMonth:00}_{toYear:0000}-{toMonth:00}";
        }

        private static FinanceReportDto MapReport(FinanceReport report)
        {
            var periods = report.Periods
                .OrderBy(p => p.FromYear).ThenBy(p => p.FromMonth)
                .ThenBy(p => p.ToYear).ThenBy(p => p.ToMonth)
                .Select(p => new FinanceReportPeriodDto
                {
                    Id = p.Id,
                    FromYear = p.FromYear,
                    FromMonth = p.FromMonth,
                    ToYear = p.ToYear,
                    ToMonth = p.ToMonth
                })
                .ToList();

            var entries = report.Entries
                .OrderBy(e => e.PlayerName, StringComparer.Create(System.Globalization.CultureInfo.GetCultureInfo("cs-CZ"), ignoreCase: true))
                .Select(e => new FinanceReportEntryDto
                {
                    PlayerId = e.PlayerId.ToString(),
                    PlayerName = e.PlayerName,
                    Total = e.Total,
                    AmountsPerPeriod = e.AmountsPerPeriod
                })
                .ToList();

            return new FinanceReportDto
            {
                Periods = periods,
                Entries = entries
            };
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

    public class SaveBillingReportRequestDto
    {
        public int FromYear { get; set; }
        public int FromMonth { get; set; }
        public int ToYear { get; set; }
        public int ToMonth { get; set; }
        public List<SaveBillingReportRowDto> Rows { get; set; }
    }

    public class SaveBillingReportRowDto
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public decimal AmountOwed { get; set; }
    }

    public class FinanceReportDto
    {
        public List<FinanceReportPeriodDto> Periods { get; set; }
        public List<FinanceReportEntryDto> Entries { get; set; }
    }

    public class FinanceReportPeriodDto
    {
        public string Id { get; set; }
        public int FromYear { get; set; }
        public int FromMonth { get; set; }
        public int ToYear { get; set; }
        public int ToMonth { get; set; }
    }

    public class FinanceReportEntryDto
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public decimal Total { get; set; }
        public Dictionary<string, decimal> AmountsPerPeriod { get; set; }
    }
}
