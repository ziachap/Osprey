using System;
using System.Linq;
using System.Threading;
using Fclp;
using Osprey.Http;
using Osprey.Utilities;

namespace Osprey.Monitor
{
    class Program
    {
        static void Main(string[] args)
        {
            using (OSPREY.Join("osprey.monitor", "acceptance"))
            {
                while (true)
                {
                    //Console.Clear();
                    Console.WriteLine("========== OSPREY MONITOR ==========");
                    Console.WriteLine("");
                    PrintDiscovered();

                    Thread.Sleep(4000);
                }
            }
        }

        private static void PrintDiscovered()
        {
            var active = OSPREY.Network.Node.Receiver.Active.ToList();
            Console.WriteLine($"-- Active [{active.Count}] --");
            foreach (var node in active)
            {
                Console.WriteLine($"{node.Id} | {node.Name} | {node.Ip}");
                foreach (var service in node.Services)
                {
                    switch (service.Type)
                    {
                        case "http":
                            Console.WriteLine($"    HTTP [{service.Name}] ({service.Address})");
                            break;
                        case "zmq":
                            Console.WriteLine($"    ZMQ [{service.Name}] ({service.Address})");
                            break;
                    }
                }
            }
        }
    }
}
