using System.Net.Http;
using System.Web.Http.ExceptionHandling;
using Carbon.Business.Logging;
using Carbon.Framework.Logging;
using Carbon.Owin.Common.Dependencies;

namespace Carbon.Owin.Common.WebApi
{
    public class WebApiExceptionLogger : ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {
            var scope = context.Request.GetOwinContext().GetScopedContainer();
            var logService = scope.Resolve<ILogService>();
            logService.GetLogger(this).ErrorWithContext(context.Exception, scope);
        }
    }
}