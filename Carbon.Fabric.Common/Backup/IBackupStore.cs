using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using System;

namespace Carbon.Fabric.Common.Backup
{
    public interface IBackupStore
    {
        TimeSpan BackupFrequency { get; }

        Task ArchiveBackupAsync(BackupInfo backupInfo, CancellationToken cancellationToken);

        Task<string> RestoreLatestBackupToTempLocation(CancellationToken cancellationToken);

        Task DeleteBackupsAsync(CancellationToken cancellationToken);
    }
}