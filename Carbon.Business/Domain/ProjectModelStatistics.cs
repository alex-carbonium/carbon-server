using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Carbon.Business.Domain
{
    public class ProjectModelStatistics
    {
        public List<FontUsage> FontUsage { get; set; }
        public JArray FontMetadata { get; set; }
    }    

    public class FontUsage : IEquatable<FontUsage>
    {
        public string Family { get; set; }
        public int Weight { get; set; }
        public int Style { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as FontUsage);
        }

        public override int GetHashCode()
        {
            var hashCode = Family?.GetHashCode() ?? 0;
            hashCode = (hashCode * 397) ^ Weight;
            hashCode = (hashCode * 397) ^ Style;
            return hashCode;
        }

        public bool Equals(FontUsage other)
        {
            if (other == null)
            {
                return false;
            }
            return Family == other.Family && Weight == other.Weight && Style == other.Style;
        }
    }
}