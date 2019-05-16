using System;

namespace Osprey
{
	public interface IMessage
	{
		string Service { get; }

		string Endpoint { get; }
	}

    internal sealed class EmptyMessage : IMessage
    {
        public string Service { get; set; }

        public string Endpoint { get; set; }
    }

    public class MultiCastMessage<T> : IMessage
	{
		public string Service { get; set; }

		public string Endpoint { get; set; }

		public T Payload { get; set; }
	}

	public class RequestMessage<T> : IMessage
	{
		public string Service { get; set; }

		public string Endpoint { get; set; }

        public NodeInfo Sender => Osprey.Node.Info;

		public T Payload { get; set; }
	}
}
