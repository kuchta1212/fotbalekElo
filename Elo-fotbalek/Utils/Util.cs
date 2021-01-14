using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elo_fotbalek.Models;

namespace Elo_fotbalek.Utils
{
    public static class Util
    {
        public static int CountGeneralElo(SeasonalElos elos)
        {
            return 1000 + (elos.SummerElo - 1000) + (elos.WinterElo - 1000);
        }
    }
}
