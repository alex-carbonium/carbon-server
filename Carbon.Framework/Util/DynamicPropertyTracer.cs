using System.Dynamic;

namespace Carbon.Framework.Util
{
    public class DynamicPropertyTracer : DynamicObject
    {        
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = binder.Name;
            return true;
        }
    }
}
