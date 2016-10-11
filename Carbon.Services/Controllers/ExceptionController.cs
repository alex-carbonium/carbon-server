using System;
using System.Net.Http;
using System.Web.Http;
using Carbon.Business.Exceptions;
using Carbon.Owin.Common.WebApi;

namespace Carbon.Services.Controllers
{
    [RoutePrefix("exception")]
    public class ExceptionController : BaseApiController
    {
        [HttpGet]
        public HttpResponseMessage TestException()
        {
            throw new Exception("Test exception");
        }
        [HttpGet]
        public HttpResponseMessage TestKnownException()
        {
            throw new CompanyLockedException();
        }
    }
}