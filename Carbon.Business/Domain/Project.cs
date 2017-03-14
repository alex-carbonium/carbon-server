using Carbon.Framework.Models;
using System.Collections.Generic;

namespace Carbon.Business.Domain
{
    public class Project : IDomainObject<string>
    {
        private readonly HashSet<MirroringCode> _mirroringCodes = new HashSet<MirroringCode>(MirroringCode.UniqueComparer);

        public string Id { get; set; }

        public virtual string Name { get; set; }
        //public virtual string MirroringCode { get; set; }

        public ICollection<MirroringCode> MirroringCodes => _mirroringCodes;

    }
}
