using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Carbon.Business.Services;
using Carbon.Owin.Common.WebApi;
using Carbon.Services.IdentityServer;
using IdentityServer3.Core;
using Microsoft.Owin.Security;

namespace Carbon.Services.Controllers
{
    public class IdentityServerExtensionsController : AuthorizedApiController
    {
        private readonly IActorFabric _actorFabric;

        public IdentityServerExtensionsController(IActorFabric actorFabric)
        {
            _actorFabric = actorFabric;
        }

        [HttpGet]
        public async Task<IHttpActionResult> UserId()
        {
            var userId = GetUserId();
            //TODO: security issue, cookie should be issued only after successful login
            IssueAuthenticationCookie(userId);

            var actor = _actorFabric.GetProxy<ICompanyActor>(userId);
            var companyInfo = await actor.GetCompanyInfo();
            var companyName = companyInfo.Name;
            

            return Ok(new { UserId = userId, CompanyName = companyName, companyInfo.Logo, userName = companyInfo.Owner.Name, avatar = companyInfo.Owner.Avatar });
        }

        [HttpPost, AllowAnonymous]
        public IHttpActionResult Logout()
        {
            Request.GetOwinContext().Authentication.SignOut(Constants.PrimaryAuthenticationType);
            return Success();
        }

        private void IssueAuthenticationCookie(string userId)
        {
            var options = IdentityServerConfig.Options;
            var props = new AuthenticationProperties();

            var expires = DateTime.UtcNow.Add(options.AuthenticationOptions.CookieOptions.RememberMeDuration);
            props.ExpiresUtc = expires;

            var user = IdentityServerPrincipal.Create(userId, userId);
            var identity = user.Identities.First();

            Request.GetOwinContext().Authentication.SignIn(props, identity);
        }
    }
}
