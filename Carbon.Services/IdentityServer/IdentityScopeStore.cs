using System.Collections.Generic;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.InMemory;

namespace Carbon.Services.IdentityServer
{
    public class IdentityScopeStore : InMemoryScopeStore
    {
        public const string Account = "account";

        public static readonly IEnumerable<Scope> All = new List<Scope>
        {
            new Scope
            {
                Name = Account,
                DisplayName = Account,
                Type = ScopeType.Resource,
                Emphasize = false
            },
            StandardScopes.OpenId
        };

        public IdentityScopeStore() : base(All)
        {
        }
    }
}