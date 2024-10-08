﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elo_fotbalek.Account;
using Elo_fotbalek.Models;

namespace Elo_fotbalek.Storage
{
    public interface IBlobClient
    {
        Task AddPlayer(Player player);

        Task UpdatePlayers(List<Player> players);

        Task RemovePlayer(Player player);

        Task<List<Player>> GetPlayers();

        Task AddMatch(Match match);

        Task RemoveMatch(Match match);

        Task<List<Match>> GetMatches();

        Task<List<Match>> GetMatches(DateTime since);

        Task<List<MyUser>> GetUsers();

        Task<List<Doodle>> GetDoodle();

        Task SaveDoodle(List<Doodle> doodle);
    }
}
