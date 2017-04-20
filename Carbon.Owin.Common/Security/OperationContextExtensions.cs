using System.Security.Claims;
using System.Security.Principal;
using Carbon.Framework;
using Carbon.Framework.Logging;

namespace Carbon.Owin.Common.Security
{
    public static class OperationContextExtensions
    {
        public static void SetUserId(this OperationContext operation, IPrincipal principal)
        {
            var claimsIdentity = principal?.Identity as ClaimsIdentity;
            if (claimsIdentity != null && claimsIdentity.IsAuthenticated)
            {
                operation.UserId = claimsIdentity.FindFirst(Defs.ClaimTypes.Subject).Value;
                //first assuming that user performs operation in his own company
                operation.CompanyId = operation.UserId;
            }
        }
    }
}