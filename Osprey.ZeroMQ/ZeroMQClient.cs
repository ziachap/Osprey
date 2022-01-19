using System;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
using Osprey.ServiceDescriptors;

namespace Osprey.ZeroMQ
{
    public class ZeroMQClient : IDisposable
    {
        private const int HeartbeatTimeoutMs = 3000;

        public Action OnConnected;
        public Action OnDisconnected;
        
        private readonly string _id;

        private string _serviceName;
        private string _endpointName;

        private ResponseSocket _heartbeatSocket;
        private SubscriberSocket _streamSocket;
        private bool _connected = false;
        
        public ZeroMQClient(string service, string endpoint)
        {
            _id = Guid.NewGuid().ToString();
            _serviceName = service;
            _endpointName = endpoint;
            OnDisconnected += () =>
            {
                _connected = false;
                _streamSocket?.Dispose();
                Task.Run(RetryConnect);
            };
        }

        private void RetryConnect()
        {
            while (!_connected)
            {
                try
                {
                    Connect();
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to reconnect: " + ex.Message);
                }
                Thread.Sleep(2000);
            }
        }

        public void Connect()
        {
            // Locate service
            var expires = DateTime.Now.AddSeconds(3);
            ZeroMQService service = null;
            while (service == null)
            {
                service = Osprey.Locate(_serviceName)?.Services[_endpointName] as ZeroMQService;
                Thread.Sleep(10);

                if (DateTime.Now > expires)
                    throw new TimeoutException("Timed out while trying to locate service.");
            } 

            // Establish connection to server
            var data = new EstablishRequest
            {
                ClientId = Osprey.Node.Info.Id
            };

            var json = Osprey.Serializer.Serialize(data);

            string streamEndpoint;
            string heartbeatEndpoint;
            using (var client = new RequestSocket())
            {
                client.Connect("tcp://" + service.Endpoint.Address);
                if (!client.TrySendFrame(TimeSpan.FromSeconds(3), json))
                {
                    throw new TimeoutException("Timed out while waiting to send to server.");
                };

                if (!client.TryReceiveFrameString(TimeSpan.FromSeconds(3), out var raw))
                {
                    throw new TimeoutException("Timed out while waiting for response from server.");
                };

                var response = Osprey.Serializer.Deserialize<EstablishResponse>(raw);

                Console.WriteLine("Stream address is: " + response.StreamEndpoint);
                Console.WriteLine("Heartbeat address is: " + response.HeartbeatEndpoint);

                streamEndpoint = response.StreamEndpoint;
                heartbeatEndpoint = response.HeartbeatEndpoint;
            }

            // Setup streaming socket
            _streamSocket = new SubscriberSocket();
            _streamSocket.Options.ReceiveHighWatermark = 1000;
            _streamSocket.Connect("tcp://" + streamEndpoint);

            // Setup heartbeat socket
            _heartbeatSocket = new ResponseSocket();
            _heartbeatSocket.Connect("tcp://" + heartbeatEndpoint);

            _connected = true;
            StartHeartbeatThread();
            StartListenerThread();

            OnConnected?.Invoke();
        }

        private void StartHeartbeatThread()
        {
            Task.Run(() =>
            {
                Console.WriteLine("Heartbeat started for {0}", _id);
                try
                {
                    while (_connected)
                    {
                        if (!_heartbeatSocket.TryReceiveFrameString(TimeSpan.FromMilliseconds(HeartbeatTimeoutMs), out var request))
                            throw new TimeoutException("Heartbeat timed out");

                        //Console.WriteLine("ping");

                        if (!_heartbeatSocket.TrySendFrame(TimeSpan.FromMilliseconds(HeartbeatTimeoutMs), "pong"))
                            throw new TimeoutException("Heartbeat timed out");

                        //Console.WriteLine("pong");
                    }
                }
                catch (TimeoutException ex)
                {
                    OnDisconnected?.Invoke();
                }
            }).ContinueWith(task =>
            {
                Console.WriteLine("¬ Heartbeat thread has ended.");
            });
        }

        private void StartListenerThread()
        {
            Task.Run(() =>
            {
                Console.WriteLine("Listener started for {0}", _id);
                Console.WriteLine("Subscriber socket connecting...");

                while (_connected)
                {
                    string messageTopic;
                    while (!_streamSocket.TryReceiveFrameString(out messageTopic))
                    {
                        if (!_connected) return;
                        Thread.Sleep(1);
                    }

                    string messageReceived;
                    while (!_streamSocket.TryReceiveFrameString(out messageReceived))
                    {
                        if (!_connected) return;
                        Thread.Sleep(1);
                    }

                    Console.WriteLine($"{DateTime.Now} | {messageTopic} = {messageReceived}");
                }
            }).ContinueWith(task =>
            {
                Console.WriteLine("¬ Listener thread has ended.");
            });
        }

        public void Subscribe(string topic)
        {
            _streamSocket.Subscribe(topic);
        }

        public void Dispose()
        {
            _streamSocket?.Dispose();
        }
    }
}