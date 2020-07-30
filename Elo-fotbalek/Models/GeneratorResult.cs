using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elo_fotbalek.Models;

namespace Elo_fotbalek.TeamGenerator
{
    public class GeneratorResult
    {
        public Team TeamOne { get; set; }

        public Team TeamTwo { get; set; }

        public int EloDiff { get; set; }
    }
}
