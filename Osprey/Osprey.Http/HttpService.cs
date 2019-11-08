using System.Collections.Generic;

namespace Osprey.Http
{
    public class HttpService : IService
    {
        public HttpService(string name, string url)
        {
            Name = name;
            Url = url;
            Endpoints = new List<Endpoint>();
        }

        public string Name { get; }
        public string Url { get; }
        public List<Endpoint> Endpoints { get; }

        public void RegisterEndpoint(string endpoint)
        {
            Endpoints.Add(new Endpoint
            {
                Name = endpoint,
                Address = Url + "/" + endpoint
            });
        }
    }
}