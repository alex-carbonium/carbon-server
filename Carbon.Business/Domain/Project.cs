using Carbon.Framework.Models;

namespace Carbon.Business.Domain
{
    public class Project : IDomainObject<string>
    {                        
        public string Id { get; set; }
        public virtual string Name { get; set; }             
        public virtual string MirroringCode { get; set; }                                                  
    }
}
