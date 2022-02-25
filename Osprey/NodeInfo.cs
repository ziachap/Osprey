using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Osprey
{
    [Serializable]
    public class NodeInfo
    {
        public NodeInfo()
        {
            Services = new List<ServiceInfo>();
        }

        [JsonProperty("id")]
        public string NodeId { get; set; }

        [JsonProperty("n")]
        public string Name { get; set; }

        [JsonProperty("e")]
        public string Environment { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("s")]
        public List<ServiceInfo> Services { get; } // TODO: Thread safety
    }
}