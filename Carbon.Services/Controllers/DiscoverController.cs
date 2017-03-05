using System.Threading.Tasks;
using System.Web.Http;
using Carbon.Business.Services;
using Carbon.Owin.Common.WebApi;

namespace Carbon.Services.Controllers
{
    [RoutePrefix("discover")]
    public class DiscoverController : AuthorizedApiController
    {
        private readonly ActiveProjectTrackingService _trackingService;

        public DiscoverController(ActiveProjectTrackingService discoveryService)
        {
            _trackingService = discoveryService;
        }

        [HttpGet]
        [Route("projectHub")]
        public async Task<IHttpActionResult> ProjectHub(string companyId, string projectId)
        {
            var port = await _trackingService.GetConnectionPort(GetUserId(), companyId, projectId);
            return Ok(new { Url = "//" + Request.RequestUri.Host + ":" + port });
        }
    }
}
