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
    }
}