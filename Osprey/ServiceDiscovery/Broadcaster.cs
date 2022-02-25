using System;
using System.Threading;
using System.Threading.Tasks;
using Osprey.Communication;

namespace Osprey.ServiceDiscovery
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
                    var serialized = OSPREY.Network.Serializer.Serialize(_info);
					_channel.Send(serialized);
					Thread.Sleep(1000);
				}
			}, TaskCreationOptions.LongRunning);
		}
    }
}