using System;
using System.Linq;
using System.Threading;
using Osprey.Http;
using Osprey.ServiceDescriptors;
using TcpService = Osprey.Tcp.TcpService;

namespace Osprey.Monitor
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Osprey.Default())
            using (Osprey.Join("osprey.monitor"))
            {
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("========== OSPREY MONITOR ==========");
                    Console.WriteLine("");
                    PrintDiscovered();

                    Thread.Sleep(2000);
                }
            }
        }

        private static void PrintDiscovered()
        {
            var active = Osprey.Node.Receiver.Active.ToList();
            Console.WriteLine($"-- Active [{active.Count}] --");
            foreach (var node in active)
            {
                Console.WriteLine($"{node.Id} | {node.Name} | {node.Ip}");
                foreach (var host in node.Services)
                {
                    switch (host.Value)
                    {
                        case HttpService httpInfo:
                            Console.WriteLine($"    HTTP [{httpInfo.Name}] ({httpInfo.Url})");
                            foreach (var endpoint in httpInfo.Endpoints)
                            {
                                Console.WriteLine($"        {endpoint.Name} | {endpoint.Address}");
                            }
                            break;
                        case TcpService tcpHost:
                            Console.WriteLine($"    TCP [{tcpHost.Name}] ({tcpHost.Endpoint.Address})");
                            break;
                        case ZeroMQService mqHost:
                            Console.WriteLine($"    ZMQ [{mqHost.Name}] ({mqHost.Endpoint.Address})");
                            break;
                    }
                }
            }
        }
    }
}
