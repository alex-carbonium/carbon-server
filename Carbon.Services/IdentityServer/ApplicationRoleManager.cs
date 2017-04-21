using ElCamino.AspNet.Identity.AzureTable;
using ElCamino.AspNet.Identity.AzureTable.Model;
using Microsoft.AspNet.Identity;

namespace Carbon.Services.IdentityServer
{
    public class ApplicationRoleManager : RoleManager<IdentityRole>
    {
        public ApplicationRoleManager(ApplicationDbContext context)
            : base(new RoleStore<IdentityRole>(context))
        {
        }
    }
}