using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
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

        internal Node(string id, string name, IPAddress ip)
        {
            Info = new NodeInfo
			{
				Id = id,
				Name = name,
				Ip = ip.ToString(),
                UdpPort = Address.GetUdpPort(),
			};
            _broadcastChannel = new UdpChannel(new IPEndPoint(IPAddress.Loopback, 12345));
        }

        internal void Start()
        {
            Receiver = new Receiver(_broadcastChannel);
            Receiver.Start();
	        Broadcaster = new Broadcaster(_broadcastChannel, Info);
	        Broadcaster.Start();

            Console.WriteLine($"Node started:");
            Console.WriteLine($"  Id:       {Info.Id}");
            Console.WriteLine($"  Service:  {Info.Name}");
            Console.WriteLine($"  UDP Broadcaster:      {Info.UdpAddress}");
        }

        public void Register(IService host)
        {
            if(Info.Services.ContainsKey(host.Name)) throw new Exception("Already registered: " + host.Name);
            Info.Services[host.Name] = host;
        }

        public void Dispose()
        {
            _broadcastChannel?.Dispose();
        }
    }
}