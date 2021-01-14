using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elo_fotbalek.Models;

namespace Elo_fotbalek.Utils
{
    public static class ExtensionMethods
    {
        public static string ToFriendlyString(this Season season)
        {
            return season == Season.Summer ? "Léto" : "Zima";
        }
    }
}
