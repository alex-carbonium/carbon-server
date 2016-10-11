using System.Web.Http;
using Carbon.Owin.Common.WebApi;

namespace Carbon.Services
{
    public class IdentityWebApiConfig
    {
        public static HttpConfiguration Register()
        {
            var config = CommonWebApiConfig.Register(useAttributeRoutes: false);

            config.Routes.MapHttpRoute(
                "UserId",
                "userId",
                new { controller = "IdentityServerExtensions", action = "UserId" });

            config.Routes.MapHttpRoute(
                "Logout",
                "logout",
                new { controller = "IdentityServerExtensions", action = "Logout" });

            return config;
        }
    }
}