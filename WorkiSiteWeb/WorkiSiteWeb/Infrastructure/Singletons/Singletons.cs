
using System;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using System.Configuration;
using Microsoft.WindowsAzure;
using Worki.Infrastructure.Helpers;
using Microsoft.ApplicationServer.Caching;
namespace Worki.Web.Singletons
{
    public sealed class CloudBlobContainerSingleton
    {
        private static readonly Lazy<CloudBlobContainerSingleton> lazy = new Lazy<CloudBlobContainerSingleton>(() => new CloudBlobContainerSingleton());

        public static CloudBlobContainerSingleton Instance { get { return lazy.Value; } }

        private CloudBlobContainerSingleton()
        {
            _IsDevStore = bool.Parse(ConfigurationManager.AppSettings["IsDevStorage"]);
            _BlobContainerName = ConfigurationManager.AppSettings["AzureBlobContainer"];

            if (!RoleEnvironment.IsAvailable || _BlobContainer != null)
                return;

            if (string.IsNullOrEmpty(_BlobContainerName))
                return;

            var blobStorageAccount = _IsDevStore ? CloudStorageAccount.DevelopmentStorageAccount : CloudStorageAccount.FromConfigurationSetting(MiscHelpers.AzureConstants.DataConnectionString);
            var blobClient = blobStorageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(_BlobContainerName);
            blobContainer.CreateIfNotExist();
            blobContainer.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Container });

            _BlobContainer = blobContainer;
        }

        #region Fields

        private CloudBlobContainer _BlobContainer;
        private bool _IsDevStore;
        string _BlobContainerName;

        #endregion

        #region Properties

        public CloudBlobContainer BlobContainer
        {
            get { return _BlobContainer; }
        }

        #endregion
    }

    public sealed class DataCacheSingleton
    {
        private static readonly Lazy<DataCacheSingleton> lazy = new Lazy<DataCacheSingleton>(() => new DataCacheSingleton());

        public static DataCacheSingleton Instance { get { return lazy.Value; } }

        private DataCacheSingleton()
        {
            _CacheFactory = new DataCacheFactory();
            _Cache = _CacheFactory.GetDefaultCache();
        }

        #region Fields

        DataCacheFactory _CacheFactory;
        DataCache _Cache;

        #endregion

        #region Properties

        public DataCache Cache
        {
            get { return _Cache; }
        }

        #endregion
    }
}