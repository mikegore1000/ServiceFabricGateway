using System;
using System.Fabric;
using System.Net.Http;
using Microsoft.ServiceFabric.Services.Communication.Client;

namespace Gateway
{
    public class HttpCommunicationClient : ICommunicationClient
    {
        public HttpCommunicationClient(Uri endpoint, HttpClient httpClient)
        {
            Url = endpoint;
            HttpClient = httpClient;
        }

        public Uri Url { get; private set; }

        public HttpClient HttpClient { get; private set; }
       
        public ResolvedServicePartition ResolvedServicePartition { get; set; }

        public string ListenerName { get; set; }

        public ResolvedServiceEndpoint Endpoint { get; set; }
    }
}