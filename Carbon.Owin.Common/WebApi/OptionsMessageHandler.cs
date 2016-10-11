using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Carbon.Owin.Common.WebApi
{
    public class OptionsMessageHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Options)
            {
                return Task.FromResult(new HttpResponseMessage {StatusCode = HttpStatusCode.OK});
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}