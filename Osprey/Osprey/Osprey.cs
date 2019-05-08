using System;
using System.Net;
using System.Net.Sockets;
using Osprey.Serialization;

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

		public static IDisposable Join(string name)
		{
			var ip = GetLocalIPAddress();
			var id = Guid.NewGuid().ToString();
			var port = 12345;
            //var port = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;

            Node = new Node(id, name, ip, port);

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
