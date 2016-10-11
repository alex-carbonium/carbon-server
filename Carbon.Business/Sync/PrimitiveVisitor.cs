using System;
using System.Collections.Generic;
using System.Linq;
using Carbon.Business.Domain.DataTree;

namespace Carbon.Business.Sync
{
    public class PrimitiveVisitor : DataTreeVisitor
    {
        private readonly Dictionary<DataNodePath, List<DataNodeBasePrimitive>> _primitiveMap;

        public PrimitiveVisitor(IReadOnlyList<DataNodeBasePrimitive> primitives)
        {
            _primitiveMap = primitives
                .GroupBy(x => x.Path, x => x)
                .ToDictionary(x => x.Key, x => x.ToList());            
        }

        public override bool Visit(DataNode element, DataNodePath path)
        {            
            List<DataNodeBasePrimitive> primitives;            
            if (_primitiveMap.TryGetValue(path, out primitives))
            {
                foreach (var primitive in primitives)
                {
                    Apply(element, primitive);
                }
            }            
            return true;
        }

        private void Apply(DataNode element, Primitive primitive)
        {
            switch (primitive.Type)
            {
                case PrimitiveType.DataNodeAdd:
                    var add = (DataNodeAddPrimitive) primitive;
                    element.InsertChild(add.Node, add.Index);
                    break;
                case PrimitiveType.DataNodeSetProps:
                    var setProps = (DataNodeSetPropsPrimitive)primitive;
                    element.SetProps(setProps.Props);
                    break;
                case PrimitiveType.DataNodeRemove:
                    var remove = (DataNodeRemovePrimitive)primitive;
                    element.RemoveChild(remove.ChildId);
                    break;
                case PrimitiveType.DataNodeChange:
                    var change = (DataNodeChangePrimitive)primitive;
                    element.ReplaceChild(change.Node);
                    break;
                case PrimitiveType.DataNodeChangePosition:
                    var changePosition = (DataNodeChangePositionPrimitive)primitive;
                    element.ChangeChildPosition(changePosition.ChildId, changePosition.NewPosition);
                    break;
                case PrimitiveType.DataNodePatchProps:
                    var patch = (DataNodePatchPropsPrimitive)primitive;
                    element.PatchProps(patch.PatchType, patch.PropName, patch.Item);                                    
                    break;
                default: 
                    throw new Exception("Unexpected primitive type " + primitive.Type);
            }
        }
    }
}