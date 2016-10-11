using System.Collections.Generic;
using Carbon.Business.Domain.DataTree;
using Newtonsoft.Json;

namespace Carbon.Business.Sync
{
    public class DataNodeSetPropsPrimitive : DataNodeBasePrimitive
    {
        public DataNodeSetPropsPrimitive() : base(PrimitiveType.DataNodeSetProps)
        {
        }
        
        public Dictionary<string, dynamic> Props { get; set; }

        protected override bool ReadProperty(string property, JsonReader reader)
        {
            if (base.ReadProperty(property, reader))
            {
                return true;
            }
            switch (property)
            {
                case "props":                    
                    Props = DataNode.ReadProps(reader);
                    return true;                
                default:
                    return false;
            }
        }

        protected override void WriteProperties(JsonWriter writer)
        {
            base.WriteProperties(writer);

            writer.WritePropertyName("props");
            DataNode.WriteProps(writer, Props);
        }
    }
}