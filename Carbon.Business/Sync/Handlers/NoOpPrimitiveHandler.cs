using Carbon.Business.Domain;
using Carbon.Framework.Util;
using Newtonsoft.Json.Linq;

namespace Carbon.Business.Sync.Handlers
{
    //[PrimitiveHandler("app_online"), PrimitiveHandler("app_offline")]
    public class NoOpPrimitiveHandler : PrimitiveHandler
    {
        public override void Apply(Primitive primitive, ProjectModel projectModel, IDependencyContainer scope)
        {
        }
    }
}
