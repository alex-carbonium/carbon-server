using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Carbon.Business;
using Carbon.Business.Domain;
using Carbon.Business.Services;
using Carbon.Framework.Validation;
using IdentityServer3.AspNetIdentity;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Carbon.Services.IdentityServer
{
    public class IdentityUserService : AspNetIdentityUserService<ApplicationUser,string>
    {
        //private readonly IDependencyContainer _container;
        //private readonly ILogService _logService;

        public static IUserService Create(AppSettings appSettings)
        {
            var context = new ApplicationDbContext(appSettings);
            var userManager = new ApplicationUserManager(context);
            return new IdentityUserService(userManager);
        }

        public IdentityUserService(UserManager<ApplicationUser, string> userManager)
            : base(userManager)
        {
//            _container = container;
//            _logService = logService;
        }

        public override async Task AuthenticateLocalAsync(LocalAuthenticationContext ctx)
        {
            //using (var scope = _container.BeginScope())
            {
                //var userService = scope.Resolve<IUserService>();
                //var uow = scope.Resolve<IUnitOfWork>();

                if (ctx.UserName == "trial" && ctx.Password == "trial")
                {
                    ctx.AuthenticateResult = AuthenticateNewGuestUser();
                    //uow.Commit();
                }
                else
                {
                    await base.AuthenticateLocalAsync(ctx);
                }
            }
        }

        protected override Task<ApplicationUser> FindUserAsync(string email)
        {
            return userManager.FindByEmailAsync(email);
        }

        public override Task IsActiveAsync(IsActiveContext ctx)
        {
            ctx.IsActive = true;
            return Task.FromResult(true);
        }

        public Task<AuthenticateResult> AuthenticateExternalAsync(ExternalIdentity externalUser, SignInMessage message)
        {
            if (string.IsNullOrEmpty(externalUser.ProviderId))
            {
                throw new Exception("Unknown user ID from provider " + externalUser.Provider);
            }
            RegistrationType registrationType;
            if (!Enum.TryParse(externalUser.Provider, out registrationType))
            {
                throw new Exception("Unknown provider " + externalUser.Provider);
            }

            //using (var scope = _container.BeginScope())
            {
//                var userService = scope.Resolve<IUserService>();
//                var uow = scope.Resolve<IUnitOfWork>();

                //var user = new User { RegistrationType = registrationType, ExternalId = externalUser.ProviderId, SubscribeForUpdates = true };
                //string legacyId = null;
                //switch (user.RegistrationType)
                //{
                //    case RegistrationType.Google:
                //        user.FriendlyName = GetFirstClaim(externalUser.Claims, "name");
                //        user.Email = GetFirstClaim(externalUser.Claims, "email");
                //        legacyId = user.Email;
                //        break;
                //    case RegistrationType.Twitter:
                //        user.FriendlyName = GetFirstClaim(externalUser.Claims, "name", "urn:twitter:screenname");
                //        legacyId = GetFirstClaim(externalUser.Claims, "name");
                //        break;
                //    case RegistrationType.Facebook:
                //        user.FriendlyName = GetFirstClaim(externalUser.Claims, "name", "urn:facebook:name");
                //        user.Email = GetFirstClaim(externalUser.Claims, "email");
                //        break;
                //    default:
                //        throw new Exception("Unexpected registration type " + registrationType);
                //}

                var validator = new DictionaryValidator();
                //var email = user.Email;
                //User existingUser;
                //var canLogin = userService.ValidateUser(user, validator, out existingUser, legacyId);
                //if (existingUser == null)
                //{
                //    canLogin = false;
                //    if (!string.IsNullOrEmpty(user.Email) && validator.Errors.ContainsKey("email"))
                //    {
                //        user.Email = null;
                //    }
                //    userService.RegisterNewUser(user, sendWelcomeEmail: false);
                //    existingUser = user;
                //}
                //else
                //{
                //    existingUser.ExternalId = externalUser.ProviderId;
                //}

                //var subjectName = string.IsNullOrWhiteSpace(existingUser.FriendlyName) ? "Guest" : existingUser.FriendlyName;

                //TODO: fix external accounts
                //new ExternalIdentity() { }
                //new AuthenticateResult()
                //var result = canLogin
                //    ? new ExternalAuthenticateResult(externalUser.Provider.Name, existingUser.Id.ToString(), subjectName)
                //    : new ExternalAuthenticateResult("/account/mustUpdateSettings", externalUser.Provider.Name, existingUser.Id.ToString(), subjectName);

                //result.RedirectClaims.Add(new Claim(ClaimTypes.Email, email ?? string.Empty));

                //uow.Commit();

                //return Task.FromResult(result);
                return null;
            }
        }

        private AuthenticateResult AuthenticateNewGuestUser()
        {
            var id = Guid.NewGuid().ToString("N");
            return new AuthenticateResult(id, id, new List<Claim>
            {
                new Claim(Framework.Defs.ClaimTypes.Subject, id)
            });
        }

        private string GetFirstClaim(IEnumerable<Claim> claims, params string[] names)
        {
            foreach (var name in names)
            {
                var claim = claims.FirstOrDefault(x => x.Type == name);
                if (claim != null)
                {
                    return claim.Value;
                }
            }
            //_logService.GetLogger(this).Warning("Could not find claims " + string.Join(",", names) + " among " + claims.Aggregate(string.Empty, (current, c) => current + c.Type + ","));
            return null;
        }
    }
}