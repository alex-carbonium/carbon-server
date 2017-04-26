using Carbon.Framework.Util;
using System;

namespace Carbon.Framework.Logging
{
    public interface ILogger
    {
        void Trace(string message, IDependencyContainer scope = null, string source = null);

        void Info(string message, IDependencyContainer scope = null, string source = null);

        void Warning(string message, IDependencyContainer scope = null, string source = null);

        void Error(string message, IDependencyContainer scope = null, string source = null);
        void Error(Exception ex, IDependencyContainer scope = null, string source = null);

        void Fatal(string message, IDependencyContainer scope = null, string source = null);
        void Fatal(Exception ex, IDependencyContainer scope = null, string source = null);
    }
}
