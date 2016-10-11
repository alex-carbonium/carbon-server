using System;
using Carbon.Framework.Logging;
using IdentityServer3.Core.Logging;
using Logger = IdentityServer3.Core.Logging.Logger;

namespace Carbon.Services.IdentityServer
{
    public class LogProviderAdapter : ILogProvider
    {        
        private readonly Framework.Logging.Logger _logger;

        public LogProviderAdapter(ILogService logService)
        {
            _logger = logService.GetLogger("IdentityService");
        }

        public IDisposable OpenNestedContext(string message)
        {
            return null;
        }

        public IDisposable OpenMappedContext(string key, string value)
        {
            return null;
        }

        Logger ILogProvider.GetLogger(string name)
        {
            return Log;
        }

        private bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
        {
            if (exception != null)
            {
                _logger.Error(exception);
                return true;
            }

            if (messageFunc == null)
            {
                return true;
            }
                        
            switch (logLevel)
            {
                //other log levels produce too much unnecessary data                
                case LogLevel.Warn:
                    _logger.Warning(messageFunc(), formatParameters);
                    break;
                case LogLevel.Error:
                    _logger.Error(messageFunc(), formatParameters);
                    break;
                case LogLevel.Fatal:
                    _logger.Fatal(messageFunc(), formatParameters);
                    break;
            }

            return true;
        }
    }
}