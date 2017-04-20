namespace Carbon.Framework.Logging
{
    public class ConsoleLogService : ILogService
    {
        public ILogger GetLogger()
        {
            return new ConsoleLogger();
        }
    }
}