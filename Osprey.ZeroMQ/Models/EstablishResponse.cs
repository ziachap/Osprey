namespace Osprey.ZeroMQ.Models
{
    public class EstablishResponse
    {
        public string ClientId { get; set; }
        public string StreamEndpoint { get; set; }
        public string HeartbeatEndpoint { get; set; }
        public string ResponseEndpoint { get; set; }
    }
}