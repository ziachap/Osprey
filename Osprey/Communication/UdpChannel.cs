using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Osprey.Communication
{
    public class UdpChannel : IDisposable
    {
        private readonly UdpClient _client;
        private readonly IPEndPoint _remote;
        private readonly IPEndPoint _local;

        public string LocalEndpoint => _local.ToString();
        public string RemoteEndpoint => _remote.ToString();

        public UdpChannel(IPAddress remote, IPAddress local, int port)
        {
            _remote = new IPEndPoint(remote, port);
            _local = new IPEndPoint(local, port);

            _client = new UdpClient
            {
                EnableBroadcast = true
            };
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _client.Client.ReceiveTimeout = 30000;
            _client.Client.Bind(_local);

            if (IsMulticast(remote)) _client.JoinMulticastGroup(remote, local);
        }

        public void Send<T>(T obj)
        {
            var serialized = Osprey.Serializer.Serialize(obj);
            var bytes = Encoding.ASCII.GetBytes(serialized);
            _client.Send(bytes, bytes.Length, _remote);
        }

        public T Receive<T>()
        {
            var _ = null as IPEndPoint;
            var buffer = _client.Receive(ref _);
            var message = Encoding.ASCII.GetString(buffer);
            return Osprey.Serializer.Deserialize<T>(message);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

        private static bool IsMulticast(IPAddress address)
        {
            return address.GetAddressBytes()[0] >= 224 && address.GetAddressBytes()[0] <= 239;
        }
    }
}
