using System;
using System.Collections.Generic;
using Elo_fotbalek.Models;
using NUnit.Framework;

namespace Tests
{
    public class TrendCalculatorTests
    {
        private Elo_fotbalek.TrendCalculator.TrendCalculator trendCalculator;

        [SetUp]
        public void Setup()
        {
            trendCalculator = new Elo_fotbalek.TrendCalculator.TrendCalculator();
        }

        [Test]
        public void RecalculateTrend_ScoreAtLeast2_ReturnsTrendUp()
        {
            var trendData = new TrendData
            {
                Data = new Dictionary<DateTime, int>
                {
                    { DateTime.Now.AddDays(-2), 1 },
                    { DateTime.Now.AddDays(-1), 1 }
                },
                Trend = Trend.STAY
            };

            trendCalculator.RecalculateTrend(trendData);

            Assert.That(trendData.Trend, Is.EqualTo(Trend.UP));
        }

        [Test]
        public void RecalculateTrend_ScoreAtMostNeg2_ReturnsTrendDown()
        {
            var trendData = new TrendData
            {
                Data = new Dictionary<DateTime, int>
                {
                    { DateTime.Now.AddDays(-2), -1 },
                    { DateTime.Now.AddDays(-1), -1 }
                },
                Trend = Trend.STAY
            };

            trendCalculator.RecalculateTrend(trendData);

            Assert.That(trendData.Trend, Is.EqualTo(Trend.DOWN));
        }

        [Test]
        public void RecalculateTrend_ScoreBetweenNeg1And1_ReturnsTrendStay()
        {
            var trendData = new TrendData
            {
                Data = new Dictionary<DateTime, int>
                {
                    { DateTime.Now.AddDays(-2), 1 },
                    { DateTime.Now.AddDays(-1), -1 }
                },
                Trend = Trend.UP
            };

            trendCalculator.RecalculateTrend(trendData);

            Assert.That(trendData.Trend, Is.EqualTo(Trend.STAY));
        }

        [Test]
        public void RecalculateTrend_EmptyData_ReturnsTrendStay()
        {
            var trendData = new TrendData
            {
                Data = new Dictionary<DateTime, int>(),
                Trend = Trend.UP
            };

            trendCalculator.RecalculateTrend(trendData);

            Assert.That(trendData.Trend, Is.EqualTo(Trend.STAY));
        }
    }
}
