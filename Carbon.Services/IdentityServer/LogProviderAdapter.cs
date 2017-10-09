using System;
using Carbon.Framework.Logging;
using IdentityServer3.Core.Logging;
using Logger = IdentityServer3.Core.Logging.Logger;
using System.IdentityModel.Tokens;

namespace Carbon.Services.IdentityServer
{
    public class LogProviderAdapter : ILogProvider
    {
        private readonly Framework.Logging.ILogger _logger;

        public LogProviderAdapter(ILogService logService)
        {
            _logger = logService.GetLogger();
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
                if (exception is SecurityTokenExpiredException)
                {
                    _logger.Info("Security token expired. " + exception.Message);
                    return true;
                }
                _logger.Error(exception);
                return true;
            }

            if (messageFunc == null)
            {
                return true;
            }

            switch (logLevel)
            {
                //case LogLevel.Info:
                //    _logger.Info(Format(messageFunc, formatParameters));
                //    break;
                case LogLevel.Warn:
                    _logger.Warning(Format(messageFunc, formatParameters));
                    break;
                case LogLevel.Error:
                    _logger.Error(Format(messageFunc, formatParameters));
                    break;
                case LogLevel.Fatal:
                    _logger.Fatal(Format(messageFunc, formatParameters));
                    break;
            }

            return true;
        }

        private static string Format(Func<string> messageFunc, object[] formatParameters)
        {
            if (formatParameters != null && formatParameters.Length > 0)
            {
                return string.Format(messageFunc(), formatParameters);
            }
            return messageFunc();
        }
    }
}