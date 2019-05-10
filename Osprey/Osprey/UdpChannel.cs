using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace Osprey
{
    public class UdpChannel : IDisposable
    {
        private readonly UdpClient _client;
        private int Port => ((IPEndPoint) _client.Client.LocalEndPoint).Port;

        public UdpChannel(IPEndPoint endpoint)
        {
            _client = new UdpClient
            {
                EnableBroadcast = true
            };
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _client.Client.Bind(endpoint);
        }

        public void Send<T>(T obj)
        {
            var serialized = Osprey.Serializer.Serialize(obj);
            var bytes = Encoding.ASCII.GetBytes(serialized);
            _client.Send(bytes, bytes.Length, "255.255.255.255", Port);
        }

        public T Receive<T>()
        {
            var from = new IPEndPoint(0, 0);
            var buffer = _client.Receive(ref from);
            var message = Encoding.ASCII.GetString(buffer);
            return Osprey.Serializer.Deserialize<T>(message);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
