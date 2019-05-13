using System;

namespace Osprey
{
	public interface IMessage<T>
	{
		string Service { get; }

		string Endpoint { get; }
	}

	public class EventMessage<T> : IMessage<T>
	{
		public string Service { get; set; }

		public string Endpoint { get; set; }

		public T Payload { get; set; }
	}

	public class RequestMessage<T> : IMessage<T>
	{
		public string Service { get; set; }

		public string Endpoint { get; set; }

		public NodeInfo Sender { get; set; }

		public T Payload { get; set; }
	}

	public class HttpMessage<T, Y> : RequestMessage<T>
	{
		public HttpVerb RequestType { get; set; }

		public enum HttpVerb
		{
			GET,
			POST,
			PUT,
			DELETE
		}
	}


}