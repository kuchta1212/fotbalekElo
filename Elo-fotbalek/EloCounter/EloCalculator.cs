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

        public FifaEloResult CalculateFifaElo(Match match)
        {
            var goalDifferenceIndex = this.GoalDifferenceIndex(match.WinnerAmount - match.LooserAmount);
            var expectedResultIndex = this.ExpectedResult(match);

            var eloResult = new FifaEloResult()
            {
                WinnerPointChange = this.CalculatePointChange(1, goalDifferenceIndex, 1 - expectedResultIndex, match.Weight),
                LooserPointChange = this.CalculatePointChange(0, goalDifferenceIndex, expectedResultIndex, match.Weight)
            };

            return eloResult;
        }

        private double CalculatePointChange(int matchResult, double goalDiffIndex, double expectedMatchResult, int matchWeight)
        {
            return matchWeight * goalDiffIndex * (matchResult - expectedMatchResult);
        }

        private double GoalDifferenceIndex(int goalDiff)
        {
            switch (goalDiff)
            {
                case 0:
                case 1:
                    return 1;
                case 2:
                    return 1.5;
                default:
                    return (11.0 + goalDiff) / 8;
            }
        }

        private double ExpectedResult(Match match)
        {
            var dr = Math.Abs(match.Winner.TeamElo - match.Looser.TeamElo);
            var koeficient = dr / 400.0;
            var x = Math.Pow(10, koeficient);
            return 1.0 / (x + 1.0);
        }
    }
}
