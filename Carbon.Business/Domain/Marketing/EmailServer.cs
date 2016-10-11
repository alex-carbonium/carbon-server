using System;
using Carbon.Framework.Models;

namespace Carbon.Business.Domain.Marketing
{
    public class EmailServer : DomainObject
    {
        public virtual string Host { get; set; }
        public virtual int Port { get; set; }
        public virtual string UserName { get; set; }
        public virtual string Password { get; set; }
        public virtual int Limit { get; set; }
        public virtual int Usage { get; set; }
        public virtual DateTime PeriodStart { get; set; }
    }
}
