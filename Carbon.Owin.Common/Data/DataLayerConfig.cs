using Microsoft.WindowsAzure.Storage;
using Carbon.Business;
using Carbon.Business.CloudDomain;
using Carbon.Business.Domain;
using Carbon.Business.Services;
using Carbon.Data.Azure;
using Carbon.Data.Azure.Blob;
using Carbon.Data.Azure.Table;
using Carbon.Framework.Cloud;
using Carbon.Framework.Repositories;
using Carbon.Framework.UnitOfWork;
using Carbon.Framework.Util;

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

            container
                .RegisterTypePerWebRequest<ICloudUnitOfWork, AzureUnitOfWork>()
                //.RegisterTypePerWebRequest<IUnitOfWork, CompositeUnitOfWork>()
                .RegisterFactory<IUnitOfWorkFactory>()
                //.RegisterTypePerWebRequest<ICampaignRepository, CampaignRepository>()
                //.RegisterTypePerWebRequest<CampaignService, CampaignService>()
                .RegisterFactory<IUnitOfWorkFactory>()
                .RegisterFactory<ICloudUnitOfWorkFactory>()
                .RegisterFactory<IBlobRepositoryFactory>((methodInfo, args) =>
                    typeof(BlobRepository<>).MakeGenericType(methodInfo.ReturnType.GenericTypeArguments))

                .RegisterTypeSingleton<IRepository<ProjectModel>, CloudProjectModelRepository>()
                .RegisterTypeSingleton<IRepository<ProjectSnapshot>, BlobRepository<ProjectSnapshot>>()
                .RegisterTypeSingleton<IRepository<ProjectLog>, FatEntityRepository<ProjectLog>>()
                .RegisterTypeSingleton<IRepository<ProjectState>, TableRepository<ProjectState>>()
                .RegisterTypeSingleton<IRepository<ActiveProject>, TableRepository<ActiveProject>>()
                .RegisterTypeSingleton<IRepository<PasswordResetToken>, TableRepository<PasswordResetToken>>()
                .RegisterTypeSingleton<IRepository<CompanyNameRegistry>, TableRepository<CompanyNameRegistry>>()

                .RegisterInstance(storageAccount)
                .RegisterInstance(azureBlobClient)
                .RegisterInstance(azureTableClient)
                .RegisterInstance<IMailService>(new QueueMailService(jobsQueueClient));
        }
    }
}