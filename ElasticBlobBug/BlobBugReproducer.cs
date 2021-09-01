using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Elastic.Apm.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ElasticBlobBug
{
    public class BlobBugReproducer : IHostedService
    {
        private readonly string _connectionString;
        private readonly ITracer _tracer;
        public BlobBugReproducer(IConfiguration config, ITracer tracer)
        {
            _connectionString = config["AZURE_STORAGE_CONNECTION_STRING"];
            _tracer = tracer;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            const string containerName = "data-storage";
            var client = new BlockBlobClient(_connectionString, containerName, "test1/test2/test3/testblob");
            var testBytes = Encoding.UTF8.GetBytes("{\"test\": \"test\"}");

            var transaction = _tracer.StartTransaction("Blob Tests", "Sample");
            
            var seekableStream = new MemoryStream(testBytes);
            var seekableSpan = transaction.StartSpan("Seekable Test Blob Upload", "Sample");
            await client.UploadAsync(seekableStream, new BlobUploadOptions());
            // No errors logged ^
            seekableSpan.End();
            
            var unSeekableStream = new UnSeekableStream(testBytes);
            var unSeekableSpan = transaction.StartSpan("Un-seekable Test Blob Upload", "Sample");
            await client.UploadAsync(unSeekableStream, new BlobUploadOptions());
            // Fail error logged ^
            unSeekableSpan.End();

            var seekableDownloadStream = new MemoryStream();
            var seekableDownloadSpan = transaction.StartSpan("Seekable Test Blob Download", "Sample");
            await client.DownloadToAsync(seekableDownloadStream);
            // Fail error logged ^
            seekableDownloadSpan.End();
            
            var unSeekableDownloadStream = new UnSeekableStream();
            var unSeekableDownloadSpan = transaction.StartSpan("Un-seekable Test Blob Download", "Sample");
            await client.DownloadToAsync(unSeekableDownloadStream);
            // Fail error logged ^
            unSeekableDownloadSpan.End();

            transaction.End();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}