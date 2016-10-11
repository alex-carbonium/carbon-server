using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using Carbon.Business.Exceptions;

namespace Carbon.Services.IdentityServer
{
    public class WebApiExceptionHandler : ExceptionHandler
    {
        public override void Handle(ExceptionHandlerContext context)
        {
            if (context.Exception.GetType() == typeof (DeletedUserException))
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                var cookie = new CookieHeaderValue(
                    IdentityServer3.Core.Constants.PrimaryAuthenticationType, string.Empty)
                {
                    Expires = DateTime.Now.AddYears(-1),
                    HttpOnly = true,
                    Path = "/"
                };
                response.Headers.AddCookies(new[] {cookie});
                //response.Content =
                //    new StringContent(
                //        new StreamReader(ViewService.RenderView(MVC.Shared.Views.LogoutRedirect)).ReadToEnd(),
                //        Encoding.UTF8, "text/html");
                context.Result = new ResponseMessageResult(response);
            }
            else
            {
                context.Result = new RedirectResult(new Uri("/error/" + Uri.EscapeUriString("We could not sign you in, please try again."), UriKind.Relative), context.Request);
            }
        }

        public override bool ShouldHandle(ExceptionHandlerContext context)
        {
            return context.Exception.GetType() == typeof (DeletedUserException);
        }
    }
}