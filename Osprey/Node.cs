using System;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using Osprey.Communication;
using Osprey.ServiceDiscovery;
using Osprey.Utilities;

[assembly: InternalsVisibleTo("Osprey.Monitor")]

namespace Osprey
{
    public class Node : IDisposable
    {
        public NodeInfo Info { get; private set; }
        internal Receiver Receiver { get; private set; }
        internal Broadcaster Broadcaster { get; private set; }

        private readonly UdpChannel _broadcastChannel;

        internal Node(string id, string name, string environment)
        {
            var port = OSPREY.Network.Config.Network.UdpBroadcastPort;
            var local = Address.GetLocalIpAddress();
            var remote = IPAddress.Parse(OSPREY.Network.Config.Network.UdpBroadcastRemote);

            Info = new NodeInfo
            {
                NodeId = id,
                Name = name,
                Environment = environment,
                Ip = local.ToString(),
            };

            _broadcastChannel = new UdpChannel(remote, local, port);
        }

        internal void Start()
        {
            Receiver = new Receiver(_broadcastChannel);
            Receiver.Start();
            Broadcaster = new Broadcaster(_broadcastChannel, Info);
            Broadcaster.Start();

            Console.WriteLine($"Node started:");
            Console.WriteLine($"  Id:".PadRight(12) + Info.NodeId);
            Console.WriteLine($"  Service:".PadRight(12) + Info.Name);
            Console.WriteLine($"  Local:".PadRight(12) + Info.Ip);
        }

        public void Register(ServiceInfo service)
        {
            if (Info.Services.Any(x => x.Name == service.Name))
                throw new Exception("Already registered: " + service.Name);
            Info.Services.Add(service);
        }

        public void Dispose()
        {
            _broadcastChannel?.Dispose();
        }
    }
}