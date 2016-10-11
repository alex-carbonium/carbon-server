using Newtonsoft.Json;

namespace Carbon.Business.Sync
{
    public abstract class Primitive : RawPrimitive
    {        
        public string Id { get; set; }        
        public string SessionId { get; set; }        
        public decimal Time { get; set; }        

        protected Primitive(PrimitiveType type)
        {            
            Type = type;
        }

        protected override bool ReadProperty(string property, JsonReader reader)
        {
            switch (property)
            {
                case "id":
                    Id = ReadString(reader);
                    return true;
                case "sessionId":
                    SessionId = ReadString(reader);
                    return true;
                case "time":
                    Time = ReadDecimal(reader);
                    return true;
                default:
                    return false;
            }
        }        

        protected override void WriteProperties(JsonWriter writer)
        {
            base.WriteProperties(writer);

            writer.WritePropertyName("id");
            writer.WriteValue(Id);

            writer.WritePropertyName("sessionId");
            writer.WriteValue(SessionId);

            writer.WritePropertyName("time");
            writer.WriteValue(Time);
        }
    }
}