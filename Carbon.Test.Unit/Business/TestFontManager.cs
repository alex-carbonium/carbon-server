using Carbon.Business.Domain;
using Newtonsoft.Json.Linq;

namespace Carbon.Test.Unit.Business
{
    public class TestFontManager : FontManager
    {
        public TestFontManager()
        {
            Initialize(JObject.Parse(@"
{
    collection: [
        {
            name: 'Arial',
            fonts: []    
        },
        {
            name: 'Blokk',
            fonts: []    
        }
    ],
    popular: []
}
"));
        }
    }
}
