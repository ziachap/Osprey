using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Osprey.Utilities
{
    public static class Address
    {
        public static IPEndPoint GenerateUdpEndpoint()
        {
            return new IPEndPoint(GetLocalIpAddress(), GetTcpPort());
        }

        public static IPEndPoint GenerateTcpEndpoint()
        {
            return new IPEndPoint(GetLocalIpAddress(), GetTcpPort());
        }

        public static int GetUdpPort()
        {
            var l = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
            var p = ((IPEndPoint)l.Client.LocalEndPoint).Port;
            l.Close();
            return p;
        }

        public static int GetTcpPort()
        {
            var listener = new TcpListener(IPAddress.Any, 0);
            listener.Start();
            var p = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return p;
        }

        public static IPAddress GetLocalIpAddress()
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
}
