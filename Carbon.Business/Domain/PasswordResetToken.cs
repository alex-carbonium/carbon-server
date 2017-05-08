using Microsoft.WindowsAzure.Storage.Table;

namespace Carbon.Business.Domain
{
    public class PasswordResetToken : TableEntity
    {
        public PasswordResetToken()
        {
        }
        public PasswordResetToken(string key, string userId)
        {
            PartitionKey = key;
            RowKey = key;

            UserId = userId;
        }

        public string UserId { get; set; }
    }
}