using System.Threading.Tasks;
using System.Web.Http;
using Carbon.Business.Services;
using Carbon.Owin.Common.WebApi;
using Carbon.Services.IdentityServer;
using System.Linq;
using Carbon.Framework.Pools;
using Carbon.Business;
using System;
using Carbon.Business.Domain;
using Carbon.Framework.Repositories;
using Carbon.Business.CloudSpecifications;
using System.Net;

namespace Carbon.Services.Controllers
{
    [RoutePrefix("account")]
    public class AccountController : AuthorizedApiController
    {
        private readonly IActorFabric _actorFabric;
        private readonly ApplicationUserManager _userManager;
        private readonly AccountService _accountService;
        private readonly IMailService _mailService;
        private readonly IRepository<PasswordResetToken> _tokenRepository;

        public AccountController(IActorFabric actorFabric, ApplicationUserManager userManager, AccountService accountService,
            IMailService mailService, IRepository<PasswordResetToken> tokenRepository)
        {
            _actorFabric = actorFabric;
            _userManager = userManager;
            _accountService = accountService;
            _mailService = mailService;
            _tokenRepository = tokenRepository;
        }

        public class RegisterModel
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class EmailValidationModel
        {
            public string Email { get; set; }
        }

        [AllowAnonymous, HttpPost, Route("validateEmail")]
        public async Task<IHttpActionResult> ValidateEmail(EmailValidationModel model)
        {
            var userId = GetUserId();
            ApplicationUser current = null;
            if (!string.IsNullOrEmpty(userId))
            {
                current = await _userManager.FindByIdAsync(userId);
            }

            var other = await _userManager.FindByEmailAsync(model.Email);
            var duplicate = false;
            if (current == null)
            {
                duplicate = other != null;
            }
            else
            {
                duplicate = other != null && other.Id != current.Id;
            }

            if (duplicate)
            {
                return Error(nameof(model.Email), "@account.duplicateEmail");
            }

            return Success();
        }

        [AllowAnonymous, HttpPost, Route("register")]
        public async Task<IHttpActionResult> Register(RegisterModel model)
        {
            ApplicationUser existing = null;
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                existing = await _userManager.FindByIdAsync(userId);
            }

            if (existing == null)
            {
                var user = new ApplicationUser
                {
                    Id = userId,
                    Email = model.Email,
                    UserName = model.Username
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    return Error(nameof(model.Email), result.Errors);
                }
            }

            await _accountService.RegisterNewUser(userId, model.Username, model.Email);

            return Success();
        }

        [HttpPost, Route("resolveCompanyId")]
        public async Task<IHttpActionResult> ResolveCompanyId(string companyName)
        {
            var actor = _actorFabric.GetProxy<ICompanyActor>(GetUserId());           
            var companyId = await actor.ResolveExternalCompanyId(companyName);
            return Ok(new { CompanyId = companyId });
        }

        [HttpGet, Route("getCompanyName")]
        public async Task<IHttpActionResult> GetCompanyName()
        {
            var actor = _actorFabric.GetProxy<ICompanyActor>(GetUserId());
            var info = await actor.GetCompanyInfo();
            return Ok(new { CompanyName = info.Name });
        }

        [HttpGet, Route("overview")]
        public async Task<IHttpActionResult> Overview()
        {
            var userId = GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return Ok(new { HasAccount = false });
            }

            bool hasPassword = false;
            if (user != null)
            {
                hasPassword = await _userManager.HasPasswordAsync(userId);
            }

