using System;
using Osprey.Serialization;
using Osprey.Utilities;

namespace Osprey
{
	public class Osprey : IDisposable
    {
        private static Osprey _instance = null;
        public static Osprey Instance => _instance ?? throw new Exception("Caller has not joined an Osprey network.");

        public Node Node { get; set; }
        public ISerializer Serializer { get; set; }
        
		public static Osprey Join(string service, string environment, Action<Osprey> configuration = null)
		{
            if (Instance != null) throw new Exception("Cannot join the network more than once.");

            var osprey = new Osprey();

			var ip = Address.GetLocalIpAddress();
			var id = Guid.NewGuid().ToString();

            osprey.Serializer = new JsonSerializer();
            osprey.Node = new Node(id, service, environment, ip);

            configuration?.Invoke(osprey);

            osprey.Node.Start();

            _instance = osprey;

            return osprey;
        }

        public NodeInfo Locate(string node)
        {
            if (Node == null) throw new Exception("Caller has not joined an Osprey network");

            return Node.Receiver.Locate(node);
        }

        public void Dispose()
        {
            Node.Dispose();
            Node = null;
            Serializer = null;
            _instance = null;
        }
    }
}
