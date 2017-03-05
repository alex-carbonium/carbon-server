using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Carbon.Business;
using Carbon.Business.Domain;
using Carbon.Business.Services;
using Carbon.Owin.Common.Dependencies;
using Carbon.Owin.Common.WebApi;

namespace Carbon.Services.Controllers
{
    [RoutePrefix("admin")]
    [Authorize(Roles = Defs.Roles.Administrators)]
    public class AdminController : AuthorizedApiController
    {
        private readonly ProjectModelService _projectModelService;

        public AdminController(ProjectModelService projectModelService)
        {
            _projectModelService = projectModelService;
        }

        [Route("projectModel")]
        public async Task<HttpResponseMessage> GetProjectModel(string companyId, string modelId)
        {
            var change = new ProjectModelChange
            {
                CompanyId = companyId,
                ModelId = modelId,
                UserId = GetUserId(),
                PrimitiveStrings = new List<string>()
            };
            var model = await _projectModelService.ChangeProjectModel(Request.GetOwinContext().GetScopedContainer(), change);
            await model.EnsureLoaded();

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(model.Write(Formatting.Indented));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            return response;
        }
    }
}