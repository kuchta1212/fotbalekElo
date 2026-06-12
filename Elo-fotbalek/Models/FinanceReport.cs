using System;
using System.Collections.Generic;

namespace Elo_fotbalek.Models
{
    public class FinanceReport
    {
        public List<FinanceReportPeriod> Periods { get; set; } = new List<FinanceReportPeriod>();

        public List<FinanceReportEntry> Entries { get; set; } = new List<FinanceReportEntry>();
    }

    public class FinanceReportPeriod
    {
        public string Id { get; set; }

        public int FromYear { get; set; }

        public int FromMonth { get; set; }

        public int ToYear { get; set; }

        public int ToMonth { get; set; }
    }

    public class FinanceReportEntry
    {
        public Guid PlayerId { get; set; }

        public string PlayerName { get; set; }

        public Dictionary<string, decimal> AmountsPerPeriod { get; set; } = new Dictionary<string, decimal>();

        public decimal Total { get; set; }
    }
}