            return Ok(new
            {
                HasAccount = true,
                Info = new
                {
                    Username = user.UserName,
                    Email = _accountService.IsRealEmail(user.Email) ? user.Email : "",
                },
                HasPassword = hasPassword,
                EnabledProviders = user.Logins.Select(x => x.LoginProvider)
            });
        }

        public class InfoModel
        {
            public string Username { get; set; }
            public string Email { get; set; }
        }

        [HttpPost, Route("info")]
        public async Task<IHttpActionResult> UpdateInfo(InfoModel model)
        {
            using (var errors = PooledDictionary<string, string>.GetInstance())
            {
                var user = await _userManager.FindByIdAsync(GetUserId());

                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    errors[nameof(model.Email)] = "@account.noEmail";
                }
                else if (!model.Email.Contains("@"))
                {
                    errors[nameof(model.Email)] = "@account.badEmail";
                }
                else
                {
                    var other = await _userManager.FindByEmailAsync(model.Email);
                    if (other != null && other.Id != user.Id)
                    {
                        return Error(nameof(model.Email), "@account.duplicateEmail");
                    }
                }

                if (string.IsNullOrWhiteSpace(model.Username))
                {
                    errors[nameof(model.Username)] = "@account.noUsername";
                }

                if (errors.Count != 0)
                {
                    return Error(errors);
                }

                user.Email = model.Email;
                user.UserName = model.Username;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    errors[nameof(model.Email)] = string.Join(", ", result.Errors);
                    return Error(errors);
                }

                var actor = _actorFabric.GetProxy<ICompanyActor>(GetUserId());
                await actor.UpdateOwnerInfo(new UserInfo { Name = user.UserName, Email = user.Email, Avatar = user.Avatar });

                return Success();
            }
        }

        public class AddPasswordModel
        {
            public string NewPassword { get; set; }
        }
        [HttpPost, Route("addPassword")]
        public async Task<IHttpActionResult> AddPassword(AddPasswordModel model)
        {
            using (var errors = PooledDictionary<string, string>.GetInstance())
            {
                if (string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    errors[nameof(model.NewPassword)] = "@account.noPassword";
                }

                if (errors.Count != 0)
                {
                    return Error(errors);
                }

                var user = await _userManager.FindByIdAsync(GetUserId());
                var result = await _userManager.AddPasswordAsync(GetUserId(), model.NewPassword);

                if (!result.Succeeded)
                {
                    return Error(nameof(model.NewPassword), result.Errors);
                }

                return Success();
            }
        }

        public class ChangePasswordModel
        {
            public string OldPassword { get; set; }
            public string NewPassword { get; set; }
        }
        [HttpPost, Route("changePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordModel model)
        {
            using (var errors = PooledDictionary<string, string>.GetInstance())
            {
                if (string.IsNullOrWhiteSpace(model.OldPassword))
                {
                    errors[nameof(model.OldPassword)] = "@account.noPassword";
                }
                if (string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    errors[nameof(model.NewPassword)] = "@account.noPassword";
                }

                if (errors.Count != 0)
                {
                    return Error(errors);
                }

                var result = await _userManager.ChangePasswordAsync(GetUserId(), model.OldPassword, model.NewPassword);

                if (!result.Succeeded)
                {
                    return Error(nameof(model.OldPassword), result.Errors);
                }

                return Success();
            }
        }

        public class RequestPasswordChangeModel
        {
            public string Email { get; set; }
        }
        [HttpPost, Route("forgotPassword")]
        public async Task<IHttpActionResult> ForgotPassword(RequestPasswordChangeModel model)
        {
            using (var errors = PooledDictionary<string, string>.GetInstance())
            {
                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    errors[nameof(model.Email)] = "@account.noEmail";
                }

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    errors[nameof(model.Email)] = "@account.wrongEmail";
                }

                if (errors.Count != 0)
                {
                    return Error(errors);
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user.Id);
                await Task.WhenAll(
                    _tokenRepository.InsertAsync(new PasswordResetToken(token.GetHashCode().ToString(), user.Id)),
                    _mailService.Send(model.Email, "forgotPassword", new
                    {
                        Link = AppSettings.SiteHost + "/account/resetPassword/" + Uri.EscapeDataString(token)
                    })
               );

                return Success();
            }
        }

        public class ResetPasswordModel
        {
            public string Token { get; set; }
            public string NewPassword { get; set; }
        }
        [HttpPost, Route("resetPassword")]
        public async Task<IHttpActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Token))
            {
                return Error(nameof(model.NewPassword), "@account.wrongLink");
            }

            var hash = model.Token.GetHashCode().ToString();
            var spec = new FindByRowKey<PasswordResetToken>(hash, hash);
            var token = await _tokenRepository.FindSingleByAsync(spec);
            if (token == null)
            {
                return Error(nameof(model.NewPassword), "@account.wrongLink");
            }

            var user = await _userManager.FindByIdAsync(token.UserId);
            if (user == null)
            {
                return Error(nameof(model.NewPassword), "@account.wrongLink");
            }

            if (string.IsNullOrWhiteSpace(model.NewPassword))
            {
                return Error(nameof(model.NewPassword), "@account.noPassword");
            }

            var result = await _userManager.ResetPasswordAsync(user.Id, model.Token, model.NewPassword);
            if (!result.Succeeded)
            {
                LogService.GetLogger().Warning(string.Join(";", result.Errors), Scope);
                return Error(nameof(model.NewPassword), "@account.wrongLink");
            }

            await Task.WhenAll(
                _tokenRepository.DeleteAsync(token),
                _mailService.Send(user.Email, "passwordChanged", new
                {
                    Link = AppSettings.SiteHost + "/account"
                })
            );

            return Success();
        }
    }
}