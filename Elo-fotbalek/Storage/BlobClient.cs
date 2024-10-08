﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elo_fotbalek.Account;
using Elo_fotbalek.Configuration;
using Elo_fotbalek.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace Elo_fotbalek.Storage
{
    public class BlobClient : IBlobClient
    {
        private readonly IOptions<BlobStorageOptions> options;
        private readonly CloudBlobClient cloudBlobClient;

        public BlobClient(IOptions<BlobStorageOptions> options)
        {
            this.options = options;
            var connectionString = this.options.Value.ConnectionString;

            var account = CloudStorageAccount.Parse(connectionString);
            this.cloudBlobClient = account.CreateCloudBlobClient();
            
        }
        public async Task AddPlayer(Player player)
        {
            var players = await this.GetPlayers();

            var blobName = this.options.Value.PlayersBlobName;
            var blob = await this.GetBlob(blobName);

            players.Add(player);

            await blob.UploadTextAsync(JsonConvert.SerializeObject(players));
        }

        public async Task UpdatePlayers(List<Player> players)
        {
            var blobName = this.options.Value.PlayersBlobName;
            var blob = await this.GetBlob(blobName);

            await blob.UploadTextAsync(JsonConvert.SerializeObject(players));
        }

        public async Task RemovePlayer(Player player)
        {
            var players = await this.GetPlayers();

            var blobName = this.options.Value.PlayersBlobName;
            var blob = await this.GetBlob(blobName);

            players.Remove(player);

            await blob.UploadTextAsync(JsonConvert.SerializeObject(players));
        }

        public async Task<List<Player>> GetPlayers()
        {
            try
            {
                var blobName = this.options.Value.PlayersBlobName;
                var blob = await this.GetBlob(blobName);

                var playersJson = await blob.DownloadTextAsync();
                return JsonConvert.DeserializeObject<List<Player>>(playersJson);
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
            var blob = await this.GetBlob(blobName);

            matches.Add(match);

            await blob.UploadTextAsync(JsonConvert.SerializeObject(matches));
        }

        public async Task RemoveMatch(Match match)
        {
            var matches = await this.GetMatches();

            var blobName = this.options.Value.MatchesBlobName;
            var blob = await this.GetBlob(blobName);

            matches.RemoveAll(m => m.Date == match.Date && m.Score == match.Score);

            await blob.UploadTextAsync(JsonConvert.SerializeObject(matches));
        }

        public async Task<List<Match>> GetMatches()
        {
            try
            {
                var blobName = this.options.Value.MatchesBlobName;
                var blob = await this.GetBlob(blobName);

                var matchesJson = await blob.DownloadTextAsync();
                return JsonConvert.DeserializeObject<List<Match>>(matchesJson);
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
                var blob = await this.GetBlob(blobName);

                var usersJson = await blob.DownloadTextAsync();
                return JsonConvert.DeserializeObject<List<MyUser>>(usersJson);
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
                var blob = await this.GetBlob(blobName);

                var doodleJson = await blob.DownloadTextAsync();
                return JsonConvert.DeserializeObject<List<Doodle>>(doodleJson);
            }
            catch (Exception)
            {
                return new List<Doodle>();
            }
        }

        public async Task SaveDoodle(List<Doodle> doodle)
        {
            var blobName = this.options.Value.DoodleBlobName;
            var blob = await this.GetBlob(blobName);

            await blob.UploadTextAsync(JsonConvert.SerializeObject(doodle));
        }

        private async Task<CloudBlockBlob> GetBlob(string blobName)
        {
            var containerName = this.options.Value.ContainerName;
            var blobContainer = this.cloudBlobClient.GetContainerReference(containerName);
            await blobContainer.CreateIfNotExistsAsync();

            await blobContainer.SetPermissionsAsync(new BlobContainerPermissions()
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            });

            return blobContainer.GetBlockBlobReference(blobName);
        }
    }
}
