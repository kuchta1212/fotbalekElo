using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elo_fotbalek.Models;

namespace Elo_fotbalek.Storage
{
    public interface IBlobClient
    {
        Task AddPlayer(Player player);

        Task RemovePlayer(Player player);

        Task<List<Player>> GetPlayers();
    }
}
