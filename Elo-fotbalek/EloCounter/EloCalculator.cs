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
                eloMatch.AddPlayerToTeam(winners, new EloPlayer(new EloPlayerIdentifier(winnerPlayer.Id), winnerPlayer.GetSeasonalElo(match.Season)));
            }

            var loosers = eloMatch.AddTeam(new EloTeam(false));
            foreach (var looserPlayer in match.Looser.Players)
            {
                eloMatch.AddPlayerToTeam(loosers, new EloPlayer(new EloPlayerIdentifier(looserPlayer.Id), looserPlayer.GetSeasonalElo(match.Season)));
            }

            return eloMatch.Calculate();
        }

        public FifaEloResult CalculateFifaElo(Match match)
        {
            var goalDifferenceIndex = this.GoalDifferenceIndex(match.WinnerAmount - match.LooserAmount);
            var expectedResultIndex = this.ExpectedResult(match);
            var isTie = match.WinnerAmount - match.LooserAmount == 0;

            var eloResult = new FifaEloResult()
            {
                WinnerPointChange = this.CalculatePointChange(isTie ? 0.5 : 1, goalDifferenceIndex, expectedResultIndex, match.Weight),
                LooserPointChange = this.CalculatePointChange(isTie ? 0.5 : 0, goalDifferenceIndex, 1 - expectedResultIndex, match.Weight)
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
            var koeficient = ((-1) * dr) / 400.0;
            var x = Math.Pow(10, koeficient);
            return 1.0 / (x + 1.0);
        }
    }
}
