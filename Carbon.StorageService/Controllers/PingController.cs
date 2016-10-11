using System;
using System.Web.Http;
using Carbon.Business;
using Carbon.Framework.Extensions;
using Carbon.Owin.Common.WebApi;

namespace Carbon.StorageService.Controllers
{
    public class PingController : BaseApiController
    {
        private readonly AppSettings _appSettings;

        public PingController(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        [HttpGet, Route("ping")]
        public dynamic Get()
        {
            return new { ok = true, testSecret = _appSettings.TestSecret.ToPlainText() };
        }

        [HttpGet, Route("exception")]
        public dynamic Exception()
        {
            throw new Exception("Test exception");
        }
    }
}