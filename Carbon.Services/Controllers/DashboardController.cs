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
        private readonly ProjectModelService _projectModelService;

        public DashboardController(IActorFabric actorFabric, ProjectModelService projectModelService)
        {
            _actorFabric = actorFabric;
            _projectModelService = projectModelService;
        }

        [Route("")]
        public async Task<IHttpActionResult> Get(string companyId)
        {
            var dashboard = await GetActor(companyId).GetDashboard(GetUserId());
            return Ok(new { Folders = dashboard });
        }

        [Route("deleteProject"), HttpGet]
        public async Task<IHttpActionResult> DeleteProject(string companyId, string projectId)
        {
            await GetActor(companyId).DeleteProject(projectId);
            return await Get(companyId);
        }

        [HttpPost, Route("duplicateProject")]
        public async Task<IHttpActionResult> DuplicateProject(string companyId, string projectId)
        {
            await _projectModelService.Duplicate(GetUserId(), companyId, projectId);
            return await Get(companyId);
        }

        private ICompanyActor GetActor(string companyId)
        {
            return _actorFabric.GetProxy<ICompanyActor>(companyId);
        }
    }
}