using System;
using System.Net;
using System.Web.Cors;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using Carbon.Business;
using Carbon.Framework.Logging;
using Carbon.Framework.Util;
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
        public IDependencyContainer Container { get; private set; }

        public StorageStartup() : this(null)
        {

        }
        public StorageStartup(Action<IDependencyContainer> addons)
        {
            Container = NinjectConfig.Configure(addons);
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
            var logService = Container.Resolve<ILogService>();
            var appSettings = Container.Resolve<AppSettings>();

            app.Use(typeof (NinjectMiddleware), Container);

            if (string.IsNullOrEmpty(basePath))
            {
                app.UseLogAdapter(Container.Resolve<ILogService>());
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
                signalr.RunSignalR(SignalrConfig.Configure(logService, Container, appSettings));
            });
            app.Map("/api", apiApp =>
            {
                apiApp.UseWebApi(CommonWebApiConfig.Register(typeof (StorageStartup).Assembly, basePath + "/api"));

            });

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }
    }
}