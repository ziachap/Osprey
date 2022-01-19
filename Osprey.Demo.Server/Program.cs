using System;
using System.Threading;
using System.Threading.Tasks;
using Osprey.Tcp;
using Osprey.Http;
using Osprey.ZeroMQ;

namespace Osprey.Demo.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("========== OSPREY SERVER ==========");

            using (Osprey.Default())
            using (Osprey.Join("osprey.server"))
            using (var zmq = new ZeroMQServer("zmq"))
            //using (var tcp = new TcpServer("mango"))
            {
                /*
                StartSendingHttp();

                Task.Run(() =>
                {
                    while (true)
                    {
                        tcp.Broadcast(Guid.NewGuid().ToString());
                        Thread.Sleep(100);
                    }
                });
                */
                while (true)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private static void StartSendingHttp()
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        var task = Osprey.Locate("osprey.client")
                            .Http()
                            .Send(new HttpMessage<string, string>
                            {
                                Endpoint = "test",
                                Payload = "apples",
                                RequestType = HttpVerb.GET
                            });

                        var response = await task;
                        
                        Console.WriteLine("RESPONSE: " + response);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    Thread.Sleep(4000);
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
