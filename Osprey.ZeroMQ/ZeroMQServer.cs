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
using Osprey.ZeroMQ.Models;

namespace Osprey.ZeroMQ
{
    public class ZeroMQServer : IDisposable
    {
        /// <summary>
        /// Invoked when a client subscribes to a topic.
        /// </summary>
        public event Action<string> OnSubscribe;
        private readonly ConcurrentDictionary<string, string> _lastValueCache = new ConcurrentDictionary<string, string>();

        private bool _closed = false;

        private readonly ConcurrentDictionary<string, Client> _clients = new ConcurrentDictionary<string, Client>();
        private readonly IPEndPoint _endpoint;

        public ZeroMQServer(string name)
        {
            _endpoint = Address.GenerateTcpEndpoint();

            StartListenerThread();

            var hostInfo = new ZeroMQService(new Endpoint()
            {
                Name = name,
                Address = _endpoint.ToString()
            });

            Console.WriteLine("Created ZeroMQ server on " + _endpoint);

            Osprey.Instance.Node.Register(hostInfo);
        }

        private void StartListenerThread()
        {
            Task.Run(() =>
            {
                using (var server = new ResponseSocket())
                {
                    server.Bind("tcp://" + _endpoint);
                    while (!_closed)
                    {
                        try
                        {
                            var raw = server.ReceiveFrameString();
                            Console.WriteLine("Received {0}", raw);
                            var message = Osprey.Instance.Serializer.Deserialize<EstablishRequest>(raw);

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
                            
                            client.OnSubscribe += topic =>
                            {
                                if (_lastValueCache.TryGetValue(topic, out var lastValue))
                                {
                                    client.Publish(topic, lastValue);
                                }

                                OnSubscribe?.Invoke(topic);
                            };

                            Console.WriteLine("Registered client: " + message.ClientId);
                            
                            var response = Osprey.Instance.Serializer.Serialize(new EstablishResponse
                            {
                                ClientId = message.ClientId,
                                StreamEndpoint = client.StreamEndpoint.ToString(),
                                HeartbeatEndpoint = client.HeartbeatEndpoint.ToString(),
                                ResponseEndpoint = client.ResponseEndpoint.ToString(),
                            });
                            
                            server.SendFrame(response);

                            client.StartHeartbeatThread();
                            client.StartResponseThread();
                        }
                        catch (Exception ex)
                        {
                            // TODO: Proper logging
                            Console.WriteLine(ex);
                        }
                    }
                }
            });
        }

        public void Publish(string topic, object data)
        {
            var msg = Osprey.Instance.Serializer.Serialize(data);

            _lastValueCache[topic] = msg;

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

    public class Client : IDisposable
    {
        private const int HeartbeatTimeoutMs = 3000;
        private const int HeartbeatIntervalMs = 1000;

        public event Action OnDisconnected;
        public event Action<string> OnSubscribe;

        private bool _closed = false;
        public readonly HashSet<string> Topics = new HashSet<string>();

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

            ResponseSocket = new ResponseSocket();
            ResponseEndpoint = Address.GenerateTcpEndpoint();
            ResponseSocket.Bind("tcp://" + ResponseEndpoint);
        }

        public IPEndPoint StreamEndpoint { get; }
        public IPEndPoint HeartbeatEndpoint { get; }
        public IPEndPoint ResponseEndpoint { get; }
        
        public PublisherSocket StreamSocket { get; }
        public RequestSocket HeartbeatSocket { get; }
        public ResponseSocket ResponseSocket { get; }

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
                    if (!HeartbeatSocket.TryReceiveFrameString(TimeSpan.FromMilliseconds(HeartbeatTimeoutMs), out var response))
                    {
                        Console.WriteLine("Receiving heartbeat response failed.");
                        OnDisconnected?.Invoke();
                        return;
                    }
                    Thread.Sleep(HeartbeatIntervalMs);
                }
            }).ContinueWith(task =>
            {
                Console.WriteLine("¬ Heartbeat sending thread has ended.");
            });
        }

        public void StartResponseThread()
        {
            Task.Run(() =>
            {
                Console.WriteLine("Started request/response thread.");
                while (!_closed)
                {
                    var command = ResponseSocket.ReceiveFrameString();
                    var msg = ResponseSocket.ReceiveFrameString();

                    if (command == "subscribe")
                    {
                        Topics.Add(msg);

                        try
                        {
                            OnSubscribe?.Invoke(msg);
                        }
                        catch (Exception ex)
                        {
                            // TODO
                            if (!ResponseSocket.TrySendFrame(TimeSpan.FromMilliseconds(HeartbeatTimeoutMs), ex.Message))
                            {
                                Console.WriteLine("Sending error response failed.");
                                OnDisconnected?.Invoke();
                            }
                            return;
                        }
                    }
                    else if (command == "unsubscribe")
                    {
                        Topics.Remove(msg);
                    }
                    
                    if (!ResponseSocket.TrySendFrame(TimeSpan.FromMilliseconds(HeartbeatTimeoutMs), "OK"))
                    {
                        Console.WriteLine("Sending response failed.");
                        OnDisconnected?.Invoke();
                        return;
                    }
                }
            }).ContinueWith(task =>
            {
                Console.WriteLine("¬ Request/response thread has ended.");
            });
        }

        private readonly object _streamLock = new object();
        public void Publish(string topic, string data)
        {
            if (!Topics.Contains(topic)) return;

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
            }
        }
        
        public void Dispose()
        {
            _closed = true;
            StreamSocket?.Dispose();
        }
    }
}
