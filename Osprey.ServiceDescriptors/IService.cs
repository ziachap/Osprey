using System;

namespace Osprey.ServiceDescriptors
{

    public interface IService
    {
        string Name { get; }
    }

    public class Endpoint
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }
}
