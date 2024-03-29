﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elo_fotbalek.Storage;
using Microsoft.Extensions.Primitives;

namespace Elo_fotbalek.Models
{
    public class ModelCreator: IModelCreator
    {
        private readonly IBlobClient blobClient;

        public ModelCreator(IBlobClient blobClient)
        {
            this.blobClient = blobClient;
        }

        public async Task<Team> CreateTeam(IEnumerable<string> playerIds, Season season)
        {
            var players = await this.blobClient.GetPlayers();
            var team = new Team()
            {
                Players = new List<Player>()
            };

            foreach (var id in playerIds)
            {
                var player = players.FirstOrDefault(x => x.Id == Guid.Parse(id));
                if (player != null)
                {
                    team.Players.Add(player);
                }
            }

            if(team.Players.Count > 0)
            {
                team.ReCalculateTeamElo(season);
            }

            return team;
        }
    }
}
