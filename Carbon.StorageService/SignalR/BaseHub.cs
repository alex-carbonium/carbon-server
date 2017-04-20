using Microsoft.AspNet.SignalR;
using Carbon.Business.Domain;
using Carbon.Framework.Util;
using Carbon.Framework.Logging;

namespace Carbon.StorageService.SignalR
{
    public abstract class BaseHub : Hub
    {
        public IDependencyContainer Scope { get; set; }
        public OperationContext Operation { get; set; }

        protected override void Dispose(bool disposing)
        {
            if (Scope != null)
            {
                Scope.Dispose();
                Scope = null;
            }
            base.Dispose(disposing);
        }
    }
}