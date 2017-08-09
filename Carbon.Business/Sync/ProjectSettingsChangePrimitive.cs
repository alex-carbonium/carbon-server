using Carbon.Business.Domain.DataTree;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Carbon.Business.Sync
{
    public class ProjectSettingsChangePrimitive : Primitive
    {
        public ProjectSettingsChangePrimitive() : base(PrimitiveType.ProjectSettingsChange)
        {
        }

        public Dictionary<string, dynamic> Settings { get; set; }

        public string Name => Settings["name"];
        public string Avatar => Settings["avatar"];

        protected override bool ReadProperty(string property, JsonReader reader)
        {
            if (base.ReadProperty(property, reader))
            {
                return true;
            }
            if (property == "settings")
            {
                Settings = DataNode.ReadProps(reader);
                return true;
            }
            return false;
        }

        protected override void WriteProperties(JsonWriter writer)
        {
            base.WriteProperties(writer);

            writer.WritePropertyName("newName");
            DataNode.WriteProps(writer, Settings);
        }
    }
}