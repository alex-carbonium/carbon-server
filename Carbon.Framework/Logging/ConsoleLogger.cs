using System;
using Carbon.Framework.Util;

namespace Carbon.Framework.Logging
{
    public class ConsoleLogger : ILogger
    {
        public void Trace(string message, IDependencyContainer scope = null, string source = null)
        {
            Write(message);
        }

        public void Error(string message, IDependencyContainer scope = null, string source = null)
        {
            Write(message);
        }

        public void Error(Exception ex, IDependencyContainer scope = null, string source = null)
        {
            Write(ex);
        }

        public void Fatal(string message, IDependencyContainer scope = null, string source = null)
        {
            Write(message);
        }

        public void Fatal(Exception ex, IDependencyContainer scope = null, string source = null)
        {
            Write(ex);
        }

        public void Info(string message, IDependencyContainer scope = null, string source = null)
        {
            Write(message);
        }

        public void Warning(string message, IDependencyContainer scope = null, string source = null)
        {
            Write(message);
        }

        private void Write(string message, params object[] parameters)
        {
            Console.WriteLine(message, parameters);
        }
        private void Write(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}