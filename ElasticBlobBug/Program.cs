using System;
using System.Threading.Tasks;
using Elastic.Apm.Azure.Storage;
using Elastic.Apm.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ElasticBlobBug
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                    builder.SetBasePath(System.IO.Directory.GetParent(AppContext.BaseDirectory)?.FullName)
                        .AddJsonFile("appSettings.json", true);
                })
                .UseElasticApm(new AzureBlobStorageDiagnosticsSubscriber())
                .ConfigureServices((_, services) =>
                {
                    services.AddHostedService<BlobBugReproducer>();
                });
            
            return hostBuilder;
        }
    }
}