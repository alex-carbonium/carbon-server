using System;
using Carbon.Business.Domain;

namespace Carbon.Business.Sync.Handlers
{
    //used for testing only
    [PrimitiveHandler(PrimitiveType.Error)] 
    public class ErrorThrowingHandler : PrimitiveHandler
    {        
        public override void Apply(Primitive primitive, ProjectModel projectModel, PrimitiveContext context)
        {
            throw new NotImplementedException();
        }
    }
}