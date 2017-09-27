using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Carbon.Business.Domain;
using Carbon.Business.Services;
using Carbon.Owin.Common.WebApi;
using Carbon.Business.Exceptions;
using System.Linq;

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
        private static readonly Dictionary<Permission, int> _permissionMap = new Dictionary<Permission, int>();

        private readonly SharingService _sharingService;

        static ShareController()
        {
            foreach (var kv in _roleMap)
            {
                _permissionMap.Add(kv.Value, kv.Key);
            }
        }

        public ShareController(SharingService sharingService)
        {
            _sharingService = sharingService;
        }

        [HttpGet, Route("codes")]
        public async Task<IHttpActionResult> GetShareCodes(string companyId, string projectId)
        {
            var codes = await _sharingService.GetShareCodes(GetUserId(), companyId, projectId);
            return Ok(new
            {
                Codes = codes.Select(x => new
                {
                    Code = x.Id,
                    Role = _permissionMap[(Permission)x.Permission]
                })
            });
        }

        [HttpDelete, Route("codes")]
        public async Task<IHttpActionResult> DeleteShareCodes(string companyId, string projectId)
        {
            await _sharingService.RemoveShareCodes(GetUserId(), companyId, projectId);
            return Success();
        }

        [HttpPost, Route("code")]
        public async Task<IHttpActionResult> AddShareCode(string companyId, string projectId, int role)
        {
            Permission permission;
            if (!_roleMap.TryGetValue(role, out permission))
            {
                return BadRequest();
            }
            var token = await _sharingService.AddShareCode(GetUserId(), companyId, projectId, permission);
            return Ok(new { Code = token.RowKey });
        }

        [HttpDelete, Route("code")]
        public async Task<IHttpActionResult> DeleteShareCode(string companyId, string projectId, string code)
        {
            await _sharingService.RemoveShareCode(GetUserId(), companyId, projectId, code);
            return Success();
        }

        [HttpGet, Route("mirrorCode")]
        public async Task<IHttpActionResult> GetMirroringCode(string companyId, string projectId, bool enable)
        {
            var token = await _sharingService.GenerateMirroringToken(GetUserId(), companyId, projectId, enable);
            return Ok(new { Code = token });
        }

        [HttpPost, Route("disableMirroring")]
        public async Task<IHttpActionResult> DisableMirroring(string companyId, string projectId)
        {
            await _sharingService.DisableMirroring(GetUserId(), companyId, projectId);
            return Ok(new { });
        }

        //[HttpPost, Route("invite")]
        //public async Task<IHttpActionResult> Invite(string companyId, string projectId, string email, int role)
        //{
        //    Permission permission;
        //    if (!_roleMap.TryGetValue(role, out permission))
        //    {
        //        return BadRequest();
        //    }
        //    await _sharingService.AddShareCode(GetUserId(), companyId, projectId, permission, email);
        //    return Ok();
        //}

        [HttpPost, Route("use")]
        public async Task<IHttpActionResult> Use(string code)
        {
            var result = await _sharingService.UseCode(GetUserId(), code);
            if (result == null)
            {
                return Forbidden();
            }

            var acl = result.Item1;
            var userId = result.Item2;

            return Ok(new { acl.CompanyName, ProjectId = acl.Entry.ResourceId, UserId = userId, CompanyId = acl.Entry.Sid });
        }

        [HttpGet, Route("pageSetup")]
        public async Task<IHttpActionResult> GetPageSetup(string pageId)
        {
            var userId = GetUserId();
            var setup = await _sharingService.GetPageSetup(userId, pageId);

            return Ok(setup);
        }

        public class ValidatePageModel
        {
            public string Name { get; set; }
            public ResourceScope Scope { get; set; }
        }
        [HttpPost, Route("validatePageName")]
        public async Task<IHttpActionResult> ValidatePageName(ValidatePageModel model)
        {
            var userId = GetUserId();
            var status = await _sharingService.ValidatePageName(model.Scope, userId, model.Name);

            if (status == ResourceNameStatus.Taken)
            {
                return Error(nameof(model.Name), "@publish.nameTaken");
            }
            return Ok(new { ok = true, result = new { exists = status == ResourceNameStatus.CanOverride } });
        }

        [HttpPost, Route("publishPage")]
        public async Task<IHttpActionResult> PublishPage(PublishPageModel model)
        {
            var userId = GetUserId();

            try
            {
                var page = await _sharingService.PublishPage(userId, model.Name, model.Description, model.Tags, model.PageData, model.CoverUrl, model.Scope);
                return Ok(new { ok = true, result = page });
            }
            catch (InsertConflictException)
            {
                return Error(nameof(model.Name), "@nameTaken");
            }
        }

        [HttpGet, Route("resources")]
        public async Task<IHttpActionResult> Resources(int from, int to, string search)
        {
            var userId = GetUserId();
            var resources = await _sharingService.SearchCompanyResources(userId, search);
            return Ok(new
            {
                pageData = resources.Skip(from).Take(to - from).ToList(),
                totalCount = resources.Count()
            });
        }

        public class PublishPageModel
        {
            public string Name { get; set;  }
            public string Description { get; set; }
            public string Tags { get; set; }
            public string PageData { get; set; }
            public string CoverUrl { get; set; }
            public ResourceScope Scope { get; set; }
        }
    }
}