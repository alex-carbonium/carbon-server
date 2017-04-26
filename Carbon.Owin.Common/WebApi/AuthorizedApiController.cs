using System.Web.Http;

namespace Carbon.Owin.Common.WebApi
{
    [Authorize]
    public class AuthorizedApiController : BaseApiController
    {
        protected string GetUserId()
        {
            return Operation.UserId;
        }
    }
}