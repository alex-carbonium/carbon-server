using Carbon.Business.Domain.DataTree;
using Newtonsoft.Json;

namespace Carbon.Business.Sync
{
    public class DataNodeAddPrimitive : DataNodeBasePrimitive
    {
        public DataNodeAddPrimitive() : base(PrimitiveType.DataNodeAdd)
        {
        }

        public int Index { get; set; }
        public DataNode Node { get; set; }

        protected override bool ReadProperty(string property, JsonReader reader)
        {
            if (base.ReadProperty(property, reader))
            {
                return true;
            }
            switch (property)
            {
                case "index":
                    Index = ReadInt(reader);
                    return true;
                case "node":
                    ReadAssert(reader, JsonToken.PropertyName);
                    Node = DataNode.Create(reader);
                    return true;
                default:
                    return false;
            }
        }

        protected override void WriteProperties(JsonWriter writer)
        {
            base.WriteProperties(writer);

            writer.WritePropertyName("index");
            writer.WriteValue(Index);

            writer.WritePropertyName("node");
            Node.Write(writer);
        }
    }
}