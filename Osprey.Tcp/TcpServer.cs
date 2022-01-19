using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Osprey.ServiceDescriptors;
using Osprey.Utilities;

namespace Osprey.Tcp
{
    public class TcpServer : IDisposable
    {
        public TcpServer(string endpoint)
        {
            var ipEndpoint = Address.GenerateTcpEndpoint();

            Listener = new TcpListener(ipEndpoint);
            Connections = new ConcurrentBag<TcpConnection>();

            StartAsync();

            var hostInfo = new TcpService(new Endpoint()
            {
                Name = endpoint,
                Address = ipEndpoint.ToString()
            });

            Osprey.Node.Register(hostInfo);
        }

        public Task StartAsync()
        {
            Listener.Start();

            return Task.Run(() =>
            {
                while (true)
                {
                    var client = Listener.AcceptTcpClient();
                    var remote = ((IPEndPoint) client.Client.RemoteEndPoint).ToString();
                    Console.WriteLine("Client connected: " + remote);
                    var connection = new TcpConnection(client, remote);
                    Connections.Add(connection);
                }
            });
        }

        public void Dispose()
        {
            // TODO
        }

        public void Broadcast<T>(T message)
        {
            //var deadClients = new List<ClientConnection>();
            foreach (var client in Connections)
            {
                try
                {
                    client.Send(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not broadcast to client: " + ex.Message);
                    //deadClients.Add(client);
                }
            }
            /*
            foreach (var client in deadClients)
            {
                client.Dispose();
                while (!Clients.TryRemove(client.Name, out _)) { }
            }*/
        }

        public ConcurrentBag<TcpConnection> Connections { get; }

        private TcpListener Listener { get; }

        public TcpService TcpService { get; }
    }
}