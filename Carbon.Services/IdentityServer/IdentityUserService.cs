using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Carbon.Business;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Microsoft.AspNet.Identity;
using Constants = IdentityServer3.Core.Constants;
using Carbon.Business.Services;
using IdentityServer3.Core.Extensions;

namespace Carbon.Services.IdentityServer
{
    public class IdentityUserService : AspNetIdentityUserService<ApplicationUser,string>
    {
        public static IUserService Create(AppSettings appSettings, IUserTokenProvider<ApplicationUser, string> tokenProvider, AccountService accountService)
        {
            var context = new ApplicationDbContext(appSettings);
            var userManager = new ApplicationUserManager(context, tokenProvider);
            return new IdentityUserService(userManager, accountService);
        }

        private readonly AccountService _accountService;

        public IdentityUserService(UserManager<ApplicationUser, string> userManager, AccountService accountService)
            : base(userManager)
        {
            _accountService = accountService;
        }

        public override async Task AuthenticateLocalAsync(LocalAuthenticationContext ctx)
        {
            if (ctx.UserName == "trial" && ctx.Password == "trial")
            {
                ctx.AuthenticateResult = AuthenticateNewGuestUser();
            }
            else
            {
                await base.AuthenticateLocalAsync(ctx);
            }
        }

        protected override Task<ApplicationUser> FindUserAsync(string email)
        {
            return userManager.FindByEmailAsync(email);
        }

        protected override Task<ApplicationUser> InstantiateNewUserFromExternalProviderAsync(string provider, string providerId, IEnumerable<Claim> claims)
        {
            var user = CreateUserFromClaims(NewUserId(), claims);
            return Task.FromResult(user);
        }

        protected override async Task<ApplicationUser> TryGetExistingUserFromExternalProviderClaimsAsync(string provider, IEnumerable<Claim> claims, string tenantId)
        {
            var email = GetEmailClaim(claims);
            if (!string.IsNullOrEmpty(email))
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    //TODO: if user is not the same tenant, copy the projects
                    return user;
                }
            }

            if (!string.IsNullOrEmpty(tenantId))
            {
                //TODO: validate tenantId
                var user = await userManager.FindByIdAsync(tenantId);
                if (user == null) //guest
                {
                    user = CreateUserFromClaims(tenantId, claims);
                    await Task.WhenAll(
                        userManager.CreateAsync(user),
                        _accountService.RegisterNewUser(tenantId, user.UserName, user.Email));
                }
                return user;
            }

            return null;
        }

        protected override async Task<AuthenticateResult> AccountCreatedFromExternalProviderAsync(ApplicationUser user, string provider, string providerId, IEnumerable<Claim> claims, bool isNewUser)
        {
            if (isNewUser)
            {
                await _accountService.RegisterNewUser(user.Id, user.UserName, user.Email);
            }

            return null;
        }

        public override async Task IsActiveAsync(IsActiveContext ctx)
        {
            var id = ctx.Subject.GetSubjectId();
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                //guest users are not persisted
                ctx.IsActive = true;
            }
            else
            {
                await IsActiveAsync(ctx, user);
            }
        }

        private AuthenticateResult AuthenticateNewGuestUser()
        {
            var id = NewUserId();
            return new AuthenticateResult(id, id, new List<Claim>
            {
                new Claim(Constants.ClaimTypes.Subject, id)
            });
        }

        private ApplicationUser CreateUserFromClaims(string userId, IEnumerable<Claim> claims)
        {
            return new ApplicationUser
            {
                Id = userId,
                Email = GetEmailClaim(claims) ?? _accountService.MakeInternalEmail(userId),
                UserName = GetFirstClaim(claims, Constants.ClaimTypes.Name, Constants.ClaimTypes.GivenName, Constants.ClaimTypes.FamilyName) ?? "guest"
            };
        }

        private static string NewUserId()
        {
            return Guid.NewGuid().ToString("N");
        }

        private static string GetEmailClaim(IEnumerable<Claim> claims)
        {
            var email = GetFirstClaim(claims, Constants.ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email) && email.Contains("@"))
            {
                //sometimes emails are returned as user ids, etc
                return email;
            }
            return null;
        }

        private static string GetFirstClaim(IEnumerable<Claim> claims, params string[] names)
        {
            foreach (var name in names)
            {
                var claim = claims.FirstOrDefault(x => x.Type == name);
                if (claim != null && !string.IsNullOrWhiteSpace(claim.Value))
                {
                    return claim.Value;
                }
            }
            return null;
        }
    }
}