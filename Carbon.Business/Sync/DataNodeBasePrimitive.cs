using System;
using Carbon.Business.Domain.DataTree;
using Newtonsoft.Json;

namespace Carbon.Business.Sync
{
    public abstract class DataNodeBasePrimitive : Primitive
    {
        protected DataNodeBasePrimitive(PrimitiveType type) : base(type)
        {
        }

        public DataNodePath Path { get; set; }        

        protected override bool ReadProperty(string property, JsonReader reader)
        {
            if (base.ReadProperty(property, reader))
            {
                return true;
            }
            switch (property)
            {                
                case "path":
                    var segment1 = string.Empty;
                    var segment2 = string.Empty;
                    var segment3 = string.Empty;
                    var count = 0;

                    ReadAssert(reader, JsonToken.PropertyName);                     
                    ReadAssert(reader, JsonToken.StartArray);

                    while (reader.TokenType != JsonToken.EndArray)
                    {
                        switch (count)
                        {
                            case 0:
                                segment1 = (string)reader.Value;
                                break;
                            case 1:
                                segment2 = (string)reader.Value;
                                break;
                            case 2:
                                segment3 = (string)reader.Value;
                                break;
                            default:
                                throw new Exception("Unexpected number of segments in the path");
                        }
                        reader.Read();
                        ++count;
                    }

                    ReadAssert(reader, JsonToken.EndArray);
                    
                    Path = new DataNodePath(segment1, segment2, segment3);

                    return true;
                default:
                    return false;
            }
        }

        protected override void WriteProperties(JsonWriter writer)
        {
            base.WriteProperties(writer);

            writer.WritePropertyName("path");
            writer.WriteStartArray();
            if (!string.IsNullOrEmpty(Path.Segment1))
            {
                writer.WriteValue(Path.Segment1);
            }
            if (!string.IsNullOrEmpty(Path.Segment2))
            {
                writer.WriteValue(Path.Segment2);
            }
            if (!string.IsNullOrEmpty(Path.Segment3))
            {
                writer.WriteValue(Path.Segment3);
            }
            writer.WriteEndArray();
        }
    }
}