using Nancy;

namespace Osprey.Demo.Client
{
	public class TestApi : NancyModule
	{
		public TestApi()
		{
			Get("/test", parameters =>
			{
				return Response.AsJson("This is " + OSPREY.Network.Node.Info.NodeId);
			});
		}
	}
}
