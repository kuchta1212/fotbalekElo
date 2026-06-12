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
    using System.Numerics;
    using System.Threading.Tasks;

    public class OfflineBlobClient : IBlobClient
    {
        private string FolderPath = @"C:\Users\jakuchar\source\repos\fotbalekElo\Test\";

        private readonly IOptions<BlobStorageOptions> options;

        public OfflineBlobClient(IOptions<BlobStorageOptions> options)
        {
            this.options = options;
        }

        public async Task AddMatch(Match match)
        {
            var matches = await this.GetMatches();
            matches.Add(match);
            File.WriteAllText(this.FolderPath + this.options.Value.MatchesBlobName, JsonConvert.SerializeObject(matches));
        }

        public async Task AddPlayer(Player player)
        {
            var players = await this.GetPlayers();
            players.Add(player);
            File.WriteAllText(this.FolderPath + this.options.Value.PlayersBlobName, JsonConvert.SerializeObject(players));
        }

        public Task<List<Doodle>> GetDoodle()
        {
            var doodleJson = File.ReadAllText(this.FolderPath + this.options.Value.DoodleBlobName);
            return Task.FromResult(JsonConvert.DeserializeObject<List<Doodle>>(doodleJson));
        }

        public Task<List<Match>> GetMatches()
        {
            var matchesJson = File.ReadAllText(this.FolderPath + this.options.Value.MatchesBlobName);
            return Task.FromResult(JsonConvert.DeserializeObject<List<Match>>(matchesJson));
        }

        public Task<List<Player>> GetPlayers()
        {
            var playersJson = File.ReadAllText(this.FolderPath + this.options.Value.PlayersBlobName);
            return Task.FromResult(JsonConvert.DeserializeObject<List<Player>>(playersJson));
        }

        public Task<List<MyUser>> GetUsers()
        {
            var playersJson = File.ReadAllText(this.FolderPath + this.options.Value.UsersBlobName);
            return Task.FromResult(JsonConvert.DeserializeObject<List<MyUser>>(playersJson));
        }

        public Task RemoveMatch(Match match)
        {
            throw new NotImplementedException();
        }

        public async Task RemovePlayer(Player player)
        {
            var players = await this.GetPlayers();
            players.Remove(player);
            File.WriteAllText(this.FolderPath + this.options.Value.PlayersBlobName, JsonConvert.SerializeObject(players));
        }

        public Task SaveDoodle(List<Doodle> doodle)
        {
            File.WriteAllText(this.FolderPath + this.options.Value.DoodleBlobName, JsonConvert.SerializeObject(doodle));
            return Task.CompletedTask;
        }

        public Task UpdatePlayers(List<Player> players)
        {
            File.WriteAllText(this.FolderPath + this.options.Value.PlayersBlobName, JsonConvert.SerializeObject(players));
            return Task.CompletedTask;
        }

        public async Task<List<Match>> GetMatches(DateTime since)
        {
            var matches = await this.GetMatches();
            return matches.Where(m => m.Date >= since).ToList();
        }

        public Task<FinanceReport> GetFinanceReport()
        {
            var path = this.FolderPath + this.options.Value.FinanceReportBlobName;
            if (!File.Exists(path))
            {
                return Task.FromResult(new FinanceReport());
            }
            var json = File.ReadAllText(path);
            return Task.FromResult(JsonConvert.DeserializeObject<FinanceReport>(json) ?? new FinanceReport());
        }

        public Task SaveFinanceReport(FinanceReport report)
        {
            File.WriteAllText(this.FolderPath + this.options.Value.FinanceReportBlobName, JsonConvert.SerializeObject(report));
            return Task.CompletedTask;
        }
    }
}
