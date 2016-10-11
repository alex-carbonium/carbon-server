using System.Web.Http;
using Carbon.Business.Domain;

namespace Carbon.Owin.Common.WebApi
{
    [Authorize]
    public class AuthorizedApiController : BaseApiController
    {
        public IIdentityContext IdentityContext => Scope.Resolve<IIdentityContext>();

        protected string GetUserId()
        {
            return IdentityContext.GetUserId();
        }
    }
}