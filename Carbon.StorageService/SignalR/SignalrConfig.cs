using Carbon.Business;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Carbon.Framework.Logging;
using Carbon.Framework.Util;

namespace Carbon.StorageService.SignalR
{
    public static class SignalrConfig
    {
        public static HubConfiguration Configure(ILogService logService, IDependencyContainer container, AppSettings appSettings)
        {
            GlobalHost.DependencyResolver.Register(typeof(IHubActivator), () => new NinjectHubActivator(container));

            GlobalHost.HubPipeline.AddModule(new CustomPipeline(logService));
            GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = 10*1024*1024;

            var config = new HubConfiguration()
            {
#if DEBUG
                EnableDetailedErrors = true
#endif
            };

            return config;
        }
    }
}