namespace Carbon.Business.Domain
{
    public class ExternalAcl : IAclEntryContainer
    {
        public AclEntry Entry { get; set; }
        public string CompanyName { get; set; }
        public string ResourceName { get; set; }
        public string ResourceAvatar { get; set; }

        public static ExternalAcl Create(AclEntry entry, string companyName, string resourceName, string resourceAvatar)
        {
            return new ExternalAcl
            {
                Entry = entry,
                CompanyName = companyName,
                ResourceName = resourceName,
                ResourceAvatar = resourceAvatar
            };
        }

        public ExternalAcl WithCompanyName(string newName)
        {
            return new ExternalAcl
            {
                CompanyName = newName,
                ResourceName = ResourceName,
                ResourceAvatar = ResourceAvatar,
                Entry = Entry
            };
        }
    }
}