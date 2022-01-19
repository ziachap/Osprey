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
                                _clients.TryRemove(message.ClientId, out _);
                            };

                            Console.WriteLine("Registered client: " + message.ClientId);
                            
                            var response = Osprey.Serializer.Serialize(new EstablishResponse
                            {
                                ClientId = message.ClientId,
                                StreamEndpoint = client.StreamEndpoint.ToString(),
                                HeartbeatEndpoint = client.HeartbeatEndpoint.ToString()
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

        public void Publish<T>(string topic, T data)
        {
            var msg = Osprey.Serializer.Serialize(data);

            foreach (var client in _clients.Values)
            {
                client.Publish(topic, msg);
            }
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

        public string StreamEndpoint { get; set; }

        public string HeartbeatEndpoint { get; set; }
    }

    public class Client : IDisposable
    {
        private const int HeartbeatTimeoutMs = 3000;
        private const int HeartbeatIntervalMs = 1000;

        public Action OnDisconnected;
        private bool _closed = false;

        public Client()
        {
            OnDisconnected += Dispose;

            StreamSocket = new PublisherSocket();
            StreamEndpoint = Address.GenerateTcpEndpoint();
            StreamSocket.Options.SendHighWatermark = 1000;
            StreamSocket.Bind("tcp://" + StreamEndpoint);

            HeartbeatSocket = new RequestSocket();
            HeartbeatEndpoint = Address.GenerateTcpEndpoint();
            HeartbeatSocket.Bind("tcp://" + HeartbeatEndpoint);
        }

        public IPEndPoint StreamEndpoint { get; }
        public IPEndPoint HeartbeatEndpoint { get; }
        
        public PublisherSocket StreamSocket { get; }
        public RequestSocket HeartbeatSocket { get; }

        public void StartHeartbeatThread()
        {
            Task.Run(() =>
            {
                while (!_closed)
                {
                    if (!HeartbeatSocket.TrySendFrame(TimeSpan.FromMilliseconds(HeartbeatTimeoutMs), "ping"))
                    {
                        Console.WriteLine("Sending heartbeat failed.");
                        OnDisconnected?.Invoke();
                        return;
                    }
                    Console.WriteLine("ping");
                    if (!HeartbeatSocket.TryReceiveFrameString(TimeSpan.FromMilliseconds(HeartbeatTimeoutMs), out var response))
                    {
                        Console.WriteLine("Receiving heartbeat response failed.");
                        OnDisconnected?.Invoke();
                        return;
                    }
                    Console.WriteLine(response);
                    Thread.Sleep(HeartbeatIntervalMs);
                }
            }).ContinueWith(task =>
            {
                Console.WriteLine("¬ Heartbeat sending thread has ended.");
            }); ;
        }

        private readonly object _streamLock = new object();
        public void Publish(string topic, string data)
        {
            lock (_streamLock)
            {
                if (!StreamSocket.TrySendFrame(TimeSpan.FromSeconds(1), topic, true))
                {
                    throw new TimeoutException("Timed out while publishing data topic.");
                }
                if (!StreamSocket.TrySendFrame(TimeSpan.FromSeconds(1), data))
                {
                    throw new TimeoutException("Timed out while publishing data message.");
                }
                Console.WriteLine($"Published {topic} = {data}");
            }
        }
        
        public void Dispose()
        {
            _closed = true;
            StreamSocket?.Dispose();
        }
    }
}
