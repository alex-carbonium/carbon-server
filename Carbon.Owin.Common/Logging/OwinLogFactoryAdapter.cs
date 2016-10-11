using Microsoft.Owin.Logging;
using Carbon.Framework.Logging;

namespace Carbon.Owin.Common.Logging
{
    public class OwinLogFactoryAdapter : ILoggerFactory
    {
        private readonly ILogService _logService;

        public OwinLogFactoryAdapter(ILogService logService)
        {
            _logService = logService;
        }

        public ILogger Create(string name)
        {
            return new OwinLoggerAdapter(_logService, name);
        }
    }
}