using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.ExceptionHandling;
using Carbon.Owin.Common.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Carbon.Owin.Common.WebApi
{
    public static class CommonWebApiConfig
    {
        public static HttpConfiguration Register(Assembly routesAssembly = null, string basePath = null, bool useInjection = true, bool useAttributeRoutes = true)
        {
            var config = new HttpConfiguration();

            config.EnableCors(new EnableCorsAttribute(
                AllowedOrigins.AllAsList,
                "*",//"Content-Type,X-Requested-With,Authorization,X-SessionId,Cache-Control,Origin",
                "POST,GET,OPTIONS"));

            // Web API configuration and services
            config.Formatters.Remove(config.Formatters.XmlFormatter);
#if DEBUG
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
#endif
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;

            if (useAttributeRoutes)
            {
                config.MapHttpAttributeRoutes();
            }

            if (useInjection)
            {
                //config.Filters.Add(new WebApiUnitOfWorkFilterAttribute());

                config.MessageHandlers.Insert(0, new NinjectWebApiHandler());
            }

            if (routesAssembly != null && basePath != null)
            {
                config.RegisterSwagger(routesAssembly, basePath);
            }

            config.Services.Add(typeof(IExceptionLogger), new WebApiExceptionLogger());

            config.Filters.Add(new KnownExceptionFilter());

            return config;
        }
    }
}
