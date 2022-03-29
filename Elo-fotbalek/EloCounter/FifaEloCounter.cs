using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EloCalculator;
using Elo_fotbalek.Models;

namespace Elo_fotbalek.EloCounter
{
    /*Deprecated*/
    public class FifaEloCounter : IEloCalulator
    {
        public EloResult CalculateElo(Match match)
        {
            throw new NotImplementedException();
        }

        public FifaEloResult CalculateFifaElo(Match match)
        {
            var goalDifferenceIndex = this.GoalDifferenceIndex(match.WinnerAmount - match.LooserAmount);
            var isTie = match.WinnerAmount - match.LooserAmount == 0;
            var expectedResultIndex = this.ExpectedResult(match);

            var eloResult = new FifaEloResult()
            {
                WinnerPointChange = this.CalculatePointChange(isTie ? 0.5 : 1, goalDifferenceIndex, expectedResultIndex, match.Weight),
                LooserPointChange = this.CalculatePointChange(isTie ? 0.5: 0, goalDifferenceIndex, 1 - expectedResultIndex, match.Weight)
            };

            return eloResult;
        }

        private double CalculatePointChange(double matchResult, double goalDiffIndex, double expectedMatchResult, int matchWeight)
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
            var dr = match.Winner.TeamElo - match.Looser.TeamElo;
            var koeficient = ((-1)*dr) / 400.0;
            var x = Math.Pow(10, koeficient);
            return 1.0 / (x + 1.0);
        }
    }
}
