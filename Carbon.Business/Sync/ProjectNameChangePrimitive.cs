using Newtonsoft.Json;

namespace Carbon.Business.Sync
{
    public class ProjectNameChangePrimitive : Primitive
    {
        public ProjectNameChangePrimitive() : base(PrimitiveType.ProjectNameChange)
        {            
        }

        public string NewName { get; set; }

        protected override bool ReadProperty(string property, JsonReader reader)
        {
            if (base.ReadProperty(property, reader))
            {
                return true;
            }
            if (property == "newName")
            {
                NewName = ReadString(reader);
                return true;
            }
            return false;
        }

        protected override void WriteProperties(JsonWriter writer)
        {
            base.WriteProperties(writer);

            writer.WritePropertyName("newName");
            writer.WriteValue(NewName);
        }
    }
}