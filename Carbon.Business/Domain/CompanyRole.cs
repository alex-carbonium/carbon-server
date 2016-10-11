using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carbon.Framework.Models;

namespace Carbon.Business.Domain
{
    public partial class CompanyRole : DomainObject
    {
        public virtual Company Company { get; set; }
        public virtual string Name { get; set; }
        public virtual Permission Permissions { get; set; }
    }
}
