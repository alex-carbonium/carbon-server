using System;
using Carbon.Framework.Attributes;
using Carbon.Framework.Models;

namespace Carbon.Business.Domain
{
    public partial class CompanyInvitation : DomainObject
    {
        public virtual string Email { get; set; }
        [Length(255)]
        public virtual string Code { get; set; }
        public virtual string Link { get; set; }
        public virtual bool Purchased { get; set; }
        public virtual bool Accepted { get; set; }
        public virtual string UserId { get; set; }
        public virtual Permission AsRole { get; set; }
    }
}
