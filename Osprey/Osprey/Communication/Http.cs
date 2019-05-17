using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Osprey.Communication
{
	public interface IHttp
	{
		Task<T> Get<T>(string url, CancellationToken cancellationToken);
		Task<T> Post<TBody, T>(string url, TBody data, CancellationToken cancellationToken);
		Task<T> Put<TBody, T>(string url, TBody data, CancellationToken cancellationToken);
		Task<T> Delete<T>(string url, CancellationToken cancellationToken);
	}

	public class Http : IHttp
	{
		private static HttpClient Client()
		{
			return new HttpClient();
		}

		public async Task<T> Get<T>(string url, CancellationToken cancellationToken)
		{
            var message = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
                Headers = { { "Accept", "application/json" } }
            };

            var response = await Send<T>(message, cancellationToken);

            return response;
        }

		public async Task<T> Post<TBody, T>(string url, TBody data, CancellationToken cancellationToken)
        {
            var message = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = new StreamContent(ToStream(data)),
                Headers = { { "Accept", "application/json" } }
            };

            var response = await Send<T>(message, cancellationToken);

            return response;
        }

		public async Task<T> Put<TBody, T>(string url, TBody data, CancellationToken cancellationToken)
        {
            var message = new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(url),
                Content = new StreamContent(ToStream(data)),
                Headers = { { "Accept", "application/json" } }
            };

            var response = await Send<T>(message, cancellationToken);

            return response;
        }

		public async Task<T> Delete<T>(string url, CancellationToken cancellationToken)
        {
            var message = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Headers = { { "Accept", "application/json" } }
            };

            var response = await Send<T>(message, cancellationToken);

            return response;
        }

        private async Task<T> Send<T>(HttpRequestMessage message, CancellationToken cancellationToken)
        {
            using (var client = Client())
            {
                var requestTask = client.SendAsync(message, cancellationToken);

                HttpResponseMessage response;
                try
                {
                    response = await requestTask;
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }

                var content = await response.Content.ReadAsStringAsync();
                var deserialized = Osprey.Serializer.Deserialize<T>(content);

                return deserialized;
            }
        }

        private Stream ToStream(object data)
        {
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, data);
            return stream;
        }
    }
}
