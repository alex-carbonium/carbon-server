using System;
using System.Collections.Generic;
using Carbon.Business.Domain;
using Carbon.Framework.Logging;
using Carbon.Framework.Util;

namespace Carbon.Business.Logging
{
    public static class ContextLogExtensions
    {
        public static void TraceWithContext(this Logger logger, string message, IDependencyContainer scope = null, Action<IDictionary<string, string>> contextAction = null)
        {
            logger.Trace(message, SetContext(logger, scope, contextAction));
        }
        public static void InfoWithContext(this Logger logger, string message, IDependencyContainer scope = null, Action<IDictionary<string, string>> contextAction = null)
        {
            logger.Info(message, SetContext(logger, scope, contextAction));
        }
        public static void WarningWithContext(this Logger logger, string message, IDependencyContainer scope = null, Action<IDictionary<string, string>> contextAction = null)
        {
            logger.Warning(message, SetContext(logger, scope, contextAction));
        }
        public static void ErrorWithContext(this Logger logger, string message, Action<IDictionary<string, string>> contextAction = null)
        {
            logger.Error(message, SetContext(logger, null, contextAction));
        }
        public static void ErrorWithContext(this Logger logger, Exception ex, IDependencyContainer scope = null, Action<IDictionary<string, string>> contextAction = null)
        {
            logger.Error(ex, SetContext(logger, scope, contextAction));
        }
        public static void ErrorWithContext(this Logger logger, string message, Exception ex, IDependencyContainer scope = null, Action<IDictionary<string, string>> contextAction = null)
        {
            logger.Error(message, ex, SetContext(logger, scope, contextAction));
        }
        public static void FatalWithContext(this Logger logger, string message, Action<IDictionary<string, string>> contextAction = null)
        {
            logger.Fatal(message, SetContext(logger, null, contextAction));
        }
        public static void FatalWithContext(this Logger logger, string message, IDependencyContainer scope = null, Action<IDictionary<string, string>> contextAction = null)
        {
            logger.Fatal(message, SetContext(logger, scope, contextAction));
        }

        public static void EventWithContext(this Logger logger, string message, Action<IDictionary<string, string>> contextAction = null)
        {
            logger.Fatal(message, SetContext(logger, null, contextAction));
        }

        private static Dictionary<string, string> SetContext(Logger logger, IDependencyContainer scope, Action<IDictionary<string, string>> contextAction)
        {
            var context = new Dictionary<string, string>();            
            context["logger"] = logger.Name;

            if (scope != null)
            {
                var identityContext = scope.Resolve<IIdentityContext>();
                if (identityContext != null)
                {
                    context["userId"] = identityContext.GetUserId();
                    context["userEmail"] = identityContext.GetUserEmail();
                    if (!string.IsNullOrEmpty(identityContext.SessionId))
                    {
                        context["sessionId"] = identityContext.SessionId;
                    }
                }                
            }

            if (contextAction != null)
            {
                contextAction(context);
            }
            return context;
        }
    }
}
