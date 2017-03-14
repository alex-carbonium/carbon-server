using System;
using Carbon.Business;
using Carbon.Business.CloudDomain;
using Ninject;
using Carbon.Business.Domain;
using Carbon.Business.Services;
using Carbon.Data.Azure.Blob;
using Carbon.Data.Azure.Table;
using Carbon.Framework.Logging;
using Carbon.Framework.Repositories;
using Carbon.Framework.Util;
using Carbon.Owin.Common.Data;
using Carbon.Owin.Common.Security;
using Carbon.Services.IdentityServer;

namespace Carbon.Services
{
    public static class ServiceDependencyConfiguration
    {
        public static IDependencyContainer Configure(Action<IDependencyContainer> addons)
        {
            var settings = new NinjectSettings();
            //settings.UseReflectionBasedInjection = true; //uncomment for instrumentaion
            return Configure(new StandardKernel(settings), addons);
        }

        public static IDependencyContainer Configure(IKernel kernel, Action<IDependencyContainer> addons)
        {
            Container = CreateContainer(kernel, addons);
            Kernel = kernel;
            return Container;
        }

        public static IDependencyContainer CreateContainer(Action<IDependencyContainer> addons)
        {
            return CreateContainer(new StandardKernel(), addons);
        }

        public static IDependencyContainer CreateContainer(IKernel kernel, Action<IDependencyContainer> addons = null)
        {
            var container = new NinjectDependencyContainer(kernel);
            container
                .RegisterTypeSingleton<AppSettings, AppSettings>()
                //.RegisterTypePerWebRequest<IValidator, DictionaryValidator>()
                .RegisterTypeSingleton<ProjectModelService, ProjectModelService>()
                .RegisterTypeSingleton<PermissionService, PermissionService>()
                .RegisterTypeSingleton<ActiveProjectTrackingService, ActiveProjectTrackingService>()
                .RegisterTypeSingleton<SharingService, SharingService>()
                .RegisterTypeSingleton<AccountService, AccountService>()
                .RegisterTypeSingleton<DataService, DataService>()
                .RegisterTypeSingleton<IRepository<ShareToken>, TableRepository<ShareToken>>()
                .RegisterTypeSingleton<IRepository<SharedPage>, TableRepository<SharedPage>>()
                .RegisterTypeSingleton<IRepository<FeatureSubscription>, TableRepository<FeatureSubscription>>()
                .RegisterTypeSingleton<IRepository<CompanyFile>, BlobRepository<CompanyFile>>()
                .RegisterTypeSingleton<ILogService, TelemetryLogService>()
                .RegisterTypeSingleton<ResourceCache, ResourceCache>()
                .RegisterTypeSingleton<FontManager, FontManager>()

                .RegisterTypePerWebRequest<ApplicationDbContext, ApplicationDbContext>()
                .RegisterTypePerWebRequest<ApplicationUserManager, ApplicationUserManager>()
                .RegisterTypePerWebRequest<ApplicationRoleManager, ApplicationRoleManager>()
                .RegisterTypePerWebRequest<IIdentityContext, IdentityContext>();

            addons?.Invoke(container);

            DataLayerConfig.RegisterImplementation(container);

            return container;
        }

        public static IDependencyContainer Container { get; private set; }
        public static IKernel Kernel { get; private set; }
    }
}