using Newtonsoft.Json;

namespace Carbon.Business.Sync
{
    public class DataNodeRemovePrimitive : DataNodeBasePrimitive
    {
        public DataNodeRemovePrimitive() : base(PrimitiveType.DataNodeRemove)
        {
        }

        public string ChildId { get; set; }

        protected override bool ReadProperty(string property, JsonReader reader)
        {
            if (base.ReadProperty(property, reader))
            {
                return true;
            }
            switch (property)
            {
                case "childId":
                    ChildId = ReadString(reader);
                    return true;                
                default:
                    return false;
            }
        }

        protected override void WriteProperties(JsonWriter writer)
        {
            base.WriteProperties(writer);

            writer.WritePropertyName("childId");
            writer.WriteValue(ChildId);
        }
    }
}