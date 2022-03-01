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
            var config = OSPREY.Network.Config.Network;

            if (config.Discover)
            {
                Receiver = new Receiver(_broadcastChannel);
                Receiver.Start();
            }
            if (config.Broadcast)
            {
                Broadcaster = new Broadcaster(_broadcastChannel, Info);
                Broadcaster.Start();
            }

            OSPREY.Network.Logger.Info($"Node started:");
            OSPREY.Network.Logger.Info($"  Id:".PadRight(12) + Info.NodeId);
            OSPREY.Network.Logger.Info($"  Service:".PadRight(12) + Info.Name);
            OSPREY.Network.Logger.Info($"  Local:".PadRight(12) + Info.Ip);
            OSPREY.Network.Logger.Info($"  Broadcast:".PadRight(12) + config.Broadcast);
            OSPREY.Network.Logger.Info($"  Discover:".PadRight(12) + config.Discover);
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