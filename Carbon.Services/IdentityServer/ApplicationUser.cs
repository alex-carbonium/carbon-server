using ElCamino.AspNet.Identity.AzureTable.Model;
using Microsoft.AspNet.Identity;

namespace Carbon.Services.IdentityServer
{
    public class ApplicationUser : IdentityUser
    {
        public override string PeekRowKey()
        {
            return Id;
        }
    }
}