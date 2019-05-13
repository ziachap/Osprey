using System;
using System.IO;
using System.Threading;
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
				var url = "http://" + Osprey.Node.Info.Address;

				var host = new WebHostBuilder()
					.UseKestrel()
					.UseContentRoot(Directory.GetCurrentDirectory())
					.UseIISIntegration()
					.UseStartup<Startup>()
					.Build();

				host.Run();

				Console.WriteLine("Running on " + url);
					Console.ReadKey();

					while (true)
					{
						Thread.Sleep(1000);
					}
				}
		}
	}

	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			app.UseCors(CorsOptions.AllowAll);
			app.UseNancy();
		}
	}
}
