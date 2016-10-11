using System;
using System.Collections.Generic;

namespace Carbon.Framework.Logging
{
    public class ConsoleLogger : Logger
    {
        private void Write(string message, params object[] parameters)
        {
            Console.WriteLine(message, parameters);       
        }
        private void Write(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        public override void Debug(string message, params object[] parameters)
        {            
            Write(message, parameters);
        }

        public override void Info(string message, params object[] parameters)
        {
            Write(message, parameters);
        }

        public override void Info(string message, IDictionary<string, string> context)
        {
            Write(message);
        }

        public override void Warning(string message, params object[] parameters)
        {
            Write(message, parameters);
        }

        public override void Warning(string message, IDictionary<string, string> context)
        {
            Write(message);
        }

        public override void Error(string message, params object[] parameters)
        {
            Write(message, parameters);
        }

        public override void Error(string message, IDictionary<string, string> context)
        {
            Write(message);
        }

        public override void Error(string message, Exception ex, IDictionary<string, string> context)
        {
            Write(message + Environment.NewLine + ex);
        }

        public override void Error(Exception ex, IDictionary<string, string> context)
        {
            Write(ex);
        }

        public override void Fatal(Exception ex)
        {
            Write(ex);
        }

        public override void Fatal(string message)
        {
            Write(message);
        }

        public override void Fatal(string message, params object[] parameters)
        {
            Write(message, parameters);
        }

        public override void Fatal(string message, IDictionary<string, string> context)
        {
            Write(message);
        }

        public override void Trace(string message)
        {
            Write(message);
        }

        public override void Trace(string message, params object[] parameters)
        {
            Write(message, parameters);
        }

        public override void Trace(string message, IDictionary<string, string> context, params object[] parameters)
        {
            Write(message, parameters);
        }
    }
}