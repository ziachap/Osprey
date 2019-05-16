using System;
using System.Collections.Generic;
using System.Text;
using Nancy;

namespace Osprey.Client.Api
{
	public class Test : NancyModule
	{
		public Test()
		{
			Get("/test", parameters =>
			{
				return Response.AsJson("This is " + Osprey.Node.Info.Id);
			});
		}
	}
}
