using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elo_fotbalek.Models
{
    public class Match : BaseModel
    {
        public Team Winner { get; set; }

        public Team Looser { get; set; }

        public int WinnerAmount { get; set; }

        public int LooserAmount { get; set; }

        public string Score => this.WinnerAmount+":"+this.LooserAmount;

        public DateTime Date { get; set; }

        public int Weight { get; set; }

        public string WeighInStr => this.Weight == 30 ? "Velký zápas" : "Malý zápas";

        public Season Season { get; set; }

        public string Hero { get; set; }
    }
}
