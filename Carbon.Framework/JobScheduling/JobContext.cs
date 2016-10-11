using Carbon.Framework.Logging;
using Carbon.Framework.UnitOfWork;
using Carbon.Framework.Util;

namespace Carbon.Framework.JobScheduling
{                    
    public class JobContext
    {
        public IUnitOfWork UnitOfWork { get; private set; }
        public Logger Logger { get; private set; }
        public IDependencyContainer Scope { get; private set; }

        public JobContext(IDependencyContainer scope, IUnitOfWork uow, Logger logger)
        {
            UnitOfWork = uow;
            Logger = logger;
            Scope = scope;
        }
    }
}