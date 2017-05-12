using System;
using System.Diagnostics;
using Microsoft.Owin.Logging;

namespace Carbon.Owin.Common.Logging
{
    public class OwinLoggerAdapter : ILogger
    {
        private readonly Framework.Logging.ILogService _logService;
        private readonly string _name;

        public OwinLoggerAdapter(Framework.Logging.ILogService logService, string name)
        {
            _logService = logService;
            _name = name;
        }

        public bool WriteCore(TraceEventType eventType, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            var logger = _logService.GetLogger();
            var message = formatter(state, exception);
            switch (eventType)
            {
                case TraceEventType.Verbose:
                    logger.Trace(message);
                    break;
                case TraceEventType.Information:
                    logger.Info(message);
                    break;
                case TraceEventType.Warning:
                    logger.Warning(message);
                    break;
                case TraceEventType.Error:
                    //standard transport exception that often happens in SignalR
                    if (message != "The specified network name is no longer available")
                    {
                        logger.Error(message);
                    }
                    break;
                case TraceEventType.Critical:
                    logger.Fatal(message);
                    break;
            }
            return true;
        }
    }
}