using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Carbon.Business.Services;
using Carbon.Owin.Common.WebApi;
using Carbon.Services.IdentityServer;

namespace Carbon.Services.Controllers
{
    [RoutePrefix("activity")]
    public class ActivityController : AuthorizedApiController
    {
        private readonly IActorFabric _actorFabric;
        private readonly ActivityService _activityService;

        public ActivityController(IActorFabric actorFabric, ActivityService activityService)
        {
            _actorFabric = actorFabric;
            _activityService = activityService;
        }

        [HttpPost, Route("subscribeForFeature")]
        public IHttpActionResult SubscribeForFeature(string companyId, string projectId, string feature)
        {
            this._activityService.SubscribeForFeature(companyId, projectId, feature);
            return Success();
        }
    }
}
