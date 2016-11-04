using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Carbon.Framework;
using Carbon.Owin.Common.WebApi;

namespace Carbon.Services.Controllers
{
    public class ProxyController : BaseApiController
    {
        private static Lazy<HttpClient> _httpClient = new Lazy<HttpClient>(() => new HttpClient());
            
        [Route("proxy")]
        [HttpGet, HttpPost]
        public async Task<HttpResponseMessage> Invoke()
        {
            var url = Request.RequestUri.Query.Substring(1);
            var message = new HttpRequestMessage(Request.Method, url);
            if (Request.Method == HttpMethod.Post)
            {
                var content = await Request.Content.ReadAsStringAsync();
                message.Content = new StringContent(content, Defs.Encoding);
                message.Content.Headers.ContentType = Request.Content.Headers.ContentType;                
            }
            //iconfinder does not like some of the headers
            //foreach (var header in Request.Headers)
            //{
            //    message.Headers.Add(header.Key, header.Value);
            //}
            
            var response =  await _httpClient.Value.SendAsync(message);
            var proxied = new HttpResponseMessage(response.StatusCode);
            proxied.Content = response.Content;
            return proxied;
        }
    }
}