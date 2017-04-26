using Microsoft.Diagnostics.EventFlow;
using Microsoft.Diagnostics.EventFlow.ServiceFabric;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.ServiceFabric;
using System;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Carbon.Fabric.Common
{
    public static class DiagnosticsPipelineFactory
    {
        public static IDisposable Create()
        {
            var healthEntityName = "Carbon-Fabric";
            var healthReporter = new ServiceFabricHealthReporter(healthEntityName);

            var activationContext = FabricRuntime.GetActivationContext();
            var configPackage = activationContext.GetConfigurationPackageObject(ServiceFabricConfigurationProvider.DefaultConfigurationPackageName);
            var configFilePath = Path.Combine(configPackage.Path, "eventFlowConfig.json");
            if (!File.Exists(configFilePath))
            {
                string errorMessage = $"{nameof(DiagnosticsPipelineFactory)}: configuration file '{configFilePath}' is missing or inaccessible";
                healthReporter.ReportProblem(errorMessage, EventFlowContextIdentifiers.Configuration);
                throw new Exception(errorMessage);
            }

            ConfigurationBuilder configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile(configFilePath);
            configBuilder.AddServiceFabric(ServiceFabricConfigurationProvider.DefaultConfigurationPackageName);
            var configurationRoot = configBuilder.Build().ApplyFabricConfigurationOverrides(healthReporter);

            var appInsightsOutput = configurationRoot.GetSection("outputs").GetChildren().FirstOrDefault(c => c["type"] == "ApplicationInsights");
            if (appInsightsOutput != null)
            {
                appInsightsOutput["instrumentationKey"] = configPackage.Settings.Sections["Azure"].Parameters["TelemetryKey"].Value;
            }

            return DiagnosticPipelineFactory.CreatePipeline(configurationRoot, new ServiceFabricHealthReporter(healthEntityName));
        }

        internal static IConfigurationRoot ApplyFabricConfigurationOverrides(this IConfigurationRoot configurationRoot, IHealthReporter healthReporter)
        {
            Regex fabricValueReferenceRegex = new Regex(ServiceFabricDiagnosticPipelineFactory.FabricConfigurationValueReference, RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

            // Use ToList() to ensure that configuration is fully enumerated before starting to modify it.
            foreach (var kvp in configurationRoot.AsEnumerable().ToList())
            {
                if (kvp.Value == null)
                {
                    continue;
                }

                try
                {
                    Match valueReferenceMatch = fabricValueReferenceRegex.Match(kvp.Value);
                    if (valueReferenceMatch.Success)
                    {
                        string valueReferencePath = ConfigurationPath.Combine(valueReferenceMatch.Groups["section"].Value, valueReferenceMatch.Groups["name"].Value);
                        string newValue = configurationRoot[valueReferencePath];
                        if (string.IsNullOrEmpty(newValue))
                        {
                            healthReporter.ReportWarning(
                                $"Configuration value reference '{kvp.Value}' was encountered but no corresponding configuration value was found using path '{valueReferencePath}'",
                                EventFlowContextIdentifiers.Configuration);
                        }
                        else
                        {
                            configurationRoot[kvp.Key] = newValue;
                        }
                    }
                }
                catch (RegexMatchTimeoutException)
                {
                    healthReporter.ReportWarning(
                                $"Configuration entry with key '{kvp.Key}' and value '{kvp.Value}' could not be checked if it represents a configuration value reference--a timeout occurred when the value was being parsed.",
                                EventFlowContextIdentifiers.Configuration);
                    continue;
                }
            }

            return configurationRoot;
        }
    }
}
