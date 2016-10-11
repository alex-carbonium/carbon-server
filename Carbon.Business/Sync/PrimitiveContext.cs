using System.Collections.Generic;
using Carbon.Business.Domain;
using Carbon.Framework.UnitOfWork;
using Carbon.Framework.Util;

namespace Carbon.Business.Sync
{
    public class PrimitiveContext
    {
        public IUnitOfWork UnitOfWork { get; set; }
        public User User { get; set; }
        public IDependencyContainer Scope { get; set; }                

        public static PrimitiveContext Create(IDependencyContainer scope)
        {
            var context = new PrimitiveContext();
            context.UnitOfWork = scope.Resolve<IUnitOfWork>();
            //context.UserId = scope.Resolve<IIdentityContext>().GetLoggedInUser();
            context.Scope = scope;
            return context;
        }
    }
}