using System;
using Microsoft.ApplicationInsights.Extensibility;
using Carbon.Business;
using Microsoft.ApplicationInsights.Channel;

namespace Carbon.Owin.Common.Logging
{
    public class TelemetryInitializer : ITelemetryInitializer
    {
        private readonly AppSettings _appSettings;

        public TelemetryInitializer(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Component.Version = Defs.Config.VERSION.ToString();
            telemetry.Context.Cloud.RoleName = _appSettings.RoleName;
            telemetry.Context.Cloud.RoleInstance = Environment.MachineName;
        }
    }
}