using System;
using System.Threading;
using System.Threading.Tasks;
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
            using (var zmq = new ZeroMQServer("zmq1"))
            {

                Task.Run(() =>
                {
                    while (true)
                    {
                        var data = new TestData()
                        {
                            Data1 = 234
                        };

                        zmq.Publish("A", data);

                        Thread.Sleep(1000);
                    }
                });


                Task.Run(() =>
                {
                    while (true)
                    {
                        var data = new TestData()
                        {
                            Data1 = 876
                        };

                        zmq.Publish("B", data);

                        Thread.Sleep(1000);
                    }
                });
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

    internal class TestData
    {
        public int Data1 { get; set; }
    }
}
