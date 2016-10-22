using System;
using System.IO;
using Carbon.Business;
using Carbon.Data.Azure.Scheduler;
using Carbon.Framework.Util;
using Microsoft.Owin;
using Owin;
using Carbon.Owin.Common.Data;
using Carbon.Owin.Common.Dependencies;
using Carbon.Owin.Common.Logging;
using Carbon.Owin.Common.Security;
using Carbon.Owin.Common.WebApi;
using Carbon.Services;
using Carbon.Services.IdentityServer;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;

[assembly: OwinStartup(typeof(ServicesStartup))]

namespace Carbon.Services
{
    public class ServicesStartup
    {
        private readonly Action<IDependencyContainer> _addons;

        public ServicesStartup()
        {

        }
        public ServicesStartup(Action<IDependencyContainer> addons)
        {
            _addons = addons;
        }

        public void Configuration(IAppBuilder app)
        {
            var container = ServiceDependencyConfiguration.Configure(_addons);
            var appSettings = container.Resolve<AppSettings>();

            DataLayerConfig.ConfigureStandalone(container, appSettings);
            JobSchedulingConfig.Register(container);

            app.UseTelemetry(appSettings);
            app.Use(typeof(NinjectMiddleware), container);
            
            app.Map("/idsrv", idsrv =>
            {
                //temp middleware to set acs headers which identityserver does not expose
                idsrv.Use(async (ctx, next) =>
                {
                    ctx.Response.OnSendingHeaders(state =>
                    {
                        var resp = (OwinResponse)state;
                        resp.Headers["Access-Control-Allow-Credentials"] = "true";
                    }, ctx.Response);
                    await next.Invoke();
                });

                IdentityServerConfig.Configure(idsrv, container, appSettings);                
                
                //userId needs to run on idsrv path to get auth cookie for token renewal
                idsrv.Map("/ext", ext =>
                {
                    ext.UseAccessToken(appSettings);                    
                    ext.UseWebApi(IdentityWebApiConfig.Register());
                });
            });
            
            //app.Map("/storage", storage => new StorageStartup().ConfigureAsEmbedded(storage, "/storage"));
            app.Map("/api", api =>
            {                
                api.UseAccessToken(appSettings);                
                api.UseWebApi(CommonWebApiConfig.Register(typeof(ServicesStartup).Assembly, "/api"));
            });            

            var dataProvider = container.Resolve<DataProvider>();
            SetupFileSystem(app, dataProvider, "/target", "target");
            SetupFileSystem(app, dataProvider, "/fonts", @"target\fonts");

            app.UseWebApi(HtmlWebApiConfig.Register());
        }

        private static void SetupFileSystem(IAppBuilder app, DataProvider dataProvider, string pathString, string physicalPath)
        {
            var resolvedPath = dataProvider.ResolvePath(Defs.Packages.Client, physicalPath);
            if (Directory.Exists(resolvedPath))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    RequestPath = new PathString(pathString),
                    FileSystem = new PhysicalFileSystem(resolvedPath)
                });
            }            
        }
    }
}