using System;
using System.Threading;
using Microsoft.ServiceFabric.Actors.Runtime;
using Carbon.Fabric.Common;
using Carbon.Fabric.Common.Logging;
using System.Fabric;
using Microsoft.Extensions.Configuration.ServiceFabric;

namespace Carbon.CompanyActor
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

                // This line registers an Actor Service to host your actor class with the Service Fabric runtime.
                // The contents of your ServiceManifest.xml and ApplicationManifest.xml files
                // are automatically populated when you build this project.
                // For more information, see http://aka.ms/servicefabricactorsplatform
                using (var diagnosticsPipeline = DiagnosticsPipelineFactory.Create(instrumentationKey))
                {
                    ActorRuntime.RegisterActorAsync<FabricCompanyActor>(
                        (context, actorType) => new ActorServiceWithBackup(context, actorType, (service, id) => new FabricCompanyActor(service, id))).GetAwaiter().GetResult();

                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                CommonEventSource.Current.Fatal(e.ToString());
                throw;
            }
        }
    }
}
