using Carbon.Business.Services;
using Carbon.Owin.Common.WebApi;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Carbon.Services.Controllers
{
    [RoutePrefix("gallery")]
    public class GalleryController : BaseApiController
    {
        private readonly SharingService _sharingService;

        public GalleryController(SharingService sharingService)
        {
            _sharingService = sharingService;
        }

        [HttpGet, Route("resources")]
        public async Task<IHttpActionResult> Resources(int from, int to, string search)
        {
            var resources = await _sharingService.SearchPublicResources(search);
            return Ok(new
            {
                pageData = resources.Skip(from).Take(to - from + 1).ToList(),
                totalCount = resources.Count()
            });
        }

        [HttpGet, Route("resource")]
        public async Task<IHttpActionResult> Resource(string id)
        {
            var resource = await _sharingService.SearchPublicResourceById(id);

            if (resource != null)
            {
                return Ok(resource);
            }

            return NotFound();
        }

        [HttpPost, Route("trackPublicResourceUsed")]
        public async Task<IHttpActionResult> TrackPublicResourceUsed(string resourceId)
        {
            await _sharingService.TrackPublicPageUsed(resourceId);
            return Success();
        }

        [HttpPost, Route("trackPrivateResourceUsed")]
        public async Task<IHttpActionResult> TrackPrivateResourceUsed(string companyId, string resourceId)
        {
            await _sharingService.TrackPrivatePageUsed(companyId, resourceId);
            return Success();
        }
    }
}
