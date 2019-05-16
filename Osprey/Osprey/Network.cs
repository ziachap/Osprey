using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Osprey
{
	public class Network
	{
		public Task<TResponse> Send<TRequest, TResponse>(IMessage message)
		{
			switch (message)
			{
				case HttpMessage<TRequest, TResponse> httpMessage:
					return HttpRequest(httpMessage);
                default:
					throw new Exception($"Communication type '{message.GetType().Name}' not supported for request/response");
			}
		}

		private Task<TResponse> HttpRequest<TRequest, TResponse>(HttpMessage<TRequest, TResponse> httpMessage)
		{
			var service = Osprey.Node.Receiver.FindService(httpMessage.Service);
            var endpoint = $"http://{service.TcpAddress}/{httpMessage.Endpoint}";

            switch (httpMessage.RequestType)
			{
				case HttpVerb.GET:
					return Osprey.Http.Get<TResponse>(endpoint, CancellationToken.None);
				default:
					throw new Exception("HttpVerb not supported");
			}
		}

        public Task Send<TMessage>(IMessage message)
        {
            switch (message)
            {
                case MultiCastMessage<TMessage> multiCastMessage:
                    return SendMultiCast(multiCastMessage);
                default:
                    throw new Exception($"Communication type '{message.GetType().Name}' not supported form transmission");
            }
        }

        private Task SendMultiCast<TMessage>(MultiCastMessage<TMessage> multiCastMessage)
        {
            var service = Osprey.Node.Receiver.FindService(multiCastMessage.Service);
            var endpoint = service.UdpAddress;

            // TODO: Have some UDP channel before we get here, then send here

            return Task.Run(() => { });
        }
    }
}
