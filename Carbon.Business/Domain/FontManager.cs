using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Carbon.Business.Domain
{
    public class FontManager
    {
        private JObject _metadata;

        public JObject GetMetadata(string fontName)
        {
            CheckInitialized();
            return (JObject) _metadata.Value<JArray>("collection").SingleOrDefault(x => x.Value<string>("name") == fontName);
        }

        private void CheckInitialized()
        {
            if (_metadata == null)
            {
                throw new InvalidOperationException("Font manager not initialized");
            }
        }

        public JArray Collection
        {
            get
            {
                CheckInitialized();
                return _metadata.Value<JArray>("collection");
            }            
        }
        public JArray Popular
        {
            get
            {
                CheckInitialized();
                return _metadata.Value<JArray>("popular");
            }
            private set { _metadata["popular"] = value; }
        }

        public void Initialize(JObject metadata)
        {
            _metadata = metadata;


            var collection = Collection;
            var popular = Popular
                .Select(x => collection.SingleOrDefault(y => y["name"].Equals(x)))
                .Where(x => x != null);

            Popular = new JArray(popular);                        
        }
    }
}