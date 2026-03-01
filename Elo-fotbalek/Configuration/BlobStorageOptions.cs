using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elo_fotbalek.Configuration
{
    public class BlobStorageOptions
    {
        public const string BlobStorage = "BlobStorage";

        public string ConnectionString { get; set; } = string.Empty;

        public string ContainerName { get; set; } = string.Empty;

        public string PlayersBlobName { get; set; } = "players.json";

        public string MatchesBlobName { get; set; } = "matches.json";

        public string UsersBlobName { get; set; } = "users.json";

        public string DoodleBlobName { get; set; } = "doodle.json";

        public bool UseOffline { get; set; }
    }
}
