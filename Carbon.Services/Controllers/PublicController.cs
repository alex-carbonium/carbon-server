using Carbon.Business.Services;
using Carbon.Owin.Common.WebApi;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Carbon.Services.Controllers
{
    [RoutePrefix("beta")]
    public class PublicController : BaseApiController
    {
        private readonly ActivityService _activityService;

        public PublicController(ActivityService activityService)
        {
            _activityService = activityService;
        }

        [HttpPost, Route("subscribe")]
        public IHttpActionResult SubscribeForBeta(string email)
        {
            _activityService.SubscribeForBeta(email);
            return Ok();
        }
        
    }
}
