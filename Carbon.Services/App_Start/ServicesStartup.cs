using System;
using System.IO;
using Carbon.Business;
using Carbon.Data.Azure.Scheduler;
using Carbon.Framework.Util;
using Microsoft.Owin;
using Owin;
using Carbon.Owin.Common.Dependencies;
using Carbon.Owin.Common.Security;
using Carbon.Owin.Common.WebApi;
using Carbon.Services;
using Carbon.Services.IdentityServer;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Carbon.Business.Domain;
using Newtonsoft.Json.Linq;

[assembly: OwinStartup(typeof(ServicesStartup))]

namespace Carbon.Services
{
    public class ServicesStartup
    {
        public IDependencyContainer Container { get; private set; }

        public ServicesStartup() : this(null)
        {

        }
        public ServicesStartup(Action<IDependencyContainer> addons)
        {
            Container = ServiceDependencyConfiguration.Configure(addons);
        }

        public void Configuration(IAppBuilder app)
        {
            var appSettings = Container.Resolve<AppSettings>();

            JobSchedulingConfig.Register(Container);

            app.Use(typeof(NinjectMiddleware), Container);

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

                IdentityServerConfig.Configure(idsrv, Container, appSettings);

                //userId needs to run on idsrv path to get auth cookie for token renewal
                idsrv.Map("/ext", ext =>
                {
                    ext.UseAccessToken(appSettings);
                    ext.UseWebApi(IdentityWebApiConfig.Register());
                });
            });

            app.Map("/api", api =>
            {
                api.UseAccessToken(appSettings);
                api.UseWebApi(CommonWebApiConfig.Register(typeof(ServicesStartup).Assembly, "/api"));
            });

            var dataProvider = Container.Resolve<DataProvider>();
            SetupFileSystem(app, dataProvider, "/target", "target");
            SetupFileSystem(app, dataProvider, "/fonts", @"target\fonts");

            InitializeFontManager(Container, appSettings);

            app.UseWebApi(HtmlWebApiConfig.Register());

            ApplicationUserManager.StartupAsync(appSettings);
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

        private static void InitializeFontManager(IDependencyContainer container, AppSettings appSettings)
        {
            var fontManager = container.Resolve<FontManager>();
            var txt = File.ReadAllText(appSettings.ResolvePath(Defs.Packages.Data, "systemFonts.json"));
            var fonts = JObject.Parse(txt);
            fontManager.Initialize(fonts);
        }
    }
}