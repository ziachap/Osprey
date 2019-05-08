using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Osprey
{
    public class Receiver
    {
        private readonly UdpClient _client;
        public List<NodeInfo> Discovered { get; }

        public Receiver(UdpClient client)
        {
            _client = client;
            Discovered = new List<NodeInfo>();
        }

        public void Start()
        {
            var from = new IPEndPoint(0, 0);
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var recvBuffer = _client.Receive(ref from);
                    var message = Encoding.UTF8.GetString(recvBuffer);
                    var nodeInfo = Osprey.Serializer.Deserialize<NodeInfo>(message);

                    if (Discovered.Exists(MatchesId))
                    {
                        var index = Discovered.FindIndex(MatchesId);
                        Discovered[index] = nodeInfo;
                    }
                    else
                    {
                        Discovered.Add(nodeInfo);
                    }

                    Console.WriteLine("Discovered:");
                    Console.WriteLine(string.Join(Environment.NewLine, Discovered.Select(x => $"{x.Id} | {x.Name} | {x.Address}")));

                    bool MatchesId(NodeInfo x) => x.Id == nodeInfo.Id;
                }

            }, TaskCreationOptions.LongRunning);
        }
    }

    public class Broadcaster
    {

    }
}