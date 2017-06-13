using Carbon.Framework.Cloud.Blob;

namespace Carbon.Business.CloudDomain
{
    [Container(Name = ContainerName, Type = ContainerType.Public)]
    public class CompanyFile : BlobDomainObject
    {
        public const string ContainerName = "companyfiles";

        public CompanyFile()
        {
        }
        public CompanyFile(string companyId, string fileId)
        {
            Id = companyId + "/" + fileId;
            AutoDetectContentType();
        }

        public static string FullRelativeUrl(string companyId, string fileId)
        {
            return "/" + ContainerName + "/" + companyId + "/" + fileId;
        }
    }
}