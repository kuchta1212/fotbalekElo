namespace Elo_fotbalek.Models
{
    using Elo_fotbalek.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class HomeScreenModel : BaseModel
    {
        public IOrderedEnumerable<Player> Players { get; set; }

        public IOrderedEnumerable<Player> NonRegulars { get; set; }

        public IOrderedEnumerable<Match> Matches { get; set; }
    }
}
