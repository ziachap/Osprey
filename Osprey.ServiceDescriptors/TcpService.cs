namespace Osprey.ServiceDescriptors
{
    public class TcpService : IService
    {
        public TcpService(Endpoint endpoint)
        {
            Endpoint = endpoint;
        }

        public Endpoint Endpoint { get; }
        public string Name => Endpoint.Name;
    }
}