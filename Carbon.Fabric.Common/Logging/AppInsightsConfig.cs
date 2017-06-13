using Carbon.Business;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;

namespace Carbon.Fabric.Common.Logging
{
    public static class AppInsightsConfig
    {
        public static void Configure(string instrumentationKey)
        {
            TelemetryConfiguration.Active.InstrumentationKey = instrumentationKey;
            TelemetryConfiguration.Active.TelemetryInitializers.Add(new TelemetryInitializer());

            TelemetryConfiguration.Active.TelemetryProcessorChainBuilder.Use(next => new TelemetryProcessor(next));
            TelemetryConfiguration.Active.TelemetryProcessorChainBuilder.Build();
        }
    }

    public class TelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Component.Version = Defs.Config.VERSION.ToString();
            telemetry.Context.Cloud.RoleInstance = Environment.MachineName;
        }
    }

    public class TelemetryProcessor : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor _next;

        public TelemetryProcessor(ITelemetryProcessor next)
        {
            _next = next;
        }

        public void Process(ITelemetry item)
        {
            var telemetry = item as ISupportProperties;
            if (telemetry != null)
            {
                var env = Environment.GetEnvironmentVariable("Carbon_Env");
                if (string.IsNullOrEmpty(env))
                {
                    env = "local";
                }
                item.Context.Cloud.RoleName = env;
            }
            _next.Process(item);
        }
    }
}
