namespace Osprey
{
	internal interface IMessage
	{
		string Service { get; }

		string Endpoint { get; }
	}

	public class EventMessage<T> : IMessage
	{
		public string Service { get; set; }

		public string Endpoint { get; set; }

		public T Payload { get; set; }
	}

	public class RequestMessage<T> : IMessage
	{
		public string Service { get; set; }

		public string Endpoint { get; set; }

		public Node Sender { get; set; }

		public T Payload { get; set; }
	}
}