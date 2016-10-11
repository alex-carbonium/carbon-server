using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;
using Newtonsoft.Json.Linq;
using Carbon.Framework;

namespace Carbon.Owin.Common.WebApi
{
    public class KnownExceptionFilter : ExceptionFilterAttribute
    {
        public Type ExceptionType { get; set; }
        public string Message { get; set; }
        public string RedirectUrl { get; set; }
        public Action<Exception, JObject> AddContent { get; set; }

        public KnownExceptionFilter(Type exceptionType)
        {                  
            ExceptionType = exceptionType;
        }

        public override void OnException(HttpActionExecutedContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            if (actionContext.Response != null && (int)actionContext.Response.StatusCode == Defs.Web.HTTP_AJAX_ERROR_CODE)
            {
                return;
            }

            var exception = actionContext.Exception;

            // If this is not an HTTP 500 (for example, if somebody throws an HTTP 404 from an action method),
            // ignore it.
            if (new HttpException(null, exception).GetHttpCode() != 500)
            {
                return;
            }

            if (ExceptionType != null && !ExceptionType.IsInstanceOfType(exception))
            {
                return;
            }

            var message = !string.IsNullOrEmpty(Message) ? Message : exception.Message;
            actionContext.Response = new HttpResponseMessage((HttpStatusCode)Defs.Web.HTTP_AJAX_ERROR_CODE);            

            var content = new JObject();
            content["errorMessage"] = message;
            content["errorType"] = exception.GetType().Name;
            if (AddContent != null)
            {
                AddContent(exception, content);
            }
            if (!string.IsNullOrEmpty(RedirectUrl))
            {
                actionContext.Response.Headers.Location = new Uri(RedirectUrl, UriKind.RelativeOrAbsolute);
            }
            actionContext.Response.Content = new StringContent(content.ToString());
        }
    }
}