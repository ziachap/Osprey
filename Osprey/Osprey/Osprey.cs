using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Osprey
{
	public static class Osprey
	{
		public static IDisposable Join(string name)
		{
			var ip = GetLocalIPAddress();
			var id = Guid.NewGuid().ToString();
			var port = 12345;
			//var port = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;

			var node = new Node(id, name, ip, port);

			node.Start();

			return node;
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

	public class Node : IDisposable
	{
		public static string Id { get; private set; }
		public static string Name { get; private set; }
		public static string Address { get; private set; }
		public static IPAddress Ip { get; private set; }
		public static int Port { get; private set; }

		private UdpClient _client;

		public Node(string id, string name, IPAddress ip, int port)
		{
			Id = id;
			Name = name;
			Ip = ip;
			Port = port;
			Address = $"{ip}:{Port}";
		}

		public void Start()
		{
			_client = new UdpClient();
			_client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			_client.Client.Bind(new IPEndPoint(Ip, Port));

			var message = $"{Id} | {Name} | {Address}";
			var from = new IPEndPoint(0, 0);

			Task.Run(() =>
			{
				while (true)
				{
					var recvBuffer = _client.Receive(ref from);
					Console.WriteLine(Encoding.UTF8.GetString(recvBuffer));
				}
			});

			Task.Run(() =>
			{
				while (true)
				{
					var data = Encoding.UTF8.GetBytes(message);
					_client.Send(data, data.Length, "255.255.255.255", Port);
					Thread.Sleep(2000);
				}
			});
		}

		public void Dispose()
		{
			_client.Dispose();
		}
	}
}
