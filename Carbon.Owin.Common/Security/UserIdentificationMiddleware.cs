using System.Threading.Tasks;
using Microsoft.Owin;
using Carbon.Owin.Common.Dependencies;
using Carbon.Framework.Logging;

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
            var operation = context.GetScopedContainer().Resolve<OperationContext>();
            operation.SetUserId(context.Request.User);
            operation.SessionId = context.Request.Headers["X-SessionId"] ?? context.Request.Query["sessionId"];
            await Next.Invoke(context);
        }
    }
}