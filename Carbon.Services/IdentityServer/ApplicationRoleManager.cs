using ElCamino.AspNet.Identity.AzureTable;
using Microsoft.AspNet.Identity;

namespace Carbon.Services.IdentityServer
{
    public class ApplicationRoleManager : RoleManager<ApplicationRole>
    {
        public ApplicationRoleManager(ApplicationDbContext context)
            : base(new RoleStore<ApplicationRole>(context))
        {
        }
    }
}