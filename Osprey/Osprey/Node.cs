using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Osprey
{
    public class NodeInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
    }

    public class Node : NodeInfo, IDisposable
    {
        public Receiver Receiver { get; private set; }

        private readonly IPAddress _ip;
        private UdpClient _client;

        public Node(string id, string name, IPAddress ip, int port)
        {
            Id = id;
            Name = name;
            _ip = ip;
            Port = port;
            Address = $"{ip}:{Port}";
        }

        public void Start()
        {
            _client = new UdpClient
            {
                EnableBroadcast = true
            };
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _client.Client.Bind(new IPEndPoint(_ip, Port));

            var message = $"{Id} | {Name} | {Address}";
            var from = new IPEndPoint(0, 0);
            
            Receiver = new Receiver(_client);
            Receiver.Start();

            Task.Run(() =>
            {
                while (true)
                {
                    var serialized = Osprey.Serializer.Serialize((NodeInfo)this);
                    var data = Encoding.UTF8.GetBytes(serialized);
                    _client.Send(data, data.Length, "255.255.255.255", Port);
                    Thread.Sleep(1000);
                }
            });
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}