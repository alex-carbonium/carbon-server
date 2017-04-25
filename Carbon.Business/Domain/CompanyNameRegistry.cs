using Microsoft.WindowsAzure.Storage.Table;

namespace Carbon.Business.Domain
{
    public class CompanyNameRegistry : TableEntity
    {
        public CompanyNameRegistry()
        {
        }
        public CompanyNameRegistry(string name, string companyId)
        {
            PartitionKey = name;
            RowKey = name;
            CompanyId = companyId;
        }
        public string CompanyId { get; set; }
    }
}
