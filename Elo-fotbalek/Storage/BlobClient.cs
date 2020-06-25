using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elo_fotbalek.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace Elo_fotbalek.Storage
{
    public class BlobClient : IBlobClient
    {
        private readonly IConfiguration configuration;
        private readonly CloudBlobClient cloudBlobClient;

        public BlobClient(IConfiguration configuration)
        {
            this.configuration = configuration;
            var connectionString = this.configuration.GetSection("BlobStorage").GetValue<string>("ConnectionString");

            var account = CloudStorageAccount.Parse(connectionString);
            this.cloudBlobClient = account.CreateCloudBlobClient();
            
        }
        public async Task AddPlayer(Player player)
        {
            var players = await this.GetPlayers();

            var blobName = this.configuration.GetSection("BlobStorage").GetValue<string>("PlayersBlobName");
            var blob = await this.GetBlob(blobName);

            players.Add(player);

            await blob.UploadTextAsync(JsonConvert.SerializeObject(players));
        }

        public async Task RemovePlayer(Player player)
        {
            var players = await this.GetPlayers();

            var blobName = this.configuration.GetSection("BlobStorage").GetValue<string>("PlayersBlobName");
            var blob = await this.GetBlob(blobName);

            players.Remove(player);

            await blob.UploadTextAsync(JsonConvert.SerializeObject(players));
        }

        public async Task<List<Player>> GetPlayers()
        {
            try
            {
                var blobName = this.configuration.GetSection("BlobStorage").GetValue<string>("PlayersBlobName");
                var blob = await this.GetBlob(blobName);

                var playersJson = await blob.DownloadTextAsync();
                return JsonConvert.DeserializeObject<List<Player>>(playersJson);
            }
            catch (Exception)
            {
                return new List<Player>();
            }
        }

        private async Task<CloudBlockBlob> GetBlob(string blobName)
        {
            var containerName = this.configuration.GetSection("BlobStorage").GetValue<string>("ContainerName");
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
