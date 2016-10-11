using System;
using Carbon.Framework.Models;

namespace Carbon.Business.Domain
{
    [Obsolete("Project member table is obsolete, use ACL instead")]
    public partial class ProjectMember : DomainObject
    {
        public virtual User Member { get; set; }
        public virtual Project Project { get; set; }
        public virtual ProjectMembership Membership { get; set; }
    }
}
