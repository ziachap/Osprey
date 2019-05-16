using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Osprey.Communication;

namespace Osprey
{
    public class NodeInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Ip { get; set; }
        public int UdpPort { get; set; }
        public int TcpPort { get; set; }

        public string UdpAddress => Ip + ":" + UdpPort;
        public string TcpAddress => Ip + ":" + TcpPort;

    }

    public class Node : IDisposable
    {
        public NodeInfo Info { get; private set; }
        public Receiver Receiver { get; private set; }
        public Broadcaster Broadcaster { get; private set; }
        public Dictionary<string, IHandler> Endpoints { get; private set; }
		
        private readonly UdpChannel _broadcastChannel;

        public Node(string id, string name, IPAddress ip)
        {
            Endpoints = new Dictionary<string, IHandler>();

            Info = new NodeInfo
			{
				Id = id,
				Name = name,
				Ip = ip.ToString(),
			};
            _broadcastChannel = new UdpChannel(new IPEndPoint(IPAddress.Loopback, 12345));

            Info.UdpPort = GetUdpPort();
            Info.TcpPort = GetTcpPort();
            
            int GetUdpPort()
            {
                UdpClient l = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
                int p = ((IPEndPoint)l.Client.LocalEndPoint).Port;
                l.Close();
                return p;
            }

            int GetTcpPort()
            {
                var listener = new TcpListener(IPAddress.Any, 0);
                listener.Start();
                var p = ((IPEndPoint)listener.LocalEndpoint).Port;
                listener.Stop();
                return p;
            }
        }

        public void Start()
        {
            Receiver = new Receiver(_broadcastChannel);
            Receiver.Start();
	        Broadcaster = new Broadcaster(_broadcastChannel, Info);
	        Broadcaster.Start();

            Console.WriteLine($"Node started:");
            Console.WriteLine($"  Id:       {Info.Id}");
            Console.WriteLine($"  Service:  {Info.Name}");
            Console.WriteLine($"  TCP:      {Info.TcpAddress}");
            Console.WriteLine($"  UDP:      {Info.UdpAddress}");
        }

        public void RegisterEndpoint(IHandler handler)
        {
            Endpoints[handler.Endpoint] = handler;
        }

        public void InvokeEndpoint(string serialized)
        {
            var deserialized = Osprey.Serializer.Deserialize<EmptyMessage>(serialized);
            Endpoints[deserialized.Endpoint].Handle(serialized);
        }

        public void Dispose()
        {
            _broadcastChannel?.Dispose();
        }
    }
}