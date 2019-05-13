using System.Threading;
using System.Threading.Tasks;
using Osprey.Communication;

namespace Osprey
{
    public class Broadcaster
	{
		private readonly UdpChannel _channel;
		private readonly NodeInfo _info;

		public Broadcaster(UdpChannel channel, NodeInfo info)
		{
			_channel = channel;
			_info = info;
		}

		public void Start()
		{
			Task.Factory.StartNew(() =>
			{
				while (true)
				{
					_channel.Send(_info);
					Thread.Sleep(1000);
				}
			}, TaskCreationOptions.LongRunning);
		}
	}
}