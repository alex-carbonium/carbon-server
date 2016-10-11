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

        [HttpPost, Route("register")]
        public async Task<IHttpActionResult> Register(RegisterModel model)
        {
            var user = new ApplicationUser
            {
                Id = GetUserId(),
                Email = model.Email,
                UserName = model.Username
            };

            var existing = await _userManager.FindByIdAsync(user.Id);
            if (existing == null)
            {
                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    return Error(result.Errors);
                }
            }            

            var companyName = await _accountService.RegisterCompanyName(model.Username, model.Email);
            var actor = _actorFabric.GetProxy<ICompanyActor>(GetUserId());
            await actor.ChangeCompanyName(companyName);            

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