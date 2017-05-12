using Carbon.Business;
using ElCamino.AspNet.Identity.AzureTable;
using ElCamino.AspNet.Identity.AzureTable.Model;

namespace Carbon.Services.IdentityServer
{
    public class ApplicationDbContext : IdentityCloudContext
    {
        public ApplicationDbContext(AppSettings appSettings): base(
            new IdentityConfiguration
            {
                StorageConnectionString = appSettings.GetConnectionString("nosql")
            })
        {
        }

        public static async void StartupAsync(AppSettings appSettings)
        {
            var context = new ApplicationDbContext(appSettings);
            var userStore = new UserStore<ApplicationUser>(context);
            await userStore.CreateTablesIfNotExists();

            var roleStore = new RoleStore<IdentityRole>(context);
            await roleStore.CreateTableIfNotExistsAsync();

            var adminRole = await roleStore.FindByNameAsync(Defs.Roles.Administrators);
            if (adminRole == null)
            {
                try
                {
                    await roleStore.CreateAsync(new IdentityRole(Defs.Roles.Administrators));
                }
                catch
                {
                    //ignore
                }
            }
        }
    }
}