using System;
using System.Collections.Generic;
using System.Fabric.Description;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace Carbon.Fabric.Common.Backup
{
    public class DiskBackupManager : IBackupStore
    {
        private string PartitionArchiveFolder;
        private string PartitionTempDirectory;
        private TimeSpan backupFrequency;
        private int MaxBackupsToKeep;
        private long keyMin;
        private long keyMax;

        public DiskBackupManager(ConfigurationSection configSection, string partitionId, long keymin, long keymax, string codePackageTempDirectory)
        {
            this.keyMin = keymin;
            this.keyMax = keymax;

            string BackupArchivalPath = configSection.Parameters["BackupArchivalPath"].Value;
            this.backupFrequency = TimeSpan.Parse(configSection.Parameters["BackupFrequency"].Value);
            this.MaxBackupsToKeep = int.Parse(configSection.Parameters["MaxBackupsToKeep"].Value);

            this.PartitionArchiveFolder = Path.Combine(BackupArchivalPath, "Backups", partitionId);
            this.PartitionTempDirectory = Path.Combine(codePackageTempDirectory, partitionId);
        }

        public TimeSpan BackupFrequency
        {
            get { return this.backupFrequency; }
        }

        public Task ArchiveBackupAsync(BackupInfo backupInfo, CancellationToken cancellationToken)
        {
            string fullArchiveDirectory = Path.Combine(
                this.PartitionArchiveFolder,
                string.Format("{0}_{1}_{2}", Guid.NewGuid().ToString("N"), this.keyMin, this.keyMax));

            DirectoryInfo dirInfo = new DirectoryInfo(fullArchiveDirectory);
            dirInfo.Create();

            string fullArchivePath = Path.Combine(fullArchiveDirectory, "Backup.zip");

            ZipFile.CreateFromDirectory(backupInfo.Directory, fullArchivePath, CompressionLevel.Fastest, false);

            DirectoryInfo backupDirectory = new DirectoryInfo(backupInfo.Directory);
            backupDirectory.Delete(true);

            return Task.FromResult(true);
        }

        public Task<string> RestoreLatestBackupToTempLocation(CancellationToken cancellationToken)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(this.PartitionArchiveFolder);

            string backupZip = dirInfo.GetDirectories().OrderByDescending(x => x.LastWriteTime).First().FullName;

            string zipPath = Path.Combine(backupZip, "Backup.zip");

            DirectoryInfo directoryInfo = new DirectoryInfo(this.PartitionTempDirectory);
            if (directoryInfo.Exists)
            {
                directoryInfo.Delete(true);
            }

            directoryInfo.Create();

            ZipFile.ExtractToDirectory(zipPath, this.PartitionTempDirectory);

            return Task.FromResult<string>(this.PartitionTempDirectory);
        }

        public async Task DeleteBackupsAsync(CancellationToken cancellationToken)
        {
            await Task.Run(
                () =>
                {
                    if (!Directory.Exists(this.PartitionArchiveFolder))
                    {
                        //Nothing to delete; Backups may not even have been created for the partition
                        return;
                    }

                    DirectoryInfo dirInfo = new DirectoryInfo(this.PartitionArchiveFolder);

                    IEnumerable<DirectoryInfo> oldBackups = dirInfo.GetDirectories().OrderByDescending(x => x.LastWriteTime).Skip(this.MaxBackupsToKeep);

                    foreach (DirectoryInfo oldBackup in oldBackups)
                    {
                        oldBackup.Delete(true);
                    }

                    return;
                },
                cancellationToken);
        }
    }
}