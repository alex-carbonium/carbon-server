using Newtonsoft.Json;

namespace Carbon.Business.Sync
{
    public class DataNodePatchPropsPrimitive : DataNodeBasePrimitive
    {
        public DataNodePatchPropsPrimitive() : base(PrimitiveType.DataNodePatchProps)
        {
        }

        public string PropName { get; set; }        
        public PatchType PatchType { get; set; }        
        public dynamic Item { get; set; }        

        protected override bool ReadProperty(string property, JsonReader reader)
        {
            if (base.ReadProperty(property, reader))
            {
                return true;
            }
            switch (property)
            {
                case "propName":
                    PropName = ReadString(reader);
                    return true;
                case "patchType":
                    PatchType = (PatchType) ReadInt(reader);
                    return true;
                case "item":
                    reader.Read();
                    Item = Serializer.Deserialize<dynamic>(reader);
                    reader.Read();
                    return true;                
                default:
                    return false;
            }
        }

        protected override void WriteProperties(JsonWriter writer)
        {
            base.WriteProperties(writer);

            writer.WritePropertyName("item");
            writer.WriteValue(Item);

            writer.WritePropertyName("propName");
            writer.WriteValue(PropName);

            writer.WritePropertyName("patchType");
            writer.WriteValue(PatchType);
        }
    }
}