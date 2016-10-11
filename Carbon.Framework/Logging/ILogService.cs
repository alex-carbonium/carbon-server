namespace Carbon.Framework.Logging
{
    public static class Loggers
    {
        public const string PAYMENTS = "PaymentsLogger";
        public const string PERFORMANCE = "PerformanceLogger";
        public const string IMPORTANT = "ImportantLogger";
        public const string MAILING = "Mailing";        
    }

    public interface ILogService
    {
        Logger GetLogger(object owner);
        Logger GetLogger(string name);

        void SetGlobalContextProperty(string name, string value);
    }
}