using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elo_fotbalek.Models;

namespace Elo_fotbalek.TrendCalculator
{
    public class TrendCalculator : ITrendCalculator
    {
        public TrendData CalculateTrend(TrendData trendData, DateTime matchDate, int value)
        {
            if (trendData.Data.Count >= 5)
            {
                var min = trendData.Data.Min(d => d.Key);
                trendData.Data.Remove(min);
            }

            while (trendData.Data.ContainsKey(matchDate))
            {
                matchDate = matchDate.AddMinutes(1);
            }

            trendData.Data.Add(matchDate, value);
            trendData.Trend = this.ReCalculate(trendData.Data);

            return trendData;
        }

        public TrendData RemoveLatest(TrendData trendData)
        {
            if (trendData.Data.Any())
            {
                var min = trendData.Data.Min(d => d.Key);
                trendData.Data.Remove(min);
            }
            
            trendData.Trend = Trend.STAY;

            return trendData;
        }

        private Trend ReCalculate(Dictionary<DateTime, int> data)
        {
            var score = data.Sum(x => x.Value);
            if (score >= 2)
            {
                return Trend.UP;
            }

            if (score <= -2)
            {
                return Trend.DOWN;
            }

            return Trend.STAY;
        }
    }
}
