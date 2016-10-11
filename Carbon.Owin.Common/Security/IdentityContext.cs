using System.Security.Claims;
using System.Security.Principal;
using Carbon.Business.Domain;
using Carbon.Framework;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;

namespace Carbon.Owin.Common.Security
{
    public class IdentityContext : IIdentityContext
    {
        public IPrincipal Principal { get; set; }
        public string SessionId { get; set; }

        public virtual string GetUserId()
        {
            var claimsIdentity = Principal?.Identity as ClaimsIdentity;
            if (claimsIdentity != null && claimsIdentity.IsAuthenticated)
            {
                return claimsIdentity.FindFirst(Defs.ClaimTypes.Subject).Value;
            }

            return null;
        }

        public string GetUserEmail()
        {
            var claimsIdentity = Principal?.Identity as ClaimsIdentity;
            if (claimsIdentity != null && claimsIdentity.IsAuthenticated)
            {
                return claimsIdentity.FindFirst(ClaimTypes.Email)?.Value;
            }

            return null;
        }                        
    }
}