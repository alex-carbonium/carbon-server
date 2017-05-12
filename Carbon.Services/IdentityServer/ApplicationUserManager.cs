using System;
using Microsoft.AspNet.Identity;
using ElCamino.AspNet.Identity.AzureTable;

namespace Carbon.Services.IdentityServer
{
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(ApplicationDbContext dbContext, IUserTokenProvider<ApplicationUser, string> tokenProvider)
            : base(new UserStore<ApplicationUser>(dbContext))

        {
            UserValidator = new ApplicationUserValidator(this)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };

            // Configure user lockout defaults
            UserLockoutEnabledByDefault = true;
            DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            MaxFailedAccessAttemptsBeforeLockout = 5;

            UserTokenProvider = tokenProvider;
        }
    }
}