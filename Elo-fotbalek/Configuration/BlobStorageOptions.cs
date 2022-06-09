using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elo_fotbalek.Configuration
{
    public class BlobStorageOptions
    {
        public const string BlobStorage = "BlobStorage";

        public string ConnectionString { get; set; }

        public string ContainerName { get; set; }

        public string PlayersBlobName { get; set; }

        public string MatchesBlobName { get; set; }

        public string UsersBlobName { get; set; }

        public string DoodleBlobName { get; set; }

        public bool UseOffline { get; set; }
    }
}
