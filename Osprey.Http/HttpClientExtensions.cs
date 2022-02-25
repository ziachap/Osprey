using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Osprey.Http
{
    public static class HttpClientExtensions
	{
        public static ServiceInfo Http(this NodeInfo node)
        {
            return node.Services.SingleOrDefault(x => x.Type == "http")
                                 ?? throw new Exception("Located node does not contain a HTTP server");
        }

		public static Task<TResponse> Send<TRequest, TResponse>(this ServiceInfo serviceInfo, HttpMessage<TRequest, TResponse> message)
        {
            var url = $"http://{serviceInfo.Address}/{message.Endpoint}";

            switch (message.RequestType)
			{
				case HttpVerb.GET:
					return HttpOperations.Get<TResponse>(url, CancellationToken.None);
                case HttpVerb.POST:
                    return HttpOperations.Post<TRequest, TResponse>(url, message.Payload, CancellationToken.None);
                case HttpVerb.PUT:
                    return HttpOperations.Put<TRequest, TResponse>(url, message.Payload, CancellationToken.None);
                case HttpVerb.DELETE:
                    return HttpOperations.Delete<TResponse>(url, CancellationToken.None);
                default:
					throw new NotSupportedException("HttpVerb not supported");
			}
		}
    }
}
