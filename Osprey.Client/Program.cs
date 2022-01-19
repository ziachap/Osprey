using System;
using System.Threading;
using Nancy;
using Osprey.Http;
using Osprey.ServiceDescriptors;
using Osprey.Tcp;
using Osprey.ZeroMQ;

namespace Osprey.Demo.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("========== OSPREY CLIENT ==========");
            using (Osprey.Default())
            using (Osprey.Join("osprey.client"))
            //using (new TcpServer("tcp1"))
            //using (new TcpServer("tcp2"))
            //using (new TcpServer("tcp3"))
            //using (new HttpServer<DefaultStartup<DefaultNancyBootstrapper>>("http"))
            {
                //ConnectToTcpServer();

                var client = new ZeroMQClient("osprey.server", "zmq");
                client.OnDisconnected += () => Console.WriteLine("CLIENT IS DISCONNECTED");
                client.OnConnected += () => Console.WriteLine("CLIENT IS CONNECTED");
                client.Connect();

                Console.ReadKey();

                while (true)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private static void ConnectToTcpServer()
        {
            var connected = false;
            while (!connected)
            {
                Console.WriteLine("Looking for server...");

                try
                {
                    Osprey.Locate("osprey.server")
                        .Stream("mango")
                        .Subscribe(msg => Console.WriteLine("MSG: " + msg.ToString()));

                    connected = true;
                }
                catch
                {
                    // ignored
                }

                Thread.Sleep(2000);
            }

            Console.WriteLine("Connected.");
        }
    }
}
