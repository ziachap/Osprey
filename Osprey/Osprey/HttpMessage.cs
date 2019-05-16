namespace Osprey
{
    public class HttpMessage<T, Y> : RequestMessage<T>
    {
        public HttpVerb RequestType { get; set; }
    }

    public enum HttpVerb
    {
        GET,
        POST,
        PUT,
        DELETE
    }
}