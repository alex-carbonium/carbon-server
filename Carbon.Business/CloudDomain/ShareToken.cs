using Carbon.Business.Domain;
using Microsoft.WindowsAzure.Storage.Table;

namespace Carbon.Business.CloudDomain
{
    public class ShareToken : TableEntity
    {
        public void SetCode(string code)
        {
            PartitionKey = code;
            RowKey = code;
        }

        public string CompanyId { get; set; }
        public string ProjectId { get; set; }
        public int Permission { get; set; }
        public string Email { get; set; }
        public int TimesUsed { get; set; }
        public string CreatedByUserId { get; set; }
    }
}
