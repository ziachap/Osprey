using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
using Osprey.ServiceDescriptors;
using Osprey.Utilities;

namespace Osprey.ZeroMQ
{
    public class ZeroMQServer : IDisposable
    {
        private bool _closed = false;

        private readonly ConcurrentDictionary<string, Client> _clients = new ConcurrentDictionary<string, Client>();
        private readonly IPEndPoint _endpoint;

        public ZeroMQServer(string name)
        {
            _endpoint = Address.GenerateTcpEndpoint();

            StartListener();

            var hostInfo = new ZeroMQService(new Endpoint()
            {
                Name = name,
                Address = _endpoint.ToString()
            });

            Console.WriteLine("Created ZeroMQ server on " + _endpoint);

            Osprey.Node.Register(hostInfo);
        }

        public void StartListener()
        {
            Task.Run(() =>
            {
                using (var server = new ResponseSocket())
                {
                    server.Bind("tcp://" + _endpoint.ToString());
                    while (!_closed)
                    {
                        try
                        {
                            var raw = server.ReceiveFrameString();
                            Console.WriteLine("Received {0}", raw);
                            var message = Osprey.Serializer.Deserialize<EstablishRequest>(raw);

                            var client = _clients.AddOrUpdate(message.ClientId, id => new Client(), (id, client) =>
                            {
                                client?.Dispose();
                                return new Client();
                            });

                            client.OnDisconnected += () =>
                            {
                                Console.WriteLine("Lost connection to client: "+ message.ClientId);
                                _clients.TryRemove(message.ClientId, out var c);
                                c?.Dispose();
                            };

                            Console.WriteLine("Registered client: " + message.ClientId);
                            
                            var response = Osprey.Serializer.Serialize(new EstablishResponse
                            {
                                ClientId = message.ClientId,
                                Endpoint = client.Endpoint.ToString()
                            });
                            
                            server.SendFrame(response);

                            client.StartHeartbeatThread();
                        }
                        catch (Exception ex)
                        {
                            // TODO
                            Console.WriteLine(ex);
                        }
                    }
                }
            });
        }

        public void Dispose()
        {
            _closed = true;
        }
    }

    public class EstablishRequest
    {
        public string ClientId { get; set; }
    }

    public class EstablishResponse
    {
        public string ClientId { get; set; }

        public string Endpoint { get; set; }
    }

    public class Client : IDisposable
    {
        public Action OnDisconnected;
        public bool _closed = false;

        public Client()
        {
            Socket = new PublisherSocket();
            Endpoint = Address.GenerateTcpEndpoint();
            Socket.Bind("tcp://" + Endpoint);
        }

        public IPEndPoint Endpoint { get; }
        
        public PublisherSocket Socket { get; }

        public void StartHeartbeatThread()
        {
            Task.Run(() =>
            {
                while (!_closed)
                {
                    Console.WriteLine("Sending heartbeat.");
                    if (!Socket.SendMoreFrame("__heartbeat").TrySendFrame(TimeSpan.FromSeconds(5), "1"))
                    {
                        Console.WriteLine("Sending heartbeat failed.");
                        _closed = true;
                        OnDisconnected?.Invoke();
                        return;
                    }
                    Thread.Sleep(1000);
                }
            }).ContinueWith(task =>
            {
                Console.WriteLine("¬ Heartbeat sending thread has ended.");
            }); ;
        }
        
        public void Dispose()
        {
            _closed = true;
            Socket?.Dispose();
        }
    }
}
