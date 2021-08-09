
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Elo_fotbalek.Models
{
    public class Player : IEquatable<Player>
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public int Elo { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public SeasonalElos Elos { get; set; }

        public MatchCounter AmountOfWins { get; set; }

        public MatchCounter AmountOfLooses { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TrendData Trend { get; set; }

        public int AmountOfMissedGames { get; set; }

        public int Percentage { get; set; }

        public override string ToString()
        {
            return this.Name + "(" + this.Elo +")";
        }
        
        public bool Equals(Player other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Player) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public void UpdateElo(int change, Season matchSeason)
        {
            if (matchSeason == Season.Summer)
            {
                this.Elos.SummerElo += change;
            }
            else
            {
                this.Elos.WinterElo += change;
            }
        }

        public int GetSeasonalElo(Season season) => season == Season.Summer ? this.Elos.SummerElo : this.Elos.WinterElo;
    }
}
