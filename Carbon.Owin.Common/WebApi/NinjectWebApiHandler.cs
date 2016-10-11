using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Hosting;
using Carbon.Owin.Common.Dependencies;

namespace Carbon.Owin.Common.WebApi
{
    public class NinjectWebApiHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var container = request.GetOwinContext().GetScopedContainer();
            request.Properties[HttpPropertyKeys.DependencyScope] = new NinjectWebApiScope(container);

            return base.SendAsync(request, cancellationToken);
        }
    }
}