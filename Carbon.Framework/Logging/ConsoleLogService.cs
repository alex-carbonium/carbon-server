namespace Carbon.Framework.Logging
{
    public class ConsoleLogService : ILogService
    {
        public Logger GetLogger(object owner)
        {
            return new ConsoleLogger();
        }

        public Logger GetLogger(string name)
        {
            return new ConsoleLogger();
        }

        public void SetGlobalContextProperty(string name, string value)
        {            
        }
    }
}