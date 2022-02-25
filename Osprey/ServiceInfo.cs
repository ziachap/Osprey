using System;
using Newtonsoft.Json;

namespace Osprey
{
    [Serializable]
    public class ServiceInfo
    {
        [JsonProperty("t")]
        public string Type { get; set; }

        [JsonProperty("n")]
        public string Name { get; set; }

        [JsonProperty("a")]
        public string Address { get; set; }
    }
}
