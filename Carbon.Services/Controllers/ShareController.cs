using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Carbon.Business.Domain;
using Carbon.Business.Services;
using Carbon.Owin.Common.WebApi;

namespace Carbon.Services.Controllers
{
    [RoutePrefix("share")]
    public class ShareController : AuthorizedApiController
    {
        private static readonly Dictionary<int, Permission> _roleMap = new Dictionary<int, Permission>
        {
            { 0, PredefinedRoles.ViewOnly },
            { 1, PredefinedRoles.Comment },
            { 2, PredefinedRoles.Edit }
        };

        private readonly SharingService _sharingService;

        public ShareController(SharingService sharingService)
        {
            _sharingService = sharingService;
        }

        [HttpGet, Route("code")]
        public async Task<IHttpActionResult> GetShareCode(string companyId, string projectId, int role)
        {
            Permission permission;
            if (!_roleMap.TryGetValue(role, out permission))
            {
                return BadRequest();
            }
            var token = await _sharingService.Invite(companyId, projectId, permission);
            return Ok(new { Code = token.RowKey });
        }

        [HttpGet, Route("mirrorCode")]
        public async Task<IHttpActionResult> GetMirroringCode(string companyId, string projectId, bool enable)
        {
            var token = await _sharingService.GenerateMirroringToken(companyId, projectId, enable);
            return Ok(new { Code = token });
        }

        [HttpPost, Route("disableMirroring")]
        public async Task<IHttpActionResult> DisableMirroring(string companyId, string projectId)
        {
            await _sharingService.DisableMirroring(companyId, projectId);
            return Ok(new { });
        }

        [HttpPost, Route("invite")]
        public async Task<IHttpActionResult> Invite(string companyId, string projectId, string email, int role)
        {
            Permission permission;
            if (!_roleMap.TryGetValue(role, out permission))
            {
                return BadRequest();
            }
            await _sharingService.Invite(companyId, projectId, permission, email);
            return Ok();
        }

        [HttpPost, Route("use")]
        public async Task<IHttpActionResult> Use(string code)
        {
            var acl = await _sharingService.UseCode(GetUserId(), code);
            if (acl == null)
            {
                return Forbidden();
            }

            return Ok(new { acl.CompanyName, ProjectId = acl.Entry.ResourceId, CompanyId = acl.Entry.Sid });
        }

        [HttpPost, Route("publishPage")]

        public async Task<IHttpActionResult> PublishPage(string name, string description, string tags, string pageData, string previewPicture, bool isPublic)
        {
            var userId = GetUserId();
            
            var page = await _sharingService.PublishPage(userId, name, description, tags, pageData, previewPicture, isPublic ? PublishScope.Public : PublishScope.Company);
            return Ok(new {data=page});
        }

        [HttpGet, Route("resources")]

        public async Task<IHttpActionResult> AvailiableResource(string search)
        {
            var userId = GetUserId();

            var myResources = _sharingService.SearchResources(userId, search, PublishScope.Company);
            var publicResources = _sharingService.SearchResources(userId, search, PublishScope.Public);
            return Ok(
            new {
                myResources = await myResources,
                publicResources = await publicResources
            });
        }
    }
}