using Microsoft.WindowsAzure.Storage.Table;

namespace Carbon.Business.Domain
{
    public class CompanyNameRegistry : TableEntity
    {
        public CompanyNameRegistry()
        {            
        }
        public CompanyNameRegistry(string name)
        {
            PartitionKey = name;
            RowKey = name;
        }
    }
}
