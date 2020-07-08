using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elo_fotbalek.Models
{
    public class Match
    {
        public Team Winner { get; set; }

        public Team Looser { get; set; }

        public int WinnerAmount { get; set; }

        public int LooserAmount { get; set; }

        public string Score => this.WinnerAmount+":"+this.LooserAmount;
    }
}
