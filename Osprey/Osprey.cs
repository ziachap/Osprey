using System;
using System.Net;
using System.Net.Sockets;
using Osprey.Communication;
using Osprey.Serialization;
using Osprey.Utilities;

namespace Osprey
{
	public static class Osprey
	{
        public static Node Node { get; set; }
        public static ISerializer Serializer { get; set; }

        private class DisposeOsprey : IDisposable
        { 
            public void Dispose()
            {
                Node.Dispose();
                Node = null;
                Serializer = null;
            }
        }

        public static IDisposable Default()
        {
           Serializer = new JsonSerializer();
           return new DisposeOsprey();
        }

		public static Node Join(string service)
		{
            if (Node != null) throw new Exception("Cannot join the network more than once.");

			var ip = Address.GetLocalIpAddress();
			var id = Guid.NewGuid().ToString();

            Node = new Node(id, service, ip);
            Node.Start();

            return Node;
        }

        public static NodeInfo Locate(string node)
        {
            if (Node == null) throw new Exception("Caller has not joined an Osprey network");

            return Node.Receiver.Locate(node);
        }
    }
}
