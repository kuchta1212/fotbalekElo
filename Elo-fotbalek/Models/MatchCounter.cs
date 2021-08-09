using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Elo_fotbalek.Models
{
    public class MatchCounter
    {
        [Display(Name = "VZ:")]
        public int BigMatches { get; set; }

        [Display(Name = "MZ:")]
        public int SmallMatches { get; set; }

        public override string ToString()
        {
            return BigMatches+SmallMatches + " (v:" + BigMatches + ", m:" + SmallMatches + ")";
        }

        public int TotalAmount()
        {
            return BigMatches + SmallMatches;
        }
    }
}
