using Microsoft.AspNet.SignalR.Hubs;
using Carbon.Framework.Util;
using Carbon.Framework.Logging;

namespace Carbon.StorageService.SignalR
{
    public class NinjectHubActivator : IHubActivator
    {
        private readonly IDependencyContainer _container;

        public NinjectHubActivator(IDependencyContainer container)
        {
            _container = container;
        }

        public IHub Create(HubDescriptor descriptor)
        {
            IDependencyContainer scope = null;
            try
            {
                scope = _container.BeginScope();

                var hub = (BaseHub)scope.Resolve(descriptor.HubType);
                hub.Scope = scope;
                hub.Operation = scope.Resolve<OperationContext>();
                return hub;
            }
            catch
            {
                if (scope != null)
                {
                    scope.Dispose();
                }

                throw;
            }
        }
    }
}