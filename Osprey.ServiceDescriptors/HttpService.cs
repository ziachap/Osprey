using System.Collections.Generic;

namespace Osprey.ServiceDescriptors
{
    public class HttpService : IService
    {
        public HttpService(string name, string url)
        {
            Name = name;
            Url = url;
        }

        public string Name { get; }
        public string Url { get; }
    }
}