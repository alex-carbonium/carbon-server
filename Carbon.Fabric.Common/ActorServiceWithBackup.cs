using Microsoft.ServiceFabric.Actors.Runtime;
using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using System.Fabric;
using System.Threading;
using Microsoft.ServiceFabric.Data;
using Carbon.Fabric.Common.Backup;
using System.IO;
using System.Fabric.Description;
using Carbon.Fabric.Common.Logging;
using Carbon.Framework.Util;
using Ninject;
using Carbon.Business;

namespace Carbon.Fabric.Common
{
    public class ActorServiceWithBackup : ActorService
    {
        private IBackupStore backupManager;
        private BackupManagerType backupStorageType;
        private IDependencyContainer scope;

        public ActorServiceWithBackup(StatefulServiceContext context, ActorTypeInformation actorTypeInfo, Func<ActorService, ActorId, ActorBase> actorFactory = null, Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null, IActorStateProvider stateProvider = null, ActorServiceSettings settings = null) : base(context, actorTypeInfo, actorFactory, stateManagerFactory, stateProvider, settings)
        {
            var appSettings = new AppSettings(new FabricConfiguration(context), new FabricDataProvider(context));

            var container = new NinjectDependencyContainer(new StandardKernel());
            container.RegisterInstance<ServiceContext>(context);
            container.RegisterInstance(appSettings);
            scope = container.BeginScope();
        }

        protected override Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                CommonEventSource.Current.Info("inside RunAsync for Inventory Service", scope);

                return Task.WhenAll(this.PeriodicTakeBackupAsync(cancellationToken));
            }
            catch (Exception e)
            {
                CommonEventSource.Current.Fatal(e, scope);
                throw;
            }
        }

        //Dataloss testing can be triggered via powershell. To do so, run the following commands as a script
        //Connect-ServiceFabricCluster
        //$s = "fabric:/Carbon.Fabric/CompanyActorService"
        //$p = Get-ServiceFabricApplication | Get-ServiceFabricService -ServiceName $s | Get-ServiceFabricPartition | Select -First 1
        //$p | Invoke-ServiceFabricPartitionDataLoss -DataLossMode FullDataLoss -ServiceName $s
        protected override async Task<bool> OnDataLossAsync(RestoreContext restoreCtx, CancellationToken cancellationToken)
        {
            CommonEventSource.Current.Info("OnDataLoss Invoked!", scope);
            this.SetupBackupManager();

            try
            {
                string backupFolder;

                if (this.backupStorageType == BackupManagerType.None)
                {
                    //since we have no backup configured, we return false to indicate
                    //that state has not changed. This replica will become the basis
                    //for future replica builds
                    return false;
                }
                else
                {
                    backupFolder = await this.backupManager.RestoreLatestBackupToTempLocation(cancellationToken);
                }

                CommonEventSource.Current.Info("Restoration Folder Path " + backupFolder, scope);

                RestoreDescription restoreRescription = new RestoreDescription(backupFolder, RestorePolicy.Force);

                await restoreCtx.RestoreAsync(restoreRescription, cancellationToken);

                CommonEventSource.Current.Info("Restore completed", scope);

                DirectoryInfo tempRestoreDirectory = new DirectoryInfo(backupFolder);
                tempRestoreDirectory.Delete(true);

                return true;
            }
            catch (Exception e)
            {
                CommonEventSource.Current.Fatal(e, scope);

                throw;
            }
        }

        private async Task PeriodicTakeBackupAsync(CancellationToken cancellationToken)
        {
            long backupsTaken = 0;
            this.SetupBackupManager();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (this.backupStorageType == BackupManagerType.None)
                {
                    break;
                }
                else
                {
                    var backupDescription = new BackupDescription(BackupOption.Full, this.BackupCallbackAsync);

                    await this.BackupAsync(backupDescription, TimeSpan.FromHours(1), cancellationToken);

                    backupsTaken++;

                    CommonEventSource.Current.Info("Backup taken: " + backupsTaken, scope);

                    await Task.Delay(this.backupManager.BackupFrequency, cancellationToken);
                }
            }
        }

        private async Task<bool> BackupCallbackAsync(BackupInfo backupInfo, CancellationToken cancellationToken)
        {
            CommonEventSource.Current.Info("Inside backup callback for replica " + this.Context.PartitionId + "|" + this.Context.ReplicaId, scope);

            try
            {
                CommonEventSource.Current.Info("Archiving backup", scope);
                await this.backupManager.ArchiveBackupAsync(backupInfo, cancellationToken);
                CommonEventSource.Current.Info("Backup archived", scope);
            }
            catch (Exception e)
            {
                CommonEventSource.Current.Fatal(e, scope);
            }

            await this.backupManager.DeleteBackupsAsync(cancellationToken);

            CommonEventSource.Current.Info("Backups deleted", scope);

            return true;
        }

        private void SetupBackupManager()
        {
            string partitionId = this.Context.PartitionId.ToString("N");
            long minKey = ((Int64RangePartitionInformation)this.Partition.PartitionInfo).LowKey;
            long maxKey = ((Int64RangePartitionInformation)this.Partition.PartitionInfo).HighKey;

            if (this.Context.CodePackageActivationContext != null)
            {
                ICodePackageActivationContext codePackageContext = this.Context.CodePackageActivationContext;
                ConfigurationPackage configPackage = codePackageContext.GetConfigurationPackageObject("Config");
                ConfigurationSection configSection = configPackage.Settings.Sections["Backup.Settings"];

                string backupSettingValue = configSection.Parameters["BackupMode"].Value;

                if (string.Equals(backupSettingValue, "none", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.backupStorageType = BackupManagerType.None;
                }
                else if (string.Equals(backupSettingValue, "azure", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.backupStorageType = BackupManagerType.Azure;

                    ConfigurationSection azureBackupConfigSection = configPackage.Settings.Sections["Backup.Azure"];

                    this.backupManager = new AzureBlobBackupManager(scope, azureBackupConfigSection, partitionId, minKey, maxKey, codePackageContext.TempDirectory);
                }
                else if (string.Equals(backupSettingValue, "local", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.backupStorageType = BackupManagerType.Local;

                    ConfigurationSection localBackupConfigSection = configPackage.Settings.Sections["Backup.Local"];

                    this.backupManager = new DiskBackupManager(localBackupConfigSection, partitionId, minKey, maxKey, codePackageContext.TempDirectory);
                }
                else
                {
                    throw new ArgumentException("Unknown backup type");
                }

                CommonEventSource.Current.Info("Backup Manager Set Up", scope);
            }
        }

        private enum BackupManagerType
        {
            Azure,
            Local,
            None
        }

    }
}
