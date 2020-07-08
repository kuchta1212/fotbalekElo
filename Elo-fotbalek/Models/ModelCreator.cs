using System;
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

        public async Task<Team> CreateTeam(StringValues values)
        {
            var teamElo = 0;
            var players = await this.blobClient.GetPlayers();
            var team = new Team()
            {
                Players = new List<Player>()
            };

            var ids = values.Where(v => Guid.Parse(v) != Guid.Empty);
            foreach (var id in ids)
            {
                var player = players.FirstOrDefault(x => x.Id == Guid.Parse(id));
                if (player != null)
                {
                    team.Players.Add(player);
                    teamElo += player.Elo;
                }
            }

            team.TeamElo = teamElo / team.Players.Count;

            return team;
        }
    }
}
