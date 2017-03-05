using System.Security.Principal;

namespace Carbon.Business.Domain
{
    public interface IIdentityContext
    {
        IPrincipal Principal { get; set; }
        string GetUserId();
        string GetUserEmail();
        string SessionId { get; set; }
    }
}
