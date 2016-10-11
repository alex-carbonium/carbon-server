using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Carbon.Framework.Models;

namespace Carbon.Business.Domain.Marketing
{
    public class Campaign : DomainObject
    {
        public virtual string Name { get; set; }
        public virtual bool IncludeUnsubsribedUsers { get; set; }
    }
}
