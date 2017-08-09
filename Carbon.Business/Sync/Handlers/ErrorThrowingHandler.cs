using System;
using Carbon.Business.Domain;
using Carbon.Framework.Util;

namespace Carbon.Business.Sync.Handlers
{
    //used for testing only
    [PrimitiveHandler(PrimitiveType.Error)]
    public class ErrorThrowingHandler : PrimitiveHandler
    {
        public override void Apply(Primitive primitive, ProjectModel projectModel, IDependencyContainer scope)
        {
            throw new NotImplementedException();
        }
    }
}