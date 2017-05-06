using ElCamino.AspNet.Identity.AzureTable.Model;

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