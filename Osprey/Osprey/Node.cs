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
        private readonly UdpChannel _channel;

        public Node(string id, string name, IPAddress ip, int port)
        {
            Id = id;
            Name = name;
            _ip = ip;
            Port = port;
            Address = $"{ip}:{Port}";
            _channel = new UdpChannel(new IPEndPoint(_ip, Port));
        }

        public void Start()
        {

            Receiver = new Receiver(_channel);
            Receiver.Start();

            Task.Run(() =>
            {
                while (true)
                {
                    _channel.Send(this);
                    Thread.Sleep(1000);
                }
            });
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}