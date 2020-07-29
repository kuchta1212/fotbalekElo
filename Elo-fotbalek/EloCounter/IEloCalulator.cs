using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EloCalculator;
using Elo_fotbalek.Models;

namespace Elo_fotbalek.EloCounter
{
    public interface IEloCalulator
    {
        EloResult CalculateElo(Match match);

        FifaEloResult CalculateFifaElo(Match match);
    }
}
