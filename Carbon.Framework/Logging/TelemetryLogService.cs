namespace Carbon.Framework.Logging
{    
    public class TelemetryLogService : ILogService
    {
        public Logger GetLogger(object owner)
        {            
            return new TelemetryLogger(owner.GetType().FullName);
        }

        public Logger GetLogger(string name)
        {
            return new TelemetryLogger(name);
        }

        public void SetGlobalContextProperty(string name, string value)
        {
        }
    }
}