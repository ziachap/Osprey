using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Osprey
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

    public class NodeInfo
    {
        public NodeInfo()
        {
            Services = new Dictionary<string, IService>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Ip { get; set; }
        public int UdpPort { get; set; }
        public string UdpAddress => Ip + ":" + UdpPort;
        public Dictionary<string, IService> Services { get; } // TODO: Thread safety
    }
}