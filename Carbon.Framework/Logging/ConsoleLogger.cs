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
            Console.ForegroundColor = ConsoleColor.Red;
            Write(message);
            Console.ResetColor();
        }

        public void Error(Exception ex, IDependencyContainer scope = null, string source = null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Write(ex);
            Console.ResetColor();
        }

        public void Fatal(string message, IDependencyContainer scope = null, string source = null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Write(message);
            Console.ResetColor();
        }

        public void Fatal(Exception ex, IDependencyContainer scope = null, string source = null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Write(ex);
            Console.ResetColor();
        }

        public void Info(string message, IDependencyContainer scope = null, string source = null)
        {
            Write(message);
        }

        public void Warning(string message, IDependencyContainer scope = null, string source = null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Write(message);
            Console.ResetColor();
        }

        private void Write(string message, params object[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.WriteLine(message);
            }
        }
        private void Write(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}