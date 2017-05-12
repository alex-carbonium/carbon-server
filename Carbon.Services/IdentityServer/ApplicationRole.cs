using ElCamino.AspNet.Identity.AzureTable.Model;

namespace Carbon.Services.IdentityServer
{

    public class ApplicationRole : IdentityRole
    {
        public override string PeekRowKey()
        {
            return Name;
        }
    }
}