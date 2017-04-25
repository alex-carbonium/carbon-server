using Carbon.Business;
using Carbon.Business.Services;
using Carbon.Framework.Logging;
using Carbon.Services;
using Carbon.StorageService;
using Microsoft.Owin.Hosting;

namespace Carbon.Console
{
    class ConsoleProgram
    {
        static void Main(string[] args)
        {
            var actorFabric = new InMemoryActorFabric();

            StartServices(actorFabric);
            StartStorage(actorFabric);

            System.Console.WriteLine("Services running, press Enter to exit");
            System.Console.ReadLine();
        }

        private static void StartServices(IActorFabric actorFabric)
        {
            string baseAddress = "http://127.0.0.1:9000/";

            var configuration = new InMemoryConfiguration();
            var start = new ServicesStartup(container =>
            {
                container.RegisterInstance<Configuration>(configuration);
                container.RegisterTypeSingleton<DataProvider, InMemoryDataProvider>();
                container.RegisterTypeSingleton<ILogService, ConsoleLogService>();
                container.RegisterInstance(actorFabric);
            });

            WebApp.Start(baseAddress, start.Configuration);
        }

        private static void StartStorage(IActorFabric actorFabric)
        {
            string baseAddress = "http://127.0.0.1:9100/";

            var configuration = new InMemoryConfiguration();
            var start = new StorageStartup(container =>
            {
                container.RegisterInstance<Configuration>(configuration);
                container.RegisterTypeSingleton<DataProvider, InMemoryDataProvider>();
                container.RegisterTypeSingleton<ILogService, ConsoleLogService>();
                container.RegisterInstance(actorFabric);
            });

            WebApp.Start(baseAddress, start.Configuration);
        }
    }
}
