namespace Carbon.Framework.Logging
{
    public static class LogExtensions
    {
        public static void WritePerf(this Logger logger, string area, string action, string millisecondsTaken, string parameters = null)
        {
            logger.Trace(string.Format("{0},{1},{2},{3}", area, action, millisecondsTaken, parameters));
        }
    }
}