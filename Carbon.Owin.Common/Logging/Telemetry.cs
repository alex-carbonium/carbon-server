using Carbon.Business;
using Microsoft.ApplicationInsights.Extensibility;
using Owin;

namespace Carbon.Owin.Common.Logging
{
    public static class Telemetry
    {
        public static IAppBuilder UseTelemetry(this IAppBuilder app, AppSettings appSettings)
        {
            TelemetryConfiguration.Active.InstrumentationKey = appSettings.Azure.TelemetryKey;
            TelemetryConfiguration.Active.TelemetryInitializers.Add(new TelemetryInitializer(appSettings));
            return app;
        }        
    }
}
