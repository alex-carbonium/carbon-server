using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Carbon.Framework.Models;

namespace Carbon.Business.Domain
{
    public partial class PromoCode : DomainObject
    {
        public virtual string Code { get; set; }
        public virtual int DiscountMonths { get; set; }
        public virtual int UsageCount { get; set; }
        public virtual int UsedTimes { get; set; }
        public virtual double Price { get; set; }
        public virtual string CampaignName { get; set; }
    }
}
