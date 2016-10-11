using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace Carbon.Framework.Logging
{
    public abstract class Logger
    {
        [Conditional(Defs.DEBUG)]
        public abstract void Debug(string message, params object[] parameters);
        public abstract void Info(string message, params object[] parameters);
        public abstract void Info(string message, IDictionary<string, string> context);
        public abstract void Warning(string message, params object[] parameters);
        public abstract void Warning(string message, IDictionary<string, string> context);

        public abstract void Error(string message, params object[] parameters);        
        public abstract void Error(string message, IDictionary<string, string> context);
        public abstract void Error(string message, Exception ex, IDictionary<string, string> context);
        public abstract void Error(Exception ex, IDictionary<string, string> context = null);
        
        public abstract void Fatal(Exception ex);
        public abstract void Fatal(string message);
        public abstract void Fatal(string message, params object[] parameters);
        public abstract void Fatal(string message, IDictionary<string, string> context);

        public void DebugWarningOrReleaseError(string message, params object[] parameters)
        {
#if DEBUG
            Warning(message, parameters);
#else   
            Error(message, parameters);
#endif
        }

        [Conditional(Defs.TRACE)]
        public abstract void Trace(string message);
        [Conditional(Defs.TRACE)]
        public abstract void Trace(string message, params object[] parameters);
        [Conditional(Defs.TRACE)]
        public abstract void Trace(string message, IDictionary<string, string> context, params object[] parameters);

        public virtual void Event(string name, IDictionary<string, string> properties)
        {
        }        

        public virtual string Name 
        {
            get { return null; }
        }
    }
}
