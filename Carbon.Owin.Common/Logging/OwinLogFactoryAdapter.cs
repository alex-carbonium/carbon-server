using Microsoft.Owin.Logging;

namespace Carbon.Owin.Common.Logging
{
    public class OwinLogFactoryAdapter : ILoggerFactory
    {
        private readonly Framework.Logging.ILogService _logService;

        public OwinLogFactoryAdapter(Framework.Logging.ILogService logService)
        {
            _logService = logService;
        }

        public ILogger Create(string name)
        {
            return new OwinLoggerAdapter(_logService, name);
        }
    }
}