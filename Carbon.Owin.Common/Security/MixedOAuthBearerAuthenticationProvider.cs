using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;
using Carbon.Business;

namespace Carbon.Owin.Common.Security
{
    public class MixedOAuthBearerAuthenticationProvider : OAuthBearerAuthenticationProvider
    {
        public override Task RequestToken(OAuthRequestTokenContext context)
        {
            var token = context.Request.Headers.Get("Authorization");
            if (!string.IsNullOrEmpty(token))
            {
                token = token.Substring("Bearer ".Length);
            }
            if (string.IsNullOrEmpty(token))
            {
                token = context.Request.Query.Get(Defs.IdentityServer.AccessTokenKey);
            }
            context.Token = token;
            context.Request.Context.Set(Defs.IdentityServer.AccessTokenKey, token);

            return Task.FromResult<object>(null);
        }

    }
}