using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace Elo_fotbalek.Models
{
    public class TrendData
    {
        public Dictionary<DateTime, int> Data { get; set; }

        public Trend Trend { get; set; }
    }
}
