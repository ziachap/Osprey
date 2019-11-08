﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Osprey
{/*
	public class Network
	{
		public Task<TResponse> Send<TRequest, TResponse>(IMessage message)
		{
			switch (message)
			{
				case HttpMessage<TRequest, TResponse> httpMessage:
					return HttpRequest(httpMessage);
                default:
					throw new NotSupportedException($"Communication type '{message.GetType().Name}' not supported for request/response communication");
			}
		}

		private Task<TResponse> HttpRequest<TRequest, TResponse>(HttpMessage<TRequest, TResponse> httpMessage)
        {
            var url = Osprey.Node.Receiver
                .Locate(httpMessage.Service)
                .Service<HttpService>(httpMessage.Host)
                .Url;
            var endpoint = $"http://{url}/{httpMessage.Endpoint}";

            switch (httpMessage.RequestType)
			{
				case HttpVerb.GET:
					return Osprey.Http.Get<TResponse>(endpoint, CancellationToken.None);
                case HttpVerb.POST:
                    return Osprey.Http.Post<TRequest, TResponse>(endpoint, httpMessage.Payload, CancellationToken.None);
                case HttpVerb.PUT:
                    return Osprey.Http.Put<TRequest, TResponse>(endpoint, httpMessage.Payload, CancellationToken.None);
                case HttpVerb.DELETE:
                    return Osprey.Http.Delete<TResponse>(endpoint, CancellationToken.None);
                default:
					throw new NotSupportedException("HttpVerb not supported");
			}
		}

        public Task Send<TMessage>(IMessage message)
        {
            switch (message)
            {
                case MultiCastMessage<TMessage> multiCastMessage:
                    return SendMultiCast(multiCastMessage);
                default:
                    throw new NotSupportedException($"Communication type '{message.GetType().Name}' not supported for one-way communication");
            }
        }

        private Task SendMultiCast<TMessage>(MultiCastMessage<TMessage> multiCastMessage)
        {
            var service = Osprey.Node.Receiver.FindService(multiCastMessage.Service);
            var endpoint = service.UdpAddress;

            // TODO: Have some UDP channel before we get here, then send here

            return Task.Run(() => { });
        }
    }*/
}
