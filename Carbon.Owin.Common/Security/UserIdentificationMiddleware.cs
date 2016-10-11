using System.Threading.Tasks;
using Microsoft.Owin;
using Carbon.Business.Domain;
using Carbon.Owin.Common.Dependencies;

namespace Carbon.Owin.Common.Security
{
    public class UserIdentificationMiddleware : OwinMiddleware
    {
        public UserIdentificationMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            var identityContext = context.GetScopedContainer().Resolve<IIdentityContext>();
            identityContext.Principal = context.Request.User;            
            identityContext.SessionId = context.Request.Headers["X-SessionId"] ?? context.Request.Query["sessionId"];
            await Next.Invoke(context);            
        }
    }
}