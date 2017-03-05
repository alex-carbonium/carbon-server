using System;
using System.Net;
using System.Web.Cors;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using Carbon.Business;
using Carbon.Data.Azure.Scheduler;
using Carbon.Framework.Logging;
using Carbon.Framework.Util;
using Carbon.Owin.Common.Data;
using Carbon.Owin.Common.Dependencies;
using Carbon.Owin.Common.Logging;
using Carbon.Owin.Common.Security;
using Carbon.Owin.Common.WebApi;
using Carbon.StorageService;
using Carbon.StorageService.Dependencies;
using Carbon.StorageService.SignalR;

[assembly: OwinStartup(typeof(StorageStartup))]

namespace Carbon.StorageService
{
    public class StorageStartup
    {
        private readonly Action<IDependencyContainer> _addons;

        public StorageStartup()
        {

        }
        public StorageStartup(Action<IDependencyContainer> addons)
        {
            _addons = addons;
        }

        public void Configuration(IAppBuilder app)
        {
            Configure(app);
        }

        public void ConfigureAsEmbedded(IAppBuilder app, string basePath)
        {
            Configure(app, basePath);
        }

        private void Configure(IAppBuilder app, string basePath = "")
        {
            var container = NinjectConfig.Configure(_addons);

            var logService = container.Resolve<ILogService>();
            var appSettings = container.Resolve<AppSettings>();

            logService.SetGlobalContextProperty("build", Defs.Config.VERSION.ToString());

            app.UseTelemetry(appSettings);

            app.Use(typeof (NinjectMiddleware), container);

            JobSchedulingConfig.Register(container);

            //embedded
            if (!string.IsNullOrEmpty(basePath))
            {
                DataLayerConfig.ConfigureEmbedded(container);
            }
            else
            {
                app.UseLogAdapter(container.Resolve<ILogService>());
                DataLayerConfig.ConfigureStandalone(container, appSettings);
            }

            app.UseAccessToken(appSettings);

            app.Map("/signalr", signalr =>
            {
                var corsPolicy = new CorsPolicy()
                {
                    AllowAnyHeader = true,
                    AllowAnyMethod = false,
                    AllowAnyOrigin = false,
                    Methods = { "GET", "POST", "OPTIONS"}
                };
                foreach (var origing in AllowedOrigins.All)
                {
                    corsPolicy.Origins.Add(origing);
                }
                var corsOptions = new CorsOptions
                {
                    PolicyProvider = new CorsPolicyProvider { PolicyResolver = context => System.Threading.Tasks.Task.FromResult(corsPolicy) }
                };
                signalr.UseCors(corsOptions);
                signalr.RunSignalR(SignalrConfig.Configure(logService, container, appSettings));
            });
            app.Map("/api", apiApp =>
            {
                apiApp.UseWebApi(CommonWebApiConfig.Register(typeof (StorageStartup).Assembly, basePath + "/api"));

            });

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }
    }
}