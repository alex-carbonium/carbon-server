using Microsoft.Owin.Diagnostics;
using Microsoft.Owin.Logging;
using Owin;
using Carbon.Framework.Logging;

namespace Carbon.Owin.Common.Logging
{
    public static class OwinLoggingExtensions
    {
        public static IAppBuilder UseLogAdapter(this IAppBuilder app, ILogService logService)
        {
            app.SetLoggerFactory(new OwinLogFactoryAdapter(logService));
            app.UseErrorPage(
#if DEBUG
                ErrorPageOptions.ShowAll
#endif
                );
            return app;
        }
    }
}