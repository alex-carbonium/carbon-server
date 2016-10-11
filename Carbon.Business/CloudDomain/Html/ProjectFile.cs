using Carbon.Framework.Cloud.Blob;

namespace Carbon.Business.CloudDomain.Html
{
    [Container(Name = "html", Type = ContainerType.Public)]
    public class ProjectFile : BlobDomainObject
    {
        public ProjectFile()
        {            
        }

        public ProjectFile(string prefix, string filePath)
        {
            Id = prefix + "/" + filePath;
        }
    }
}
