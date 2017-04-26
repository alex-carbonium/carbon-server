using System;
using Ninject;
using Carbon.Framework.Util;

namespace Carbon.StorageService.Dependencies
{
    public class NinjectConfig
    {
        public static IDependencyContainer Configure(Action<IDependencyContainer> addons)
        {
            Kernel = new StandardKernel();

            var container = StorageDependencyConfig.Configure(Kernel, addons);

            return container;
        }

        public static IKernel Kernel { get; private set; }
    }
}