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

				while (true)
				{
					Thread.Sleep(1000);
				}
			}
		}
	}
}
