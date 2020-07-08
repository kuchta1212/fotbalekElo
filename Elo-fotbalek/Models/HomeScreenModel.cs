using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elo_fotbalek.Models
{
    public class HomeScreenModel
    {
        public IOrderedEnumerable<Player> Players { get; set; }

        public IOrderedEnumerable<Match> Matches { get; set; }
    }
}
