using System;
using System.Collections.Generic;
using Carbon.Framework.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Carbon.Business.Domain
{
    public class CompanyFileInfo : IDomainObject<string>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string Metadata { get; set; }
        public DateTime ModifiedDateTime { get; set; }

        public JObject GetMetadata()
        {
            if (string.IsNullOrEmpty(Metadata))
            {
                return new JObject();
            }
            return JsonConvert.DeserializeObject<JObject>(Metadata);
        }

        public void UpdateMetadata(JObject metadata)
        {
            Metadata = metadata.ToString(Formatting.None);
        }

        public static readonly IEqualityComparer<CompanyFileInfo> UniqueComparer = new UniquePropertyComparer<CompanyFileInfo>(
            x => x.Name);
    }
}