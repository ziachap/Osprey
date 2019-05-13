using System;
using System.Net;
using Osprey.Communication;

namespace Osprey
{
    public class NodeInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
    }

    public class Node : IDisposable
    {
        public NodeInfo Info { get; private set; }
        public Receiver Receiver { get; private set; }
        public Broadcaster Broadcaster { get; private set; }
		
        private readonly UdpChannel _channel;

        public Node(string id, string name, IPAddress ip, int port)
        {
			Info = new NodeInfo
			{
				Id = id,
				Name = name,
				Port = port,
				Address = $"{ip}:{port}"
			};
            _channel = new UdpChannel(new IPEndPoint(ip, port));
        }

        public void Start()
        {
            Receiver = new Receiver(_channel);
            Receiver.Start();
	        Broadcaster = new Broadcaster(_channel, Info);
	        Broadcaster.Start();
		}

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}