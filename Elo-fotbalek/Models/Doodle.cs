namespace Elo_fotbalek.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Doodle
    {
        public string Player { get; set; }

        public Dictionary<DateTime, DoodleValue> PlayersPoll { get; set; }

        public Dictionary<DateTime, DoodleValue> GetSortedPlayersPoll()
        {
            return this.PlayersPoll.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }
}
