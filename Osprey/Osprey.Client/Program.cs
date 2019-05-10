using System;
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
				Console.WriteLine("Running...");

                Console.ReadKey();

				while (true)
				{
					Thread.Sleep(1000);
				}
			}
		}
	}
}
