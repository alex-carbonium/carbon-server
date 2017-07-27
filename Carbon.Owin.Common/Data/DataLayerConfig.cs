using Microsoft.WindowsAzure.Storage;
using Carbon.Business;
using Carbon.Business.CloudDomain;
using Carbon.Business.Domain;
using Carbon.Business.Services;
using Carbon.Data.Azure;
using Carbon.Data.Azure.Blob;
using Carbon.Data.Azure.Table;
using Carbon.Framework.Repositories;
using Carbon.Framework.Util;
using Carbon.Data.RepositoryImpl;

namespace Carbon.Owin.Common.Data
{
    public class DataLayerConfig
    {
        public static void RegisterImplementation(IDependencyContainer container)
        {
            var appSettings = container.Resolve<AppSettings>();
            var storageConnectionString = appSettings.GetConnectionString("nosql");
            var storageAccount = storageConnectionString != null ? CloudStorageAccount.Parse(storageConnectionString) : CloudStorageAccount.DevelopmentStorageAccount;
            var azureTableClient = storageAccount.CreateCloudTableClient();
            var azureBlobClient = storageAccount.CreateCloudBlobClient();

            var jobsConnectionString = appSettings.GetConnectionString("jobs");
            var jobsAccount = jobsConnectionString != null ? CloudStorageAccount.Parse(jobsConnectionString) : CloudStorageAccount.DevelopmentStorageAccount;
            var jobsQueueClient = jobsAccount.CreateCloudQueueClient();

            var cdnConnectionString = appSettings.GetConnectionString("cdn");
            var cdnAccount = cdnConnectionString != null ? CloudStorageAccount.Parse(cdnConnectionString) : CloudStorageAccount.DevelopmentStorageAccount;
            var cdnBlobClient = cdnAccount.CreateCloudBlobClient();

            container
                //.RegisterTypePerWebRequest<ICampaignRepository, CampaignRepository>()
                //.RegisterTypePerWebRequest<CampaignService, CampaignService>()

                .RegisterTypeSingleton<IRepository<ProjectModel>, CloudProjectModelRepository>()
                .RegisterTypeSingleton<IRepository<ProjectSnapshot>, BlobRepository<ProjectSnapshot>>()
                .RegisterTypeSingleton<IRepository<ProjectLog>, FatEntityRepository<ProjectLog>>()
                .RegisterTypeSingleton<IRepository<ProjectState>, TableRepository<ProjectState>>()
                .RegisterTypeSingleton<IRepository<ActiveProject>, TableRepository<ActiveProject>>()
                .RegisterTypeSingleton<IRepository<PasswordResetToken>, TableRepository<PasswordResetToken>>()
                .RegisterTypeSingleton<IRepository<CompanyNameRegistry>, TableRepository<CompanyNameRegistry>>()
                .RegisterTypeSingleton<IRepository<ShareToken>, TableRepository<ShareToken>>()
                .RegisterTypeSingleton<IPublicSharedPageRepository, PublicSharedPageRepository>()
                .RegisterTypeSingleton<IPrivateSharedPageRepository, PrivateSharedPageRepository>()
                .RegisterTypeSingleton<IRepository<FeatureSubscription>, TableRepository<FeatureSubscription>>()
                .RegisterTypeSingleton<IRepository<BetaSubscription>, TableRepository<BetaSubscription>>()
                .RegisterTypeSingleton<IRepository<CompanyFile>, BlobRepository<CompanyFile>>()
                .RegisterInstance<IRepository<File>>(new BlobRepository<File>(cdnBlobClient))

                .RegisterInstance(storageAccount)
                .RegisterInstance(azureBlobClient)
                .RegisterInstance(azureTableClient)
                .RegisterInstance<IMailService>(new QueueMailService(jobsQueueClient));
        }
    }
}