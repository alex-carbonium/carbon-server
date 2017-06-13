using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Carbon.Business
{
    public abstract class JsonObject
    {
        static JsonObject()
        {
            Serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            });
        }

        public virtual void Read(JsonReader reader)
        {
            if (reader.TokenType == JsonToken.None)
            {
                reader.Read();
            }

            ReadAssert(reader, JsonToken.StartObject);
            ReadProperties(reader);
            ReadAssert(reader, JsonToken.EndObject);
        }

        public void ReadProperties(JsonReader reader)
        {
            while (reader.TokenType == JsonToken.PropertyName)
            {
                var property = (string)reader.Value;
                if (!ReadProperty(property, reader))
                {
                    reader.Read();
                    if (reader.TokenType == JsonToken.StartObject || reader.TokenType == JsonToken.StartArray)
                    {
                        reader.Skip();
                        reader.Read();
                    }
                    else
                    {
                        reader.Read();
                    }
                }
            }
        }

        public virtual void Read(string json)
        {
            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                Read(reader);
            }
        }

        public virtual void Read(Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            using (var reader = new JsonTextReader(streamReader))
            {
                Read(reader);
            }
        }

        protected string ReadString(JsonReader reader)
        {
            var value = reader.ReadAsString();
            reader.Read();
            return value;
        }

        protected int ReadInt(JsonReader reader)
        {
            var value = reader.ReadAsInt32().Value;
            reader.Read();
            return value;
        }

        protected decimal ReadDecimal(JsonReader reader)
        {
            var value = reader.ReadAsDecimal().Value;
            reader.Read();
            return value;
        }

        protected dynamic ReadDynamic(JsonReader reader)
        {
            var value = reader.Read();
            reader.Read();
            return value;
        }

        protected void ReadAssert(JsonReader reader, JsonToken tokenType)
        {
            Debug.Assert(reader.TokenType == tokenType);
            reader.Read();
        }

        public static JsonSerializer Serializer { get; private set; }

        public virtual string Write(Formatting formatting = Formatting.None)
        {
            var builder = new StringBuilder(256);
            using (var sw = new StringWriter(builder, CultureInfo.InvariantCulture))
            using (var writer = new JsonTextWriter(sw))
            {
                writer.Formatting = formatting;

                Write(writer);
            }
            return builder.ToString();
        }

        public virtual void Write(JsonWriter writer)
        {
            writer.WriteStartObject();
            WriteProperties(writer);
            writer.WriteEndObject();
        }

        public Stream ToStream()
        {
            var stream = new MemoryStream();
            using (var streamWriter = new StreamWriter(stream, Framework.Defs.Encoding, 1024, leaveOpen: true))
            using (var writer = new JsonTextWriter(streamWriter))
            {
                Write(writer);
            }
            stream.Position = 0;
            return stream;
        }

        protected abstract bool ReadProperty(string property, JsonReader reader);
        protected abstract void WriteProperties(JsonWriter writer);
    }
}