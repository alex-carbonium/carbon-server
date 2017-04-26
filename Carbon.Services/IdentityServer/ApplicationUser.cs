using ElCamino.AspNet.Identity.AzureTable.Model;

namespace Carbon.Services.IdentityServer
{
    public class ApplicationUser : IdentityUser
    {
        public override string PeekRowKey()
        {
            return Id;
        }

        public bool HasEmail()
        {
            if (string.IsNullOrEmpty(Email))
            {
                return false;
            }
            if (Email.StartsWith("guest") && Email.EndsWith("@carbonium.io"))
            {
                return false;
            }
            return true;
        }

        public static string MakeInternalEmail(string userId)
        {
            return "guest" + userId + "@carbonium.io";
        }
    }
}