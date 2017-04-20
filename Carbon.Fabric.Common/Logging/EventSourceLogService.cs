using Carbon.Framework.Logging;

namespace Carbon.Fabric.Common.Logging
{
    public class EventSourceLogService : ILogService
    {
        public ILogger GetLogger()
        {
            return CommonEventSource.Current;
        }
    }
}
