using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Carbon.Business.Sync
{
    public static class PrimitiveFactory
    {        
        private static readonly JsonSerializer _serializer;

        static PrimitiveFactory()
        {                                    
            _serializer = new JsonSerializer();
            _serializer.MaxDepth = int.MaxValue;
            _serializer.Converters.Clear();
            _serializer.Converters.Add(new PrimitiveConverter());
        }

        public static T Create<T>(string json) where T : RawPrimitive
        {
            using (var stringReader = new StringReader(json))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                var primitive = _serializer.Deserialize<T>(jsonReader);
                primitive.SourceString = json;
                return primitive;
            }            
        }

        public static IEnumerable<T> CreateMany<T>(IEnumerable<string> jsonList) where T : RawPrimitive
        {
            return jsonList.Select(Create<RawPrimitive>).OfType<T>();
        }
    }
}