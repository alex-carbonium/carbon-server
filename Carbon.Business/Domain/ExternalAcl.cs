namespace Carbon.Business.Domain
{
    public class ExternalAcl : IAclEntryContainer
    {
        public AclEntry Entry { get; set; }
        public string CompanyName { get; set; }
        public string ResourceName { get; set; }

        public static ExternalAcl Create(AclEntry entry, string companyName, string resourceName)
        {
            return new ExternalAcl
            {
                Entry = entry,
                CompanyName = companyName,
                ResourceName = resourceName
            };
        }

        public ExternalAcl WithCompanyName(string newName)
        {
            return new ExternalAcl
            {
                CompanyName = newName,
                ResourceName = ResourceName,
                Entry = Entry
            };
        }
    }
}