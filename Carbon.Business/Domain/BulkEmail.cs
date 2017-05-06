namespace Carbon.Business.Domain
{
    public class BulkEmail
    {
        public long CampaignId { get; set; }
        public int MessageCount { get; set; }
        public bool TestMode { get; set; }
    }
}