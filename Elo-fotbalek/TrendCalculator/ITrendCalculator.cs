﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elo_fotbalek.Models;

namespace Elo_fotbalek.TrendCalculator
{
    public interface ITrendCalculator
    {
        TrendData CalculateTrend(TrendData data, DateTime matchDate, int value);

        TrendData RemoveLatest(TrendData data);
    }
}
