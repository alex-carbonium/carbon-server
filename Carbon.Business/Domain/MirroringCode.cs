using Carbon.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carbon.Business.Domain
{
    public class MirroringCode : IDomainObject<string>
    {
        public string Id { get; set; }

        public string UserId { get; set; }

        public static readonly IEqualityComparer<MirroringCode> UniqueComparer = new UniquePropertyComparer<MirroringCode>(
            x => x.Id);
    }
}
