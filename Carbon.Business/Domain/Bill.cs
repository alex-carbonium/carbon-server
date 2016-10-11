using System;
using Carbon.Framework.Models;

namespace Carbon.Business.Domain
{    
    public partial class Bill : DomainObject
    {
        public virtual int Amount { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual bool Paid { get; set; }        
        public virtual string TransactionId { get; set; }        
        public virtual DateTime? LastNotificationDate { get; set; }        
        public virtual bool HasProDiscount { get; set; }        

        public virtual string GetOrderDescription()
        {
            const string description = "Monthly Subscription";            
            if (Id == 0)
            {
                return description;
            }
            return string.Format("{0} - {1}", Id, description);
        }

        public virtual int DaysSinceLastNotification(DateTime currentDate)
        {
            if (LastNotificationDate == null)
            {
                return int.MaxValue;
            }
            return (int)(currentDate - LastNotificationDate.Value).TotalDays;
        }

        public static Bill Create(DateTime date, int amount, bool paid = false)
        {
            var bill = new Bill();                        
            bill.Date = date;            
            bill.Amount = amount;
            bill.Paid = paid;
            return bill;
        }
    }
}
