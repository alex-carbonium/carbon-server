using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Carbon.Business.Sync
{
    public class PrimitiveConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var type = PrimitiveType.None;

            reader.Read();
            if (reader.TokenType == JsonToken.PropertyName && string.Equals(reader.Value as string, "type", StringComparison.Ordinal))
            {
                type = (PrimitiveType)reader.ReadAsInt32();
            }

            if (type == PrimitiveType.None)
            {
                reader = ParseSlow(reader, out type);
            }
            if (type == PrimitiveType.None)
            {
                throw new Exception("Could not determine primitive type");
            }

            RawPrimitive primitive = null;
            switch (type)
            {
                case PrimitiveType.DataNodeAdd:
                    primitive = new DataNodeAddPrimitive();
                    break;
                case PrimitiveType.DataNodeRemove:
                    primitive = new DataNodeRemovePrimitive();
                    break;
                case PrimitiveType.DataNodeChange:
                    primitive = new DataNodeChangePrimitive();
                    break;
                case PrimitiveType.DataNodeChangePosition:
                    primitive = new DataNodeChangePositionPrimitive();
                    break;
                case PrimitiveType.DataNodeSetProps:
                    primitive = new DataNodeSetPropsPrimitive();
                    break;
                case PrimitiveType.DataNodePatchProps:
                    primitive = new DataNodePatchPropsPrimitive();
                    break;

                case PrimitiveType.ProjectSettingsChange:
                    primitive = new ProjectSettingsChangePrimitive();
                    break;

                case PrimitiveType.Error:
                    primitive = new ErrorPrimitive();
                    break;
            }
            reader.Read();

            if (primitive != null)
            {
                primitive.ReadProperties(reader);
            }
            else
            {
                primitive = new RawPrimitive();
                primitive.Type = type;
            }
            return primitive;
        }

        private static JsonReader ParseSlow(JsonReader reader, out PrimitiveType type)
        {
            type = PrimitiveType.None;

            var tokens = new List<JProperty>();
            while (reader.TokenType != JsonToken.EndObject)
            {
                tokens.Add(JProperty.Load(reader));
            }

            var obj = new JObject();
            foreach (var token in tokens)
            {
                if (token.Name == "type")
                {
                    type = (PrimitiveType)token.Value.Value<int>();
                }
                obj.Add(token.Name, token.Value);
            }

            reader = obj.CreateReader();
            reader.Read();
            return reader;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(RawPrimitive).IsAssignableFrom(objectType);
        }

        public override bool CanWrite => false;
    }
}