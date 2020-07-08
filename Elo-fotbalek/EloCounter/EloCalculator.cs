using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EloCalculator;
using Elo_fotbalek.Models;

namespace Elo_fotbalek.EloCounter
{
    public class EloCalculator : IEloCalulator
    {
        public EloResult CalculateElo(Match match)
        {
            var eloMatch = new EloMatch();

            var winners = eloMatch.AddTeam(new EloTeam(true));
            foreach (var winnerPlayer in match.Winner.Players)
            {
                eloMatch.AddPlayerToTeam(winners, new EloPlayer(new EloPlayerIdentifier(winnerPlayer.Id), winnerPlayer.Elo));
            }

            var loosers = eloMatch.AddTeam(new EloTeam(false));
            foreach (var looserPlayer in match.Looser.Players)
            {
                eloMatch.AddPlayerToTeam(loosers, new EloPlayer(new EloPlayerIdentifier(looserPlayer.Id), looserPlayer.Elo));
            }

            return eloMatch.Calculate();
        }
    }
}
