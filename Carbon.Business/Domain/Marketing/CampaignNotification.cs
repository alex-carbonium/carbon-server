using Carbon.Framework.Models;

namespace Carbon.Business.Domain.Marketing
{
    public class CampaignNotification : DomainObject
    {
        public virtual Campaign Campaign { get; set; }
        public virtual User User { get; set; }
    }
}