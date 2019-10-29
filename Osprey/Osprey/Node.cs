using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Osprey.Communication;

namespace Osprey
{
    public class Endpoint
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }

    public class NodeInfo
    {
        public NodeInfo()
        {
            Endpoints = new List<Endpoint>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Ip { get; set; }
        public int UdpPort { get; set; }
        public int TcpPort { get; set; }

        public string UdpAddress => Ip + ":" + UdpPort;
        public string TcpAddress => Ip + ":" + TcpPort;

        public List<Endpoint> Endpoints { get; }
    }

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
                Endpoints =
                {
                    new Endpoint(){Name = "test1", Address = "123.1.1.3:34567"},
                    new Endpoint(){Name = "test2", Address = "123.1.1.3:94841"},
                },
                UdpPort = GetUdpPort(),
                TcpPort = GetTcpPort(),
			};
            _broadcastChannel = new UdpChannel(new IPEndPoint(IPAddress.Loopback, 12345));
            
            int GetUdpPort()
            {
                var l = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
                var p = ((IPEndPoint)l.Client.LocalEndPoint).Port;
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

        internal void Start()
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
            //UdpEndpoints[handler.Endpoint] = handler;
        }

        internal void InvokeEndpoint(string serialized)
        {
            var deserialized = Osprey.Serializer.Deserialize<EmptyMessage>(serialized);
            //UdpEndpoints[deserialized.Endpoint].Handle(serialized);
        }

        public void Dispose()
        {
            _broadcastChannel?.Dispose();
        }
    }
}