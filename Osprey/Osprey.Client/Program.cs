using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Nancy;
using Nancy.Owin;
using Nancy.TinyIoc;

namespace Osprey.Client
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("========== OSPREY CLIENT ==========");

			using (Osprey.Default())
			using (Osprey.Join("osprey.client"))
			{
                var host = new WebHostBuilder()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseKestrel()
                    .UseStartup<Startup>()
                    .UseUrls("http://" + Osprey.Node.Info.TcpAddress)
                    .Build();
                
                Task.Factory.StartNew(host.Run, TaskCreationOptions.LongRunning);

                Osprey.Node.RegisterEndpoint(new MultiCastHandler<string>("multitest", msg =>
                {
                    Console.WriteLine($"I just received on multicast: {msg}");
                }));

                StartSendingMultiCast();
                StartSendingHttp();

                while (true)
				{
					Thread.Sleep(1000);
				}
			}
		}
        private static void StartSendingMultiCast()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        var task = Osprey.Network.Send<string>(new MultiCastMessage<string>
                        {
                            Service = "osprey.client",
                            Endpoint = "multitest",
                            Payload = "oranges"
                        });

                        task.Wait();

                        Console.WriteLine("Sent a multicast message");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    Thread.Sleep(3000);
                }
            }, TaskCreationOptions.LongRunning);
        }

        private static void StartSendingHttp()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        var task = Osprey.Network.Send<string, string>(new HttpMessage<string, string>
                        {
                            Service = "osprey.client",
                            Endpoint = "test",
                            Payload = "apples",
                            RequestType = HttpVerb.GET
                        });

                        task.Wait();

                        var response = task.Result;

                        Console.WriteLine("RESPONSE: " + response);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    Thread.Sleep(3000);
                }
            }, TaskCreationOptions.LongRunning);
        }
    }

    public class Startup
	{
        public void Configure(IApplicationBuilder app)
        {
            app.UseOwin(x => x.UseNancy(opt => opt.Bootstrapper = new Bootstrapper()));
        }
    }

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
        }
    }
}
