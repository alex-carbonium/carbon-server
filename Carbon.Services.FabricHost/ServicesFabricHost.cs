using System.Collections.Generic;
using System.Fabric;
using Carbon.Business;
using Carbon.Business.Services;
using Carbon.Fabric.Common;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Carbon.Services.FabricHost
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class ServicesFabricHost : StatelessService
    {
        public ServicesFabricHost(StatelessServiceContext context)
            : base(context)
        {
            context.CodePackageActivationContext.DataPackageModifiedEvent += OnDataPackageModified;
        }

        private static void OnDataPackageModified(object sender, PackageModifiedEventArgs<DataPackage> e)
        {
            var resourceCache = ServiceDependencyConfiguration.Container.Resolve<ResourceCache>();
            resourceCache.Clear();
        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            var configuration = new FabricConfiguration(Context);
            var start = new ServicesStartup(container =>
            {
                container.RegisterInstance<Configuration>(configuration);
                container.RegisterInstance<DataProvider>(new FabricDataProvider(Context));
                container.RegisterInstance<IActorFabric>(ActorFabric.Default);
            });

            return new[]
            {
                new ServiceInstanceListener(serviceContext => new BaseCommunicationListener(start.Configuration, serviceContext, ServiceEventSource.Current, configuration.GetString("Endpoints", "Listen")))
            };
        }
    }
}
