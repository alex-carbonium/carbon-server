using System.Threading.Tasks;
using Microsoft.Owin;
using Carbon.Framework.Util;

namespace Carbon.Owin.Common.Dependencies
{
    public class NinjectMiddleware : OwinMiddleware
    {
        private readonly IDependencyContainer _mainContainer;

        public NinjectMiddleware(OwinMiddleware next, IDependencyContainer mainContainer) : base(next)
        {
            _mainContainer = mainContainer;
        }

        public override async Task Invoke(IOwinContext context)
        {
            using (var scope = _mainContainer.BeginScope())
            {
                context.SetScopedContainer(scope);
                await Next.Invoke(context);
            }
        }
    }
}