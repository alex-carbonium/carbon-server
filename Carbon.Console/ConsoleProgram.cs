using Carbon.Business;
using Carbon.Business.Services;
using Carbon.Fabric.Common.Logging;
using Carbon.Framework.Logging;
using Carbon.Services;
using Carbon.StorageService;
using Microsoft.Owin.Hosting;
using System.Linq;

namespace Carbon.Console
{
    class ConsoleProgram
    {
        static void Main(string[] args)
        {
            var configuration = new InMemoryConfiguration();
            AppInsightsConfig.Configure(configuration.GetString("Azure", "TelemetryKey"));

            var actorFabric = new InMemoryActorFabric();
            var protocol = args.Contains("--ssl") ? "https" : "http";

            StartServices(protocol, actorFabric, configuration);
            StartStorage(protocol, actorFabric, configuration);

            System.Console.WriteLine("Press Enter to exit...");
            System.Console.ReadLine();
        }

        private static void StartServices(string protocol, IActorFabric actorFabric, Configuration configuration)
        {
            string baseAddress = protocol + "://+:9000/";

            var start = new ServicesStartup(container =>
            {
                container.RegisterInstance<Configuration>(configuration);
                container.RegisterTypeSingleton<DataProvider, InMemoryDataProvider>();
                container.RegisterTypeSingleton<ILogService, ConsoleLogService>();
                container.RegisterInstance(actorFabric);
            });

            WebApp.Start(baseAddress, start.Configuration);
            System.Console.WriteLine("Running services on " + baseAddress);
        }

        private static void StartStorage(string protocol, IActorFabric actorFabric, Configuration configuration)
        {
            string baseAddress = protocol + "://+:9100/";

            var start = new StorageStartup(container =>
            {
                container.RegisterInstance<Configuration>(configuration);
                container.RegisterTypeSingleton<DataProvider, InMemoryDataProvider>();
                container.RegisterTypeSingleton<ILogService, ConsoleLogService>();
                container.RegisterInstance(actorFabric);
            });

            WebApp.Start(baseAddress, start.Configuration);
            System.Console.WriteLine("Running storage on " + baseAddress);
        }
    }
}
