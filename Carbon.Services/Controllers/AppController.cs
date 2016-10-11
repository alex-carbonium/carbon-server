using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace Carbon.Services.Controllers
{
    public class AppController : ApiController
    {
        private readonly ResourceCache _resourceCache;

        public AppController(ResourceCache resourceCache)
        {
            _resourceCache = resourceCache;
        }
        
        [HttpGet]
        public HttpResponseMessage Index()
        {
            return Response(@"target\index.html");
        }        

        private HttpResponseMessage Response(string file)
        {
            var content = _resourceCache.GetHtmlFile(file);            
            var response = new HttpResponseMessage();
            response.Content = new StringContent(content);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}