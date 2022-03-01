namespace Osprey.ZeroMQ.Models
{
    public class EstablishRequest
    {
        public string ClientId { get; set; }

        public string Topic { get; set; }

        public string Message { get; set; }
    }
}