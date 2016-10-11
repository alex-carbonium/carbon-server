using System;
using Carbon.Framework.Cloud.Blob;

namespace Carbon.Business.CloudDomain
{
    [Container(Name = "Projects", Type = ContainerType.Private)]
    public class ProjectSnapshot : BlobDomainObject
    {
        public ProjectSnapshot()
        {            
        }
        public ProjectSnapshot(string id, DateTimeOffset dateTime)
        {
            Id = id;
            DateTime = dateTime;                        
        }

        public string EditVersion 
        {
            get { return Metadata["editVersion"]; }
            set { Metadata["editVersion"] = value; }
        }
        public DateTimeOffset DateTime
        {
            get { return new DateTimeOffset(long.Parse(Metadata["dateTime"]), TimeSpan.Zero); }
            set { Metadata["dateTime"] = value.UtcTicks.ToString(); }
        }

        public static string LatestId(string companyId, string projectId)
        {
            return companyId + "/" + projectId + "/latest.zip";
        }        
    }
}