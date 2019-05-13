using System;
using System.Net;
using System.Net.Sockets;
using Osprey.Communication;
using Osprey.Serialization;

namespace Osprey
{
	public static class Osprey
	{
        public static Node Node { get; set; }
        public static Network Network { get; set; }
        public static ISerializer Serializer { get; set; }
        public static IHttp Http { get; set; }

        private class DisposeOsprey : IDisposable
        { 
            public void Dispose()
            {
                Node.Dispose();
                Node = null;
                Serializer = null;
	            Http = null;
            }
        }

        public static IDisposable Default()
        {
           Serializer = new JsonSerializer();
		   Http = new Http();
           return new DisposeOsprey();
        }

		public static IDisposable Join(string service)
		{
			var ip = GetLocalIPAddress();
			var id = Guid.NewGuid().ToString();

            Node = new Node(id, service, ip, 12345);
            Node.Start();

			return Node;
		}
		
		private static IPAddress GetLocalIPAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					return ip;
				}
			}
			throw new Exception("No network adapters with an IPv4 address in the system!");
		}

    }
}
