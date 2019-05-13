using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Osprey.Communication
{
	public interface IHttp
	{
		Task<T> Get<T>(string url, CancellationToken cancellationToken);
		Task<T> Post<T>(string url, CancellationToken cancellationToken);
		Task<T> Put<T>(string url, CancellationToken cancellationToken);
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
			using (var client = Client())
			{
				var requestTask = client.GetAsync(url, cancellationToken);

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

		public Task<T> Post<T>(string url, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<T> Put<T>(string url, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<T> Delete<T>(string url, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
