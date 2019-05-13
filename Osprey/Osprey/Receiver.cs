using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Osprey.Communication;

namespace Osprey
{
    public class Receiver
    {
        private readonly UdpChannel _client;

        private ConcurrentDictionary<string, NodeInfoEntry> Discovered { get; }
        public IEnumerable<NodeInfo> Active => Discovered.Values.Where(x => x.Active).Select(x => x.Node);

        public Receiver(UdpChannel client)
        {
            _client = client;
            Discovered = new ConcurrentDictionary<string, NodeInfoEntry>();
        }

        public void Start()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var nodeInfo = _client.Receive<NodeInfo>();
                    var nodeInfoEntry = new NodeInfoEntry(nodeInfo);

                    Discovered.AddOrUpdate(nodeInfo.Id, nodeInfoEntry, (id, n) => nodeInfoEntry);

                    Console.WriteLine("-- Active --");
                    Console.WriteLine(string.Join(Environment.NewLine,
                        Active.Select(x => $"{x.Id} | {x.Name} | {x.Address}")));
                }
            }, TaskCreationOptions.LongRunning);
        }

	    public NodeInfo FindService(string service)
	    {
		    return Active.FirstOrDefault(x => x.Name == service) ?? throw new Exception("Service not found");
	    }

        private class NodeInfoEntry
        {
            public NodeInfoEntry(NodeInfo node)
            {
                Node = node;
                Discovered = DateTime.Now;
            }

            public NodeInfo Node { get; }
            private DateTime Discovered { get; }
            public bool Active => DateTime.Now < Discovered.AddSeconds(1.5);
        }
    }
}