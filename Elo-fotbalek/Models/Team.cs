using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Elo_fotbalek.Models
{
    public class Team : IEquatable<Team>
    {
        public List<Player> Players { get; set; }

        public int TeamElo { get; set; }

        public bool Equals(Team other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TeamElo == other.TeamElo && this.ArePlayersEqual(other.Players);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Team) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Players != null ? Players.GetHashCode() : 0) * 397) ^ TeamElo;
            }
        }

        private bool ArePlayersEqual(List<Player> otherPlayers)
        {
            if (otherPlayers.Count != this.Players.Count)
            {
                return false;
            }

            foreach (var player in otherPlayers)
            {
                if (!this.Players.Contains(player))
                {
                    return false;
                }
            }

            return true;
        }

        public void ReCalculateTeamElo(Season season)
        {
            this.TeamElo = Team.CalculateTeamElo(this.Players, season);
        }

        public static int CalculateTeamElo(List<Player> players, Season season)
        {
            var weightElo = players.ToList().Sum(x => x.Percentage < 30
                                            ? 1000
                                            : x.Percentage < 50
                                                ? AdjustElo(x.GetSeasonalElo(season))
                                                : x.GetSeasonalElo(season));

            return (int)(weightElo / players.Count);
        }

        private static int AdjustElo(int elo)
        {
            var change = Math.Abs(elo - 1000);
            var newChange = (int)(0.7 * change);

            return elo > 1000
                ? 1000 + newChange
                : 1000 - newChange;
        }


    }
}
