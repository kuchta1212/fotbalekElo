using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elo_fotbalek.Models
{
    public class Team
    {
        public List<Player> Players { get; set; }

        public int TeamElo { get; set; }
    }
}
