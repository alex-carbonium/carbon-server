using System;
using Carbon.Framework.Attributes;
using Carbon.Framework.Models;

namespace Carbon.Business.Domain
{
    public class PasswordResetToken : DomainObject
    {
        public PasswordResetToken()
        {
            Expires = DateTime.Now.AddHours(24);
        }
        [Unique, Length(255)]
        public virtual string Code { get; set; }
        public virtual DateTime Expires { get; set; }
        public virtual string Email { get; set; }
        public virtual bool Used { get; set; }

        public virtual bool IsExpired()
        {
            return Expires < DateTime.Now;
        }
    }
}