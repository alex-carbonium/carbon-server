using Newtonsoft.Json;

namespace Carbon.Business.Sync
{
    public class RawPrimitive : JsonObject
    {
        public PrimitiveType Type { get; set; }
        public string SourceString { get; set; }                

        protected override bool ReadProperty(string property, JsonReader reader)
        {
            return false;
        }

        protected override void WriteProperties(JsonWriter writer)
        {
            writer.WritePropertyName("type");
            writer.WriteValue((int)Type);
        }

        public override string Write(Formatting formatting = Formatting.None)
        {
            if (!string.IsNullOrEmpty(SourceString))
            {
                return SourceString;
            }
            return base.Write(formatting);
        }
    }
}