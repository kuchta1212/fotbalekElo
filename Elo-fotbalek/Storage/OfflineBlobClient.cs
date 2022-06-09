namespace Elo_fotbalek.Storage
{
    using Elo_fotbalek.Account;
    using Elo_fotbalek.Configuration;
    using Elo_fotbalek.Models;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class OfflineBlobClient : IBlobClient
    {
        private string FolderPath = @"C:\Users\jakuchar\source\repos\fotbalekElo\Test\";

        private readonly IOptions<BlobStorageOptions> options;

        public OfflineBlobClient(IOptions<BlobStorageOptions> options)
        {
            this.options = options;
        }

        public Task AddMatch(Match match)
        {
            throw new NotImplementedException();
        }

        public Task AddPlayer(Player player)
        {
            throw new NotImplementedException();
        }

        public Task<List<Doodle>> GetDoodle()
        {
            var doodleJson = File.ReadAllText(this.FolderPath + this.options.Value.DoodleBlobName);
            return Task.FromResult(JsonConvert.DeserializeObject<List<Doodle>>(doodleJson));
        }

        public Task<List<Match>> GetMatches()
        {
            throw new NotImplementedException();
        }

        public Task<List<Player>> GetPlayers()
        {
            var playersJson = File.ReadAllText(this.FolderPath + this.options.Value.PlayersBlobName);
            return Task.FromResult(JsonConvert.DeserializeObject<List<Player>>(playersJson));
        }

        public Task<List<MyUser>> GetUsers()
        {
            throw new NotImplementedException();
        }

        public Task RemoveMatch(Match match)
        {
            throw new NotImplementedException();
        }

        public Task RemovePlayer(Player player)
        {
            throw new NotImplementedException();
        }

        public Task SaveDoodle(List<Doodle> doodle)
        {
            File.WriteAllText(this.FolderPath + this.options.Value.DoodleBlobName, JsonConvert.SerializeObject(doodle));
            return Task.CompletedTask;
        }

        public Task UpdatePlayers(List<Player> players)
        {
            throw new NotImplementedException();
        }
    }
}
