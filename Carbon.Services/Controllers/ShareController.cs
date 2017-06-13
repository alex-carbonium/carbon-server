﻿using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Carbon.Business.Domain;
using Carbon.Business.Services;
using Carbon.Owin.Common.WebApi;
using Carbon.Business.Exceptions;

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
            var token = await _sharingService.Invite(GetUserId(), companyId, projectId, permission);
            return Ok(new { Code = token.RowKey });
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

        [HttpPost, Route("invite")]
        public async Task<IHttpActionResult> Invite(string companyId, string projectId, string email, int role)
        {
            Permission permission;
            if (!_roleMap.TryGetValue(role, out permission))
            {
                return BadRequest();
            }
            await _sharingService.Invite(GetUserId(), companyId, projectId, permission, email);
            return Ok();
        }

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
            public PublishScope Scope { get; set; }
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
        public async Task<IHttpActionResult> AvailableResource(string search)
        {
            var userId = GetUserId();

            var myResources = _sharingService.SearchResources(userId, search, PublishScope.Company);
            var publicResources = _sharingService.SearchResources(userId, search, PublishScope.Public);
            return Ok(new
            {
                myResources = await myResources,
                publicResources = await publicResources
            });
        }

        public class PublishPageModel
        {
            public string Name { get; set;  }
            public string Description { get; set; }
            public string Tags { get; set; }
            public string PageData { get; set; }
            public string CoverUrl { get; set; }
            public PublishScope Scope { get; set; }
        }
    }
}