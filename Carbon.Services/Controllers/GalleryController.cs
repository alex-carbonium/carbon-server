﻿using Carbon.Business.Services;
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
                pageData = resources.Skip(from).Take(to - from).ToList(),
                totalCount = resources.Count()
            });
        }
    }
}
