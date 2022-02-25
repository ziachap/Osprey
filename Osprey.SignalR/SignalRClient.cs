using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Osprey.Utilities;

namespace Osprey.SignalR
{
    public class SignalRClient
    {
        private readonly string _node;
        private readonly string _service;
        private readonly string _url;
        private readonly int _retryMilliseconds;
        private readonly Action<IHubConnectionBuilder> _builder;

        private HubConnection _connection;

        public event Func<Exception, Task> Closed;
        public event Func<Exception, Task> Reconnecting;
        public event Func<string, Task> Reconnected;

        public SignalRClient(string node, string service, int retryMilliseconds = 3000, Action<IHubConnectionBuilder> builder = null)
        {
            _node = node;
            _service = service;
            _retryMilliseconds = retryMilliseconds;
            _builder = builder;

            BuildConnection();
        }

        private async Task BuildConnection()
        {
            var url = OSPREY.Network.Locate(_node).FindService(_service).Address;
            
            while (string.IsNullOrEmpty(url))
            {
                await Task.Delay(_retryMilliseconds);
                url = OSPREY.Network.Locate(_node).FindService(_service)?.Address;
            }

            var builder = new HubConnectionBuilder().WithUrl(_url);

            _builder(builder);

            _connection = builder.Build();

            _connection.Closed += ex => Closed?.Invoke(ex);

            _connection.Reconnecting += async ex =>
            {
                if (Reconnecting != null) await Reconnecting.Invoke(ex);
                await Task.Delay(_retryMilliseconds);
                await BuildConnection();
                await StartAsync();
            };

            _connection.Reconnected += msg => Reconnected?.Invoke(msg);
        }

        public void On<T>(string method, Action<T> handler) where T : class
        {
            _connection.On(method, handler);
        }

        public Task InvokeCoreAsync(string method, params object[] args)
        {
            return _connection.InvokeCoreAsync(method, args);
        }

        public Task StartAsync()
        {
            return _connection.StartAsync();
        }
    }

    public class RepeatingRetryPolicy : IRetryPolicy
    {
        private readonly int _milliseconds;

        public RepeatingRetryPolicy(int milliseconds)
        {
            _milliseconds = milliseconds;
        }

        public TimeSpan? NextRetryDelay(RetryContext retryContext)
        {
            return TimeSpan.FromMilliseconds(_milliseconds);
        }
    }
}
