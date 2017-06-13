using System;
using System.Threading;
using Carbon.Fabric.Common;
using Microsoft.ServiceFabric.Services.Runtime;
using Carbon.Fabric.Common.Logging;
using System.Fabric;
using Microsoft.Extensions.Configuration.ServiceFabric;

namespace Carbon.Services.FabricHost
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                var activationContext = FabricRuntime.GetActivationContext();
                var configPackage = activationContext.GetConfigurationPackageObject(ServiceFabricConfigurationProvider.DefaultConfigurationPackageName);
                var instrumentationKey = configPackage.Settings.Sections["Azure"].Parameters["TelemetryKey"].Value;
                AppInsightsConfig.Configure(instrumentationKey);

                using (var diagnosticsPipeline = DiagnosticsPipelineFactory.Create(instrumentationKey))
                {
                    // The ServiceManifest.XML file defines one or more service type names.
                    // Registering a service maps a service type name to a .NET type.
                    // When Service Fabric creates an instance of this service type,
                    // an instance of the class is created in this host process.

                    ServiceRuntime.RegisterServiceAsync("Services",
                        context => new ServicesFabricHost(context)).GetAwaiter().GetResult();

                    CommonEventSource.Current.Info("Service initialized", source: nameof(ServicesFabricHost));

                    // Prevents this host process from terminating so services keeps running.
                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                CommonEventSource.Current.Fatal(e, source: nameof(ServicesFabricHost));
                throw;
            }
        }
    }
}
