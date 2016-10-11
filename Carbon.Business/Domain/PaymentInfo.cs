namespace Carbon.Business.Domain
{
    public class PaymentInfo
    {
        //public SubscriptionType SubscriptionType { get; set; }
        public string Ticket { get; set; }        
        public string CardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public string CVC { get; set; }        
    }
}