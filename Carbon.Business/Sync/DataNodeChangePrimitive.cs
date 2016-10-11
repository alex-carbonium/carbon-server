using Carbon.Business.Domain.DataTree;
using Newtonsoft.Json;

namespace Carbon.Business.Sync
{
    public class DataNodeChangePrimitive : DataNodeBasePrimitive
    {
        public DataNodeChangePrimitive() : base(PrimitiveType.DataNodeChange)
        {
        }

        public DataNode Node { get; set; }

        protected override bool ReadProperty(string property, JsonReader reader)
        {
            if (base.ReadProperty(property, reader))
            {
                return true;
            }
            switch (property)
            {
                case "node":
                    Node = DataNode.Create(reader);
                    return true;
                default:
                    return false;
            }
        }

        protected override void WriteProperties(JsonWriter writer)
        {
            base.WriteProperties(writer);

            writer.WritePropertyName("node");
            Node.Write(writer);
        }
    }
}