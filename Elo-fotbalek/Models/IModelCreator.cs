using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Elo_fotbalek.Models
{
    public interface IModelCreator
    {
        Task<Team> CreateTeam(IEnumerable<string> playerIds, Season season);
    }
}
