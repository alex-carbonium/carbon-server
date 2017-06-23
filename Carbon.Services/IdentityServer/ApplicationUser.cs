using ElCamino.AspNet.Identity.AzureTable.Model;

namespace Carbon.Services.IdentityServer
{
    public class ApplicationUser : IdentityUser
    {
        public string Avatar { get; set; }

        public override string PeekRowKey()
        {
            return Id;
        }
    }
}