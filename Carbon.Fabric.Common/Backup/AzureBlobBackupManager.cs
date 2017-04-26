using System;
using System.Collections.Generic;
using System.Fabric.Description;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using Carbon.Framework.Extensions;
using Carbon.Fabric.Common.Logging;
using Carbon.Framework.Util;

namespace Carbon.Fabric.Common.Backup
{
    public class AzureBlobBackupManager : IBackupStore
    {
        private readonly CloudBlobClient cloudBlobClient;
        private CloudBlobContainer backupBlobContainer;
        private int MaxBackupsToKeep;

        private string PartitionTempDirectory;
        private string partitionId;

        private TimeSpan backupFrequency;
        private long keyMin;
        private long keyMax;
        private IDependencyContainer scope;

        public AzureBlobBackupManager(IDependencyContainer scope, ConfigurationSection configSection, string partitionId, long keymin, long keymax, string codePackageTempDirectory)
        {
            this.keyMin = keymin;
            this.keyMax = keymax;

            var cs = configSection.Parameters["BackupConnectionString"];
            string backupConnectionString = cs.IsEncrypted ? cs.DecryptValue().ToPlainText() : cs.Value;

            this.backupFrequency = TimeSpan.Parse(configSection.Parameters["BackupFrequency"].Value);
            this.MaxBackupsToKeep = int.Parse(configSection.Parameters["MaxBackupsToKeep"].Value);
            this.partitionId = partitionId;
            this.PartitionTempDirectory = Path.Combine(codePackageTempDirectory, partitionId);

            var account = CloudStorageAccount.Parse(backupConnectionString);
            this.cloudBlobClient = account.CreateCloudBlobClient();
            this.backupBlobContainer = this.cloudBlobClient.GetContainerReference("backup");
            this.backupBlobContainer.CreateIfNotExists();
            this.scope = scope;
        }

        public TimeSpan BackupFrequency
        {
            get { return this.backupFrequency; }
        }

        public async Task ArchiveBackupAsync(BackupInfo backupInfo, CancellationToken cancellationToken)
        {
            CommonEventSource.Current.Info("AzureBlobBackupManager: Archive Called.", this.scope);

            string fullArchiveDirectory = Path.Combine(this.PartitionTempDirectory, Guid.NewGuid().ToString("N"));

            DirectoryInfo fullArchiveDirectoryInfo = new DirectoryInfo(fullArchiveDirectory);
            fullArchiveDirectoryInfo.Create();

            string blobName = string.Format("{0}_{1}_{2}_{3}", Guid.NewGuid().ToString("N"), this.keyMin, this.keyMax, "Backup.zip");
            string fullArchivePath = Path.Combine(fullArchiveDirectory, "Backup.zip");

            ZipFile.CreateFromDirectory(backupInfo.Directory, fullArchivePath, CompressionLevel.Fastest, false);

            DirectoryInfo backupDirectory = new DirectoryInfo(backupInfo.Directory);
            backupDirectory.Delete(true);

            CloudBlockBlob blob = this.backupBlobContainer.GetBlockBlobReference(this.partitionId + "/" + blobName);
            await blob.UploadFromFileAsync(fullArchivePath,  CancellationToken.None);

            DirectoryInfo tempDirectory = new DirectoryInfo(fullArchiveDirectory);
            tempDirectory.Delete(true);

            CommonEventSource.Current.Info("AzureBlobBackupManager: UploadBackupFolderAsync: success.", this.scope);
        }

        public async Task<string> RestoreLatestBackupToTempLocation(CancellationToken cancellationToken)
        {
            CommonEventSource.Current.Info("AzureBlobBackupManager: Download backup async called.", this.scope);

            CloudBlockBlob lastBackupBlob = (await this.GetBackupBlobs(true)).First();

            CommonEventSource.Current.Info("AzureBlobBackupManager: Downloading " + lastBackupBlob.Name, this.scope);

            string downloadId = Guid.NewGuid().ToString("N");

            string zipPath = Path.Combine(this.PartitionTempDirectory, string.Format("{0}_Backup.zip", downloadId));

            lastBackupBlob.DownloadToFile(zipPath, FileMode.CreateNew);

            string restorePath = Path.Combine(this.PartitionTempDirectory, downloadId);

            ZipFile.ExtractToDirectory(zipPath, restorePath);

            FileInfo zipInfo = new FileInfo(zipPath);
            zipInfo.Delete();

            CommonEventSource.Current.Info("AzureBlobBackupManager: Downloaded " + lastBackupBlob.Name + " in " + restorePath, this.scope);

            return restorePath;
        }

        public async Task DeleteBackupsAsync(CancellationToken cancellationToken)
        {
            if (this.backupBlobContainer.Exists())
            {
                CommonEventSource.Current.Info("AzureBlobBackupManager: Deleting old backups", this.scope);

                IEnumerable<CloudBlockBlob> oldBackups = (await this.GetBackupBlobs(true)).Skip(this.MaxBackupsToKeep);

                foreach (CloudBlockBlob backup in oldBackups)
                {
                    CommonEventSource.Current.Info("AzureBlobBackupManager: Deleting " + backup.Name, this.scope);
                    await backup.DeleteAsync(cancellationToken);
                }
            }
        }

        private async Task<IEnumerable<CloudBlockBlob>> GetBackupBlobs(bool sorted)
        {
            IEnumerable<IListBlobItem> blobs = this.backupBlobContainer.ListBlobs(this.partitionId + "/", true);

            CommonEventSource.Current.Info("AzureBlobBackupManager: Got blobs: " + blobs.Count(), this.scope);

            List<CloudBlockBlob> itemizedBlobs = new List<CloudBlockBlob>();

            foreach (CloudBlockBlob cbb in blobs)
            {
                await cbb.FetchAttributesAsync();
                itemizedBlobs.Add(cbb);
            }

            if (sorted)
            {
                return itemizedBlobs.OrderByDescending(x => x.Properties.LastModified);
            }
            else
            {
                return itemizedBlobs;
            }
        }
    }
}