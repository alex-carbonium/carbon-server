using System;
using Carbon.Business;
using Ninject;
using Carbon.Business.Domain;
using Carbon.Business.Services;
using Carbon.Framework.Logging;
using Carbon.Framework.Util;
using Carbon.Owin.Common.Data;
using Carbon.Owin.Common.Security;

namespace Carbon.StorageService.Dependencies
{
    public class StorageDependencyConfig
    {
        public static IDependencyContainer Configure(Action<IDependencyContainer> addons)
        {
            return Configure(new StandardKernel(), addons);
        }

        public static IDependencyContainer Configure(IKernel kernel, Action<IDependencyContainer> addons)
        {
            Container = CreateContainer(kernel, addons);
            return Container;
        }

        public static IDependencyContainer CreateContainer()
        {
            return CreateContainer(new StandardKernel());
        }

        public static IDependencyContainer CreateContainer(IKernel kernel, Action<IDependencyContainer> addons = null)
        {
            var container = new NinjectDependencyContainer(kernel);
            container                
                .RegisterTypeSingleton<AppSettings, AppSettings>()
                .RegisterType<ProjectModelService, ProjectModelService>()                                                
                .RegisterTypePerWebRequest<IIdentityContext, IdentityContext>()                                
                .RegisterTypeSingleton<ILogService, TelemetryLogService>()
                .RegisterTypeSingleton<FontManager, FontManager>();

            addons?.Invoke(container);

            DataLayerConfig.RegisterImplementation(container);

            return container;
        }

        public static IDependencyContainer Container { get; private set; }
    }
}