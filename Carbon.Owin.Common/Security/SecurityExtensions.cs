using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Carbon.Business;
using IdentityServer3.AccessTokenValidation;
using Owin;

namespace Carbon.Owin.Common.Security
{
    public static class SecurityExtensions
    {
        public static string TryGetClaim(this ClaimsIdentity identity, string type)
        {
            var claim = identity.FindFirst(type);
            if (claim != null)
            {
                return claim.Value;
            }
            return null;
        }

        public static IAppBuilder UseAccessToken(this IAppBuilder app, AppSettings appSettings)
        {
            // turn off weird claim mappings for JWTs
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();
            JwtSecurityTokenHandler.OutboundClaimTypeMap = new Dictionary<string, string>();

            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {                
                IssuerName = "https://ppanda",
                Authority = "https://ppanda/resources",
                SigningCertificate = new X509Certificate2(appSettings.ResolvePath(Defs.Packages.Data, appSettings.IdClient.PublicKeyFile)),
                TokenProvider = new MixedOAuthBearerAuthenticationProvider()
            });
            app.Use<UserIdentificationMiddleware>();

            return app;
        }        
    }
}