using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Carbon.Business.Services;
using Carbon.Owin.Common.WebApi;

namespace Carbon.Services.Controllers
{
    [RoutePrefix("dashboard")]
    public class DashboardController : AuthorizedApiController
    {
        private readonly IActorFabric _actorFabric;

        public DashboardController(IActorFabric actorFabric)
        {
            _actorFabric = actorFabric;
        }

        [Route("")]
        public async Task<IHttpActionResult> Get(string companyId)
        {
            var dashboard = await GetActor(companyId).GetDashboard(GetUserId());
            return Ok(new { Folders = dashboard });
        }

        private ICompanyActor GetActor(string companyId)
        {
            return _actorFabric.GetProxy<ICompanyActor>(companyId);
        }
    }
}