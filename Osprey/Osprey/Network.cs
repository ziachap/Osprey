using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Osprey
{
	public class Network
	{
		public TResponse Send<TRequest, TResponse>(IMessage<TRequest> message)
		{
			switch (message)
			{
				case HttpMessage<TRequest, TResponse> httpMessage:
					return HttpRequest(httpMessage);
				default:
					throw new Exception($"Communication type '{message.GetType().Name}' not supported");
			}
		}

		private TResponse HttpRequest<TRequest, TResponse>(HttpMessage<TRequest, TResponse> httpMessage)
		{
			var service = Osprey.Node.Receiver.FindService(httpMessage.Service);

			switch (httpMessage.RequestType)
			{
				case HttpMessage<TRequest, TResponse>.HttpVerb.GET:
					return Osprey.Http.Get<TResponse>("http://" + service.Address, CancellationToken.None).Result;
				default:
					throw new Exception("HttpVerb not supported");
			}
		}
	}
}
