using System.Threading.Tasks;
using System.Web.Http;
using Carbon.Business.Services;
using Carbon.Owin.Common.WebApi;
using Carbon.Services.IdentityServer;

namespace Carbon.Services.Controllers
{
    [RoutePrefix("account")]
    public class AccountController : AuthorizedApiController
    {
        private readonly IActorFabric _actorFabric;
        private readonly ApplicationUserManager _userManager;
        private readonly AccountService _accountService;

        public AccountController(IActorFabric actorFabric, ApplicationUserManager userManager, AccountService accountService)
        {
            _actorFabric = actorFabric;
            _userManager = userManager;
            _accountService = accountService;
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

        [HttpPost, Route("validateEmail")]
        public async Task<IHttpActionResult> ValidateEmail(EmailValidationModel model)
        {
            var existing = await _userManager.FindByIdAsync(GetUserId());
            if (existing != null)
            {
                return Success();
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                return Error(nameof(model.Email), "A user with this email is already registered");
            }

            return Success();
        }

        [HttpPost, Route("register")]
        public async Task<IHttpActionResult> Register(RegisterModel model)
        {
            var userId = GetUserId();
            var existing = await _userManager.FindByIdAsync(userId);
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

            await _accountService.RegisterCompanyName(userId, model.Username, model.Email);

            return Success();
        }

        [HttpPost, Route("resolveCompanyId")]
        public async Task<IHttpActionResult> ResolveCompanyId(string companyName)
        {
            var actor = _actorFabric.GetProxy<ICompanyActor>(GetUserId());
            var companyId = await actor.ResolveExternalCompanyId(companyName);
            return Ok(new {CompanyId = companyId});
        }

        [HttpGet, Route("getCompanyName")]
        public async Task<IHttpActionResult> GetCompanyName()
        {
            var actor = _actorFabric.GetProxy<ICompanyActor>(GetUserId());
            var companyName = await actor.GetCompanyName();
            return Ok(new { CompanyName = companyName });
        }
    }
}