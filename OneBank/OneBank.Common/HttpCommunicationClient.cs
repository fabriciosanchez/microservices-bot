﻿using Microsoft.ServiceFabric.Services.Communication.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OneBank.Common
{
    public class HttpCommunicationClient : ICommunicationClient
    {
        public HttpCommunicationClient(HttpClient httpClient)
        {
            this.HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; private set; }

        public ResolvedServicePartition ResolvedServicePartition { get; set; }

        public string ListenerName { get; set; }

        public ResolvedServiceEndpoint Endpoint { get; set; }

        public string HttpEndPoint
        {
            get
            {
                JObject addresses = JObject.Parse(this.Endpoint.Address);
                return (string)addresses["Endpoints"].First();
            }
        }
    }
}