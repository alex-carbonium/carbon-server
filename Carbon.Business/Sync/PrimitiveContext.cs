using Carbon.Framework.Util;
using Carbon.Framework.Logging;

namespace Carbon.Business.Sync
{
    public class PrimitiveContext
    {
        public string UserId { get; set; }
        public IDependencyContainer Scope { get; set; }

        public static PrimitiveContext Create(IDependencyContainer scope)
        {
            var context = new PrimitiveContext();
            context.UserId = scope.Resolve<OperationContext>().UserId;
            context.Scope = scope;
            return context;
        }
    }
}