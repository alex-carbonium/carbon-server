using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Carbon.Business;
using Microsoft.AspNet.Identity;

namespace Carbon.Services.IdentityServer
{
    public class ApplicationUserValidator : UserValidator<ApplicationUser>
    {
        private readonly UserManager<ApplicationUser, string> _manager;

        public ApplicationUserValidator(UserManager<ApplicationUser, string> manager) : base(manager)
        {
            _manager = manager;
        }

        public override async Task<IdentityResult> ValidateAsync(ApplicationUser user)
        {
            var errors = new List<string>();            
            ValidateUserName(user, errors);
            if (RequireUniqueEmail)
            {
                await ValidateEmail(_manager, user, errors);
            }
            return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }

        private static void ValidateUserName(ApplicationUser user, ICollection<string> errors)
        {            
            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                errors.Add(Strings.InvalidUsername);
            }                        
        }

        // make sure email is not empty, valid, and unique
        private static async Task ValidateEmail(UserManager<ApplicationUser, string> manager, ApplicationUser user, ICollection<string> errors)
        {
            try
            {
                var m = new MailAddress(user.Email);
            }
            catch (FormatException)
            {
                errors.Add(Strings.InvalideEmail);
                return;
            }

            var owner = await manager.FindByEmailAsync(user.Email);
            if (owner != null && !string.Equals(owner.Id, user.Id))
            {
                errors.Add(Strings.DuplicateEmail);
            }
        }
    }
}