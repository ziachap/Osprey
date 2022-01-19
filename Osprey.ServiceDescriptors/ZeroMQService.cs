namespace Osprey.ServiceDescriptors
{
    public class ZeroMQService : IService
    {
        public ZeroMQService(Endpoint endpoint)
        {
            Endpoint = endpoint;
        }

        public Endpoint Endpoint { get; }
        public string Name => Endpoint.Name;
    }
}