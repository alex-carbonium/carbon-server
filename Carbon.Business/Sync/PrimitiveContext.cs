using System.Collections.Generic;
using Carbon.Business.Domain;
using Carbon.Framework.UnitOfWork;
using Carbon.Framework.Util;
using Carbon.Framework.Logging;

namespace Carbon.Business.Sync
{
    public class PrimitiveContext
    {
        public IUnitOfWork UnitOfWork { get; set; }
        public string UserId { get; set; }
        public IDependencyContainer Scope { get; set; }

        public static PrimitiveContext Create(IDependencyContainer scope)
        {
            var context = new PrimitiveContext();
            context.UnitOfWork = scope.Resolve<IUnitOfWork>();
            context.UserId = scope.Resolve<OperationContext>().UserId;
            context.Scope = scope;
            return context;
        }
    }
}