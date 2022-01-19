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
        public Action OnConnected;
        public Action OnDisconnected;
        
        private readonly string _id;

        private string _serviceName;
        private string _endpointName;

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
                _lastHeartbeat = DateTime.MinValue;
                _streamSocket?.Dispose();
                RetryConnect();
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

                Console.WriteLine("Stream address is: " + response.Endpoint);

                streamEndpoint = response.Endpoint;
            }

            // Setup streaming socket
            _streamSocket = new SubscriberSocket();
            _streamSocket.Options.ReceiveHighWatermark = 1000;
            _streamSocket.Connect("tcp://" + streamEndpoint);

            _connected = true;
            StartHeartbeat();

            OnConnected?.Invoke();
        }

        private DateTime _lastHeartbeat;
        private void StartHeartbeat()
        {
            _lastHeartbeat = DateTime.Now;

            Task.Run(() =>
            {
                Console.WriteLine("Heartbeat started for {0}", _id);
                _streamSocket.Subscribe("__heartbeat");
                Console.WriteLine("Subscriber socket connecting...");
                while (_connected)
                {
                    if (!_streamSocket.TryReceiveFrameString(TimeSpan.FromSeconds(3), out var messageTopic))
                        throw new TimeoutException("Heartbeat timed out");
                    if (!_streamSocket.TryReceiveFrameString(TimeSpan.FromSeconds(3), out var messageReceived))
                        throw new TimeoutException("Heartbeat timed out");
                    Console.WriteLine(DateTime.Now + " = " + messageReceived);
                    _lastHeartbeat = DateTime.Now;
                }
            }).ContinueWith(task =>
            {
                Console.WriteLine("¬ Heartbeat receiving thread has ended.");
            });

            Task.Run(() =>
            {
                while (true)
                {
                    if (DateTime.Now > _lastHeartbeat.AddSeconds(3))
                    {
                        OnDisconnected?.Invoke();
                        return;
                    }
                    Thread.Sleep(10);
                }
            }).ContinueWith(task =>
            {
                Console.WriteLine("¬ Heartbeat monitoring thread has ended.");
            });
        }

        public void Subscribe(string topic)
        {

        }

        public void Dispose()
        {
            _streamSocket?.Dispose();
        }
    }
}