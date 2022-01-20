using System;
using System.Threading;
using Nancy;
using Osprey.Http;
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
            using (new HttpServer<DefaultStartup<DefaultNancyBootstrapper>>("http"))
            {

                var client = new ZeroMQClient("osprey.server", "zmq1");

                client.OnDisconnected += () => Console.WriteLine("CLIENT IS DISCONNECTED");

                client.OnConnected += () => Console.WriteLine("CLIENT IS CONNECTED");

                client.OnConnected += () =>
                {
                    client.Subscribe("A");
                    client.Subscribe("B");
                };

                client.Connect();

                Console.ReadKey();

                while (true)
                {
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
