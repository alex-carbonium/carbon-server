using System.Collections.Generic;
using System.Fabric;
using Carbon.Business;
using Carbon.Business.Services;
using Carbon.Fabric.Common;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Carbon.Fabric.Common.Logging;
using Carbon.Framework.Logging;

namespace Carbon.StorageService.FabricHost
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class StorageFabricHost : StatelessService
    {
        public StorageFabricHost(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            var configuration = new FabricConfiguration(Context);
            var start = new StorageStartup(container =>
            {
                container.RegisterInstance<ServiceContext>(Context);
                container.RegisterInstance<Configuration>(configuration);
                container.RegisterTypeSingleton<DataProvider, FabricDataProvider>();
                container.RegisterTypeSingleton<ILogService, EventSourceLogService>();
                container.RegisterInstance<IActorFabric>(ActorFabric.Default);
            });

            return new[]
            {
                new ServiceInstanceListener(serviceContext => new BaseCommunicationListener(start.Configuration, serviceContext, configuration.GetString("Endpoints", "Listen"), start.Container.BeginScope()))
            };
        }
    }
}
