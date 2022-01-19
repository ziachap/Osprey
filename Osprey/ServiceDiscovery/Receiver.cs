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

                        var nodeInfo = _client.Receive<NodeInfo>();
                        var nodeInfoEntry = new NodeInfoEntry(nodeInfo);

                        Discovered.AddOrUpdate(nodeInfo.Id, nodeInfoEntry, (id, n) => nodeInfoEntry);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to receive UDP multicast: " + ex.Message);
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
                Discovered = DateTime.Now;
            }

            public NodeInfo Node { get; }
            private DateTime Discovered { get; }
            public bool Active => DateTime.Now < Discovered.AddSeconds(3);
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