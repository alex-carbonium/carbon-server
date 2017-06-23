using Carbon.Framework.Models;
using System.Collections.Generic;

namespace Carbon.Business.Domain
{
    public class User : IDomainObject<string>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Avatar { get; set; }

        public static readonly User System = new User { Name = "system" };

        public static readonly IEqualityComparer<User> UniqueComparer = new UniquePropertyComparer<User>(x => x.Id);
    }
}
