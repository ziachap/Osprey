using System;

namespace Osprey
{
    public interface IHandler
    {
        string Endpoint { get; }

        void Handle(string serialized);
    }

    public class MultiCastHandler<T> : IHandler
    {
        private readonly Action<MultiCastMessage<T>> _handler;

        public MultiCastHandler(string endpoint, Action<MultiCastMessage<T>> handler)
        {
            Endpoint = endpoint;
            _handler = handler;
        }

        public string Endpoint { get; }

        public void Handle(string serialized)
        {
            var message = Osprey.Serializer.Deserialize<MultiCastMessage<T>>(serialized);
            _handler(message);
        }
    }
}
