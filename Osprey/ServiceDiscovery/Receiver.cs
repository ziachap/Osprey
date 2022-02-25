using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Osprey.Communication;

namespace Osprey.ServiceDiscovery
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
                    try
                    {

                        var message = _client.Receive();

                        Discovered.AddOrUpdate(message, msg =>
                        {
                            var nodeInfo = OSPREY.Network.Serializer.Deserialize<NodeInfo>(message);
                            var nodeInfoEntry = new NodeInfoEntry(nodeInfo);
                            return nodeInfoEntry;
                        }, (msg, node) =>
                        {
                            node.Update();
                            return node;
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to receive UDP multicast");
                        Console.WriteLine(ex);
                    }

                }
            }, TaskCreationOptions.LongRunning);
        }

        public NodeInfo Locate(string service, bool throwError = false)
        {
            return Active
                       .Where(x => x.Name == service)
                       .OrderBy(x => Guid.NewGuid())
                       .FirstOrDefault()
                   ?? (throwError ? throw new ServiceUnavailableException("Service not found") : (NodeInfo)null);
        }

        private class NodeInfoEntry
        {
            public NodeInfoEntry(NodeInfo node)
            {
                Node = node;
                Discovered = DateTime.UtcNow;
                Timeout = OSPREY.Network.Config.Network.DiscoveryTimeout;
            }

            private DateTime Discovered { get; set; }
            private int Timeout { get; }
            public NodeInfo Node { get; }
            public bool Active => DateTime.UtcNow < Discovered.AddMilliseconds(Timeout);

            public void Update()
            {
                Discovered = DateTime.UtcNow;
            }
        }
    }

    public class ServiceUnavailableException : Exception
    {
        public ServiceUnavailableException()
        {
            
        }

        public ServiceUnavailableException(string message) : base(message)
        {
            
        }
    }
}