using Microsoft.Owin;
using Carbon.Framework.Util;

namespace Carbon.Owin.Common.Dependencies
{
    public static class NinjectOwinExtensions
    {
        public static IDependencyContainer GetScopedContainer(this IOwinContext ctx)
        {
            return ctx.Get<IDependencyContainer>("carb_scope");
        }
        public static void SetScopedContainer(this IOwinContext ctx, IDependencyContainer container)
        {
            ctx.Set("carb_scope", container);
        }
    }
}