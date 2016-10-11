using System.Collections.Generic;
using System.Linq;
using Carbon.Business.Domain.DataTree;
using Newtonsoft.Json.Linq;

namespace Carbon.Business.Domain
{
    public class ProjectModelStatisticsVisitor : DataTreeVisitor
    {
        private readonly FontManager _fontManager;        

        public ProjectModelStatisticsVisitor(FontManager fontManager)
        {
            _fontManager = fontManager;
            Statistics.FontUsage = new List<FontUsage>();
            Statistics.FontMetadata = new JArray();
        }

        public override bool Visit(DataNode element, DataNodePath path)
        {            
            if (element.Type == NodeType.Text)
            {
                var font = (JObject)element.GetProp("font");
                if (font != null)
                {
                    AddFontUsage(font);
                }
                var content = element.GetProp("content") as JArray;
                if (content != null)
                {
                    foreach (var range in content)
                    {
                        AddFontUsage(range);
                    }
                }
            }
            return true;
        }

        public ProjectModelStatistics Statistics { get; } = new ProjectModelStatistics();

        private void AddFontUsage(JToken token)
        {
            var family = token.Value<string>("family");
            var weight = token.Value<int>("weight");
            var style = token.Value<int>("style");
            if (family != null || weight != 0 || style != 0)
            {
                if (!Statistics.FontUsage.Any(x => x.Family == family && x.Weight == weight && x.Style == style))
                {
                    var usage = new FontUsage
                    {
                        Family = family,
                        Weight = weight,
                        Style = style
                    };
                    Statistics.FontUsage.Add(usage);
                }

                if (family != null)
                {
                    if (Statistics.FontMetadata.All(x => x.Value<string>("name") != family))
                    {
                        Statistics.FontMetadata.Add(_fontManager.GetMetadata(family));
                    }
                }
            }
        }
    }    
}