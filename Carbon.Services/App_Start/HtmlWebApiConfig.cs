using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Carbon.Owin.Common.WebApi;

namespace Carbon.Services
{
    public class HtmlWebApiConfig
    {
        public static HttpConfiguration Register()
        {
            var config = new HttpConfiguration();
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            config.Routes.MapHttpRoute(
                "Default",
                "{*url}",
                new { controller = "App", action = "Index", url = RouteParameter.Optional });            

            config.MessageHandlers.Insert(0, new NinjectWebApiHandler());
            config.Services.Add(typeof(IExceptionLogger), new WebApiExceptionLogger());

            return config;
        }
    }
}