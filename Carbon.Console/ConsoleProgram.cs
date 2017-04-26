using Carbon.Business;
using Carbon.Business.Services;
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
            var actorFabric = new InMemoryActorFabric();
            var protocol = args.Contains("--ssl") ? "https" : "http";

            StartServices(protocol, actorFabric);
            StartStorage(protocol, actorFabric);

            System.Console.WriteLine("Press Enter to exit...");
            System.Console.ReadLine();
        }

        private static void StartServices(string protocol, IActorFabric actorFabric)
        {
            string baseAddress = protocol + "://+:9000/";

            var configuration = new InMemoryConfiguration();
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

        private static void StartStorage(string protocol, IActorFabric actorFabric)
        {
            string baseAddress = protocol + "://+:9100/";

            var configuration = new InMemoryConfiguration();
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
