using Newtonsoft.Json;

namespace Carbon.Business.Sync
{
    public class DataNodeChangePositionPrimitive : DataNodeBasePrimitive
    {
        public DataNodeChangePositionPrimitive() : base(PrimitiveType.DataNodeChangePosition)
        {
        }

        public string ChildId { get; set; }
        public int NewPosition { get; set; }

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
                case "newPosition":
                    NewPosition = ReadInt(reader);
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

            writer.WritePropertyName("newPosition");
            writer.WriteValue(NewPosition);
        }
    }
}