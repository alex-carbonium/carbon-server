using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Carbon.Framework.Util;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Validation;
using IdentityServer3.Core.Services.Default;
using System.Linq;
using IdentityServer3.Core;

namespace Carbon.Services.IdentityServer
{
    public class IdentityClaimsProvider : DefaultClaimsProvider
    {
        private readonly IDependencyContainer _container;

        public IdentityClaimsProvider(IUserService users, IDependencyContainer container) : base(users)
        {
            _container = container;
        }

        public override async Task<IEnumerable<Claim>> GetAccessTokenClaimsAsync(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, ValidatedRequest request)
        {
            var baseClaims = await base.GetAccessTokenClaimsAsync(subject, client, scopes, request);

            var userId = subject.Claims.SingleOrDefault(x => x.Type == Constants.ClaimTypes.Subject)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return baseClaims;
            }

            var newClaims = new List<Claim>();

            using (var scope = _container.BeginScope())
            {
                var userManager = scope.Resolve<ApplicationUserManager>();
                var user = await userManager.FindByIdAsync(userId);

                if (user?.Roles != null)
                {
                    foreach (var role in user.Roles)
                    {
                        newClaims.Add(new Claim(Constants.ClaimTypes.Role, role.RoleName));
                    }
                }
            }

            return newClaims.Concat(baseClaims);
        }
    }
}