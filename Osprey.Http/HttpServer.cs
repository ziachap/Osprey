using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Owin;
using Nancy.Routing;
using Nancy.TinyIoc;
using Osprey.Utilities;

namespace Osprey.Http
{
    public class HttpServer<TStartup> : IDisposable where TStartup : class
    {
        private readonly IWebHost _webHost;

        public HttpServer(string name)
        {
            var url = "http://" + Address.GenerateTcpEndpoint();

            _webHost = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                .UseStartup<TStartup>()
                .UseUrls(url)
                .Build();

            _webHost.RunAsync();
            
            Osprey.Node.Register(new HttpService(name, url));
        }

        public void Dispose()
        {
            _webHost?.Dispose();
        }
    }

    public class DefaultStartup<TBootstrapper> where TBootstrapper : INancyBootstrapper, new()
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseOwin(x => x.UseNancy(opt => opt.Bootstrapper = new TBootstrapper()));
        }
    }
}
