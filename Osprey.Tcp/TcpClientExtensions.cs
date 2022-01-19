using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Osprey.Utilities;

namespace Osprey.Tcp
{
    public static class TcpClientExtensions
    {
        public static TcpService Stream(this NodeInfo node, string service)
        {
            return (TcpService)(node.Services.Select(x => x.Value).SingleOrDefault(x => x is TcpService && x.Name == service)
                                 ?? throw new Exception("Located node does not contain a TCP server"));
        }

        public static void Subscribe(this TcpService tcpInfo, Action<object> messageHandler)
        {
            var endpoint = Address.GenerateTcpEndpoint();
            var client = new TcpClient(endpoint);

            Console.WriteLine("Connecting to server...");
            client.Connect(IPEndPoint.Parse(tcpInfo.Endpoint.Address));
            TempConnection = new TcpConnection(client, endpoint.ToString());

            TempConnection.OnMessage += messageHandler;
        }

        private static TcpConnection TempConnection { get; set; } // TODO: Where to store client connection?
    }
}
