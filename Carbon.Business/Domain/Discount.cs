using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Carbon.Framework.Models;

namespace Carbon.Business.Domain
{
    public partial class Discount:DomainObject
    {
        public virtual int DiscountMonths { get; set; }
        public virtual double Price { get; set; }
        public virtual DateTime CreateDateTime { get; set; }
        public virtual PromoCode PromoCode { get; set; }
    }
}
