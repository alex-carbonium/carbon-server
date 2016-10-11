using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

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