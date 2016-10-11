using System;
using Carbon.Framework.Models;

namespace Carbon.Business.Domain.Marketing
{
    public class SessionBatch : DomainObject
    {
        public virtual Guid SessionId { get; set; }
    }
}
