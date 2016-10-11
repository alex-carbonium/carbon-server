using Microsoft.WindowsAzure.Storage;
using Carbon.Business;
using Carbon.Business.CloudDomain;
using Carbon.Business.Domain;
using Carbon.Business.Services;
using Carbon.Data;
using Carbon.Data.Azure;
using Carbon.Data.Azure.Blob;
using Carbon.Data.Azure.Table;
using Carbon.Data.UnitOfWorkImpl;
using Carbon.Framework.Cloud;
using Carbon.Framework.Repositories;
using Carbon.Framework.UnitOfWork;
using Carbon.Framework.Util;

namespace Carbon.Owin.Common.Data
{
    public class DataLayerConfig
    {
        public static string GetConnectionString(AppSettings appSettings)
        {
            return appSettings.GetConnectionString(StorageTypeConfig.GetConnectionStringName());
        }

        public static void ConfigureStandalone(IDependencyContainer container, AppSettings appSettings)
        {            
            //var connectionString = GetConnectionString(appSettings);
            //var codeVersion = Defs.Config.VERSION;
            //NHibernateConfig.ConfigureAndMigrate(connectionString, container, codeVersion);
        }

        public static void ConfigureEmbedded(IDependencyContainer container)
        {
            //NHibernateConfig.RegisterSessionFactory(container);
        }

        public static void RegisterImplementation(IDependencyContainer container)
        {
            var appSettings = container.Resolve<AppSettings>();
            var storageConnectionString = appSettings.GetConnectionString("nosql"); 
            var storageAccount = storageConnectionString != null ? CloudStorageAccount.Parse(storageConnectionString) : CloudStorageAccount.DevelopmentStorageAccount;
            var azureTableClient = storageAccount.CreateCloudTableClient();
            var azureQueueClient = storageAccount.CreateCloudQueueClient();
            var azureBlobClient = storageAccount.CreateCloudBlobClient();

            container
                .RegisterTypePerWebRequest<ICloudUnitOfWork, AzureUnitOfWork>()
                .RegisterTypePerWebRequest<IUnitOfWork, CompositeUnitOfWork>()                
                //.RegisterTypePerWebRequest<ICampaignRepository, CampaignRepository>()
                //.RegisterTypeSingleton<IMailService, MailService>()
                .RegisterTypePerWebRequest<CampaignService, CampaignService>()
                .RegisterFactory<IUnitOfWorkFactory>()
                .RegisterFactory<ICloudUnitOfWorkFactory>()
                .RegisterFactory<IBlobRepositoryFactory>((methodInfo, args) =>
                    typeof(BlobRepository<>).MakeGenericType(methodInfo.ReturnType.GenericTypeArguments))

                .RegisterTypeSingleton<IRepository<ProjectModel>, CloudProjectModelRepository>()
                .RegisterTypeSingleton<IRepository<ProjectSnapshot>, BlobRepository<ProjectSnapshot>>()
                .RegisterTypeSingleton<IRepository<ProjectLog>, FatEntityRepository<ProjectLog>>()
                .RegisterTypeSingleton<IRepository<ProjectState>, TableRepository<ProjectState>>()
                .RegisterTypeSingleton<IRepository<CompanyNameRegistry>, TableRepository<CompanyNameRegistry>>()                
                
                .RegisterInstance(storageAccount)
                .RegisterInstance(azureQueueClient)
                .RegisterInstance(azureBlobClient)
                .RegisterInstance(azureTableClient);
        }
    }
}