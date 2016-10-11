using System;
using System.Collections.Generic;
using Carbon.Business.Domain.DataTree;
using Carbon.Business.Sync;

namespace Carbon.Test.Unit.Sync
{
    public class PrimitiveStubs
    {
        public static Primitive AppPropertyChanged(string name, string value)
        {
            var primitive = new DataNodeSetPropsPrimitive();
            primitive.Id = Guid.NewGuid().ToString();
            primitive.Path = new DataNodePath();
            primitive.Props = new Dictionary<string, dynamic> { {name, value} };
            return primitive;
        }
        
        public static Primitive ElementNew(string pageId)
        {
            var primitive = new DataNodeAddPrimitive();
            primitive.Id = Guid.NewGuid().ToString();
            primitive.Path = new DataNodePath(pageId, pageId);
            primitive.Node = new DataNode(Guid.NewGuid().ToString(), NodeType.Text);
            return primitive;
        }

        public static DataNodeAddPrimitive PageAdd(string pageId)
        {
            var primitive = new DataNodeAddPrimitive();
            primitive.Id = Guid.NewGuid().ToString();
            primitive.Path = new DataNodePath();
            primitive.Node = new DataNode(pageId, NodeType.Page);
            return primitive;
        }

        public static Primitive PagePropertyChange(string pageId, string property, string value)
        {
            var primitive = new DataNodeSetPropsPrimitive();
            primitive.Id = Guid.NewGuid().ToString();
            primitive.Path = new DataNodePath(pageId, pageId);
            primitive.Props = new Dictionary<string, dynamic>() { {property, value} };
            return primitive;            
        }

        public static Primitive PageRemove(string pageId)
        {
            var primitive = new DataNodeRemovePrimitive();
            primitive.Id = Guid.NewGuid().ToString();
            primitive.Path = new DataNodePath();
            primitive.ChildId = pageId;
            return primitive;
        }
        
        public static Primitive ThrowError()
        {            
            return new ErrorPrimitive();
        }
    }
}