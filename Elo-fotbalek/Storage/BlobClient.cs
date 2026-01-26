using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Elo_fotbalek.Account;
using Elo_fotbalek.Configuration;
using Elo_fotbalek.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using AzureBlobClient = Azure.Storage.Blobs.BlobClient;

namespace Elo_fotbalek.Storage
{
    public class BlobClient : IBlobClient
    {
        private readonly IOptions<BlobStorageOptions> options;
        private readonly BlobServiceClient blobServiceClient;

        public BlobClient(IOptions<BlobStorageOptions> options)
        {
            this.options = options;
            var connectionString = this.options.Value.ConnectionString;
            this.blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task AddPlayer(Player player)
        {
            var players = await this.GetPlayers();

            var blobName = this.options.Value.PlayersBlobName;
            var blob = await this.GetBlobClient(blobName);

            players.Add(player);

            var json = JsonConvert.SerializeObject(players);
            await blob.UploadAsync(new BinaryData(json), overwrite: true);
        }

        public async Task UpdatePlayers(List<Player> players)
        {
            var blobName = this.options.Value.PlayersBlobName;
            var blob = await this.GetBlobClient(blobName);

            var json = JsonConvert.SerializeObject(players);
            await blob.UploadAsync(new BinaryData(json), overwrite: true);
        }

        public async Task RemovePlayer(Player player)
        {
            var players = await this.GetPlayers();

            var blobName = this.options.Value.PlayersBlobName;
            var blob = await this.GetBlobClient(blobName);

            players.Remove(player);

            var json = JsonConvert.SerializeObject(players);
            await blob.UploadAsync(new BinaryData(json), overwrite: true);
        }

        public async Task<List<Player>> GetPlayers()
        {
            try
            {
                var blobName = this.options.Value.PlayersBlobName;
                var blob = await this.GetBlobClient(blobName);

                var response = await blob.DownloadContentAsync();
                var playersJson = response.Value.Content.ToString();
                return JsonConvert.DeserializeObject<List<Player>>(playersJson) ?? new List<Player>();
            }
            catch (Exception)
            {
                return new List<Player>();
            }
        }

        public async Task AddMatch(Match match)
        {
            var matches = await this.GetMatches();

            var blobName = this.options.Value.MatchesBlobName;
            var blob = await this.GetBlobClient(blobName);

            matches.Add(match);

            var json = JsonConvert.SerializeObject(matches);
            await blob.UploadAsync(new BinaryData(json), overwrite: true);
        }

        public async Task RemoveMatch(Match match)
        {
            var matches = await this.GetMatches();

            var blobName = this.options.Value.MatchesBlobName;
            var blob = await this.GetBlobClient(blobName);

            matches.RemoveAll(m => m.Date == match.Date && m.Score == match.Score);

            var json = JsonConvert.SerializeObject(matches);
            await blob.UploadAsync(new BinaryData(json), overwrite: true);
        }

        public async Task<List<Match>> GetMatches()
        {
            try
            {
                var blobName = this.options.Value.MatchesBlobName;
                var blob = await this.GetBlobClient(blobName);

                var response = await blob.DownloadContentAsync();
                var matchesJson = response.Value.Content.ToString();
                return JsonConvert.DeserializeObject<List<Match>>(matchesJson) ?? new List<Match>();
            }
            catch (Exception)
            {
                return new List<Match>();
            }
        }

        public async Task<List<Match>> GetMatches(DateTime since)
        {
            var matches = await this.GetMatches();
            return matches.Where(m => m.Date >= since).ToList();
        }

        public async Task<List<MyUser>> GetUsers()
        {
            try
            {
                var blobName = this.options.Value.UsersBlobName;
                var blob = await this.GetBlobClient(blobName);

                var response = await blob.DownloadContentAsync();
                var usersJson = response.Value.Content.ToString();
                return JsonConvert.DeserializeObject<List<MyUser>>(usersJson) ?? new List<MyUser>();
            }
            catch (Exception)
            {
                return new List<MyUser>();
            }
        }

        public async Task<List<Doodle>> GetDoodle()
        {
            try
            {
                var blobName = this.options.Value.DoodleBlobName;
                var blob = await this.GetBlobClient(blobName);

                var response = await blob.DownloadContentAsync();
                var doodleJson = response.Value.Content.ToString();
                return JsonConvert.DeserializeObject<List<Doodle>>(doodleJson) ?? new List<Doodle>();
            }
            catch (Exception)
            {
                return new List<Doodle>();
            }
        }

        public async Task SaveDoodle(List<Doodle> doodle)
        {
            var blobName = this.options.Value.DoodleBlobName;
            var blob = await this.GetBlobClient(blobName);

            var json = JsonConvert.SerializeObject(doodle);
            await blob.UploadAsync(new BinaryData(json), overwrite: true);
        }

        private async Task<AzureBlobClient> GetBlobClient(string blobName)
        {
            var containerName = this.options.Value.ContainerName;
            var containerClient = this.blobServiceClient.GetBlobContainerClient(containerName);
            
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            return containerClient.GetBlobClient(blobName);
        }
    }
}
