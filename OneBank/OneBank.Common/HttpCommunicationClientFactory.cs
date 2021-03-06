﻿using Microsoft.ServiceFabric.Services.Communication.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneBank.Common
{
    [Serializable]
    public class HttpCommunicationClientFactory : CommunicationClientFactoryBase<HttpCommunicationClient>, IHttpCommunicationClientFactory
    {
        private readonly HttpClient httpClient;
        public HttpCommunicationClientFactory(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        protected override void AbortClient(HttpCommunicationClient client)
        {
        }

        protected override Task<HttpCommunicationClient> CreateClientAsync(string endpoint, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpCommunicationClient(this.httpClient));
        }

        protected override bool ValidateClient(HttpCommunicationClient client)
        {
            return true;
        }

        protected override bool ValidateClient(string endpoint, HttpCommunicationClient client)
        {
            return true;
        }
    }
}