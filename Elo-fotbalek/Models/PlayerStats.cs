using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elo_fotbalek.Models
{
    public class PlayerStats : BaseModel
    {
        public Player Player { get; set; }

        public List<ChartModel> Elos { get; set; }

        public int MinElo { get; set; }

        public int MaxElo { get; set; }

        public int PlayedMatches { get; set; }

        public double Attandance { get; set; }
    }
}
