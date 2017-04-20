using System;
using System.Fabric;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Owin;
using Carbon.Fabric.Common.Logging;
using Carbon.Framework.Util;

namespace Carbon.Fabric.Common
{
    public class BaseCommunicationListener : ICommunicationListener
    {
        private readonly Action<IAppBuilder> _startup;
        private readonly ServiceContext _serviceContext;
        private readonly string _endpointName;
        private readonly string _appRoot;

        private IDisposable _webApp;
        private string _publishAddress;
        private string _listeningAddress;
        private IDependencyContainer _scope;

        public BaseCommunicationListener(Action<IAppBuilder> startup, ServiceContext serviceContext, string endpointName, IDependencyContainer scope)
            : this(startup, serviceContext, endpointName, null, scope)
        {
        }

        public BaseCommunicationListener(Action<IAppBuilder> startup, ServiceContext serviceContext, string endpointName, string appRoot, IDependencyContainer scope)
        {
            if (startup == null)
            {
                throw new ArgumentNullException(nameof(startup));
            }

            if (serviceContext == null)
            {
                throw new ArgumentNullException(nameof(serviceContext));
            }

            if (endpointName == null)
            {
                throw new ArgumentNullException(nameof(endpointName));
            }

            this._startup = startup;
            this._serviceContext = serviceContext;
            this._endpointName = endpointName;
            this._appRoot = appRoot;
            _scope = scope;
        }

        public bool ListenOnSecondary { get; set; }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var serviceEndpoint = this._serviceContext.CodePackageActivationContext.GetEndpoint(this._endpointName);
            int port = serviceEndpoint.Port;

            if (this._serviceContext is StatefulServiceContext)
            {
                StatefulServiceContext statefulServiceContext = this._serviceContext as StatefulServiceContext;

                this._listeningAddress = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}://+:{1}/{2}{3}/{4}/{5}",
                    serviceEndpoint.Protocol.ToString().ToLower(),
                    port,
                    string.IsNullOrWhiteSpace(this._appRoot)
                        ? string.Empty
                        : this._appRoot.TrimEnd('/') + '/',
                    statefulServiceContext.PartitionId,
                    statefulServiceContext.ReplicaId,
                    Guid.NewGuid());
            }
            else if (this._serviceContext is StatelessServiceContext)
            {
                this._listeningAddress = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}://+:{1}/{2}",
                    serviceEndpoint.Protocol.ToString().ToLower(),
                    port,
                    string.IsNullOrWhiteSpace(this._appRoot)
                        ? string.Empty
                        : this._appRoot.TrimEnd('/') + '/');
            }
            else
            {
                throw new InvalidOperationException();
            }

            this._publishAddress = this._listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);

            try
            {
                CommonEventSource.Current.Info("Starting web server on " + this._listeningAddress, _scope);

                this._webApp = WebApp.Start(this._listeningAddress, appBuilder => this._startup.Invoke(appBuilder));

                CommonEventSource.Current.Info("Listening on " + this._publishAddress, _scope);

                return Task.FromResult(this._publishAddress);
            }
            catch (Exception ex)
            {
                CommonEventSource.Current.Fatal(ex, _scope);

                this.StopWebServer();

                throw;
            }
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            CommonEventSource.Current.Info("Closing web server", _scope);

            this.StopWebServer();

            return Task.FromResult(true);
        }

        public void Abort()
        {
            CommonEventSource.Current.Info("Aborting web server", _scope);

            this.StopWebServer();
        }

        private void StopWebServer()
        {
            if (this._webApp != null)
            {
                try
                {
                    this._webApp.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // no-op
                }
            }
        }
    }
}
