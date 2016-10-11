using Carbon.Business.Domain;

namespace Carbon.Business.Sync
{    
    public enum PrimitiveType
    {
        None = 0,
        DataNodeAdd = 1,
        DataNodeRemove = 2,
        DataNodeChange = 3,
        DataNodeSetProps = 4,
        DataNodeChangePosition = 5,
        DataNodePatchProps = 6,

        ProjectNameChange = 10,

        Error = 0xFFFFFFF //for testing only
    }

    public enum PatchType
    {
        Insert = 1,
        Remove = 2,
        Change = 3
    }

    public static class PrimitiveTypeExtensions
    {
        public static bool IsDeferred(this PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.DataNodeAdd:
                case PrimitiveType.DataNodeRemove:
                case PrimitiveType.DataNodeChange:
                case PrimitiveType.DataNodeChangePosition:
                case PrimitiveType.DataNodeSetProps:
                case PrimitiveType.DataNodePatchProps:                
                    return true;
                default:
                    return false;
            }
        }

        public static Permission RequiredPermission(this PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.DataNodeAdd:
                case PrimitiveType.DataNodeRemove:
                case PrimitiveType.DataNodeChange:
                case PrimitiveType.DataNodeChangePosition:
                case PrimitiveType.DataNodeSetProps:                
                    return Permission.Write;
                default:
                    return Permission.Owner;
            }
        }
    }
}