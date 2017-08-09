using Carbon.Framework.Models;

namespace Carbon.Business.Domain.Statistics
{
    public class ProjectInfo : DomainObject
    {
        public virtual string ProjectId { get; set; }
        public virtual string ProjectName { get; set; }
        public virtual string UserEmail { get; set; }
        public virtual string UserName { get; set; }
        public virtual string CompanyName { get; set; }
        public virtual long TimesSaved { get; set; }
    }
}
