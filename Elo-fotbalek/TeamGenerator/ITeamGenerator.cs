using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elo_fotbalek.Models;

namespace Elo_fotbalek.TeamGenerator
{
    public interface ITeamGenerator
    {
        List<GeneratorResult> GenerateTeams(List<Player> players);
    }
}
