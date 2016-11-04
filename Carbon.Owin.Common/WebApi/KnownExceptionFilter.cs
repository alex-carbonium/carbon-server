using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;
using Carbon.Business.Exceptions;
using Newtonsoft.Json.Linq;

namespace Carbon.Owin.Common.WebApi
{
    public class KnownExceptionFilter : ExceptionFilterAttribute
    {
        public Type ExceptionType { get; set; }
        public string Message { get; set; }
        public string RedirectUrl { get; set; }
        public Action<Exception, JObject> AddContent { get; set; }

        public override void OnException(HttpActionExecutedContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }            

            var exception = actionContext.Exception;
            var aggregate = exception as AggregateException;
            if (aggregate?.InnerExceptions.Count == 1)
            {
                exception = aggregate.InnerExceptions[0];
            }            

            // If this is not an HTTP 500 (for example, if somebody throws an HTTP 404 from an action method),
            // ignore it.
            if (new HttpException(null, exception).GetHttpCode() != 500)
            {
                return;
            }

            if (CheckCommonExceptions(actionContext, exception))
            {
                return;
            }

            if (ExceptionType != null && !ExceptionType.IsInstanceOfType(exception))
            {
                return;
            }

            actionContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                ReasonPhrase = exception.Message
            };
#if DEBUG
            actionContext.Response.Content = new StringContent(exception.ToString());
#endif
        }

        private static bool CheckCommonExceptions(HttpActionExecutedContext actionContext, Exception exception)
        {
            if (exception is InsufficientPermissionsException)
            {
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "You do not have permissions for this action"
                };
                return true;
            }

            return false;
        }
    }
}