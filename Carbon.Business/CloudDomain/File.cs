using Carbon.Framework.Cloud.Blob;

namespace Carbon.Business.CloudDomain
{
    [Container(Name = ContainerName, Type = ContainerType.Public)]
    public class File : BlobDomainObject
    {
        public const string ContainerName = "files";

        public File()
        {
        }

        public File(string prefix, string fileName)
        {
            Id = prefix + "/" + fileName;
            AutoDetectContentType();
        }
    }
}
