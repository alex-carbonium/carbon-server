using System;
using System.Linq;
using System.Threading.Tasks;
using Carbon.Owin.Common.Security;
using IdentityServer3.Core.Services;

namespace Carbon.Services.IdentityServer
{
    public class CorsPolicyService : ICorsPolicyService
    {
        public Task<bool> IsOriginAllowedAsync(string origin)
        {
            if (origin.EndsWith("/")) //just in case
            {
                origin = origin.Substring(0, origin.Length - 1);
            }
            return Task.FromResult(AllowedOrigins.All.Contains(origin, StringComparer.InvariantCultureIgnoreCase));
        }
    }
}