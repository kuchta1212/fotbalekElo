using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elo_fotbalek.Models;

namespace Elo_fotbalek.TeamGenerator
{
    public interface ITeamGenerator
    {
        Task<List<GeneratorResult>> GenerateTeams(List<string> playerIds, List<string> substitudeIds, Season season);

        Task<List<GeneratorResult>> GenerateTeams(List<string> playerIds, Season season);
    }
}
