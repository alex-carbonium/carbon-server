using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using Carbon.Business;
using Carbon.Framework.Logging;
using Carbon.Framework.UnitOfWork;
using Carbon.Framework.Util;
using Carbon.Owin.Common.Dependencies;

namespace Carbon.Owin.Common.WebApi
{
    public abstract class BaseApiController : ApiController
    {
        public ILogService LogService => Scope.Resolve<ILogService>();
        public AppSettings AppSettings => Scope.Resolve<AppSettings>();
        public OperationContext Operation => Scope.Resolve<OperationContext>();
        public IDependencyContainer Scope => Request.GetOwinContext().GetScopedContainer();

        protected HttpResponseMessage File(Stream stream)
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return result;
        }

        protected HttpResponseMessage ZippedContent(byte[] content, string fileName)
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(content);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName,
            };
            return result;
        }

        protected HttpResponseMessage Html(string data)
        {
            var content = new StringContent(data);
            content.Headers.ContentType = new MediaTypeHeaderValue("text/html")
            {
                CharSet = Encoding.UTF8.WebName
            };

            return new HttpResponseMessage()
            {
                Content = content
            };
        }

        public IHttpActionResult Forbidden()
        {
            return StatusCode(HttpStatusCode.Forbidden);
        }

        public IHttpActionResult Success()
        {
            return Ok(ActionResponse.Success);
        }

        public IHttpActionResult Error(string key, string message)
        {
            return Content(HttpStatusCodeExt.UnprocessableEntity,
                new ActionResponse(false, new Dictionary<string, string> { { key, message } }));
        }

        public IHttpActionResult Error(string key, IEnumerable<string> errors)
        {
            return Content(HttpStatusCodeExt.UnprocessableEntity,
                new ActionResponse(false, new Dictionary<string, string> { { key, string.Join("\n", errors) } }));
        }

        public IHttpActionResult Error(IDictionary<string, string> errors)
        {
            return Content(HttpStatusCodeExt.UnprocessableEntity, new ActionResponse(false, errors));
        }
    }
}