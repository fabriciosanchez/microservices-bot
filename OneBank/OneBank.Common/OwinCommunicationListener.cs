using Microsoft.Owin.Hosting;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Owin;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneBank.Common
{
    public class OwinCommunicationListener : ICommunicationListener
    {
        private readonly string endpointName;
        private readonly ServiceContext serviceContext;
        private readonly Action<IAppBuilder> startup;
        private string listeningAddress;
        private string publishAddress;
        private IDisposable webAppHandle;

        public OwinCommunicationListener(Action<IAppBuilder> startup, ServiceContext serviceContext, string endpointName)
        {
            this.startup = startup ?? throw new ArgumentNullException(nameof(startup));
            this.serviceContext = serviceContext ?? throw new ArgumentNullException(nameof(serviceContext));
            this.endpointName = endpointName ?? throw new ArgumentNullException(nameof(endpointName));
        }

        public void Abort()
        {
            this.StopHosting();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            this.StopHosting();
            return Task.FromResult(true);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            string ipAddress = FabricRuntime.GetNodeContext().IPAddressOrFQDN;
            var serviceEndpoint = this.serviceContext.CodePackageActivationContext.GetEndpoint(this.endpointName);
            var protocol = serviceEndpoint.Protocol;
            int port = serviceEndpoint.Port;

            if (this.serviceContext is StatefulServiceContext)
            {
                StatefulServiceContext statefulServiceContext = this.serviceContext as StatefulServiceContext;
                this.listeningAddress = $"{protocol}://+:{port}/{statefulServiceContext.PartitionId}/{statefulServiceContext.ReplicaId}/{Guid.NewGuid()}";
            }
            else if(this.serviceContext is StatelessServiceContext)
            {
                this.listeningAddress = $"{protocol}://+:{port}";
            }
            else
            {
                throw new InvalidOperationException();
            }

            this.publishAddress = this.listeningAddress.Replace("+", ipAddress);

            try
            {
                this.webAppHandle = WebApp.Start(this.listeningAddress, appBuilder => this.startup.Invoke(appBuilder));
                Task.FromResult(this.publishAddress);
            }
            catch (Exception)
            {
                this.StopHosting();
                throw;
            }
        }

        private void StopHosting()
        {
            if(this.webAppHandle != null)
            {
                try
                {
                    this.webAppHandle.Dispose();
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }
    }
}
