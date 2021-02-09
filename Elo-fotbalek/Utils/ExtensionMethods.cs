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

        public static List<List<object>> ToChartArray(this Dictionary<DateTime, int> data)
        {
            var result = new List<List<object>>();
            foreach(var item in data)
            {
                var partialData = new List<object>();
                partialData.Add(item.Key.ToString("MMM dd, yyyy"));
                partialData.Add(item.Value);
                result.Add(partialData);
            }
            return result;
        }
    }
}
