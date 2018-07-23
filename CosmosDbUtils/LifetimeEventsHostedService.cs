using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.Documents.ChangeFeedProcessor.PartitionManagement;
using Microsoft.Azure.Documents.ChangeFeedProcessor.Logging;
using Microsoft.Azure.Documents.Client;

namespace CosmosDbDiagAndFeed.CosmosDbUtils
{
    public class LifetimeEventsHostedService : IHostedService
    {
        public static ILogger _logger;
        private readonly IApplicationLifetime _appLifetime;
        private readonly IConfiguration _configuration;

        public static string localLogdirectory;

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ChangeFeedProcessorBuilder builder = new ChangeFeedProcessorBuilder();
        private IChangeFeedProcessor iChangeFeedProcessor;

        public LifetimeEventsHostedService(
            IConfiguration configuration, ILogger<LifetimeEventsHostedService> logger, IApplicationLifetime appLifetime)
        {
            _configuration = configuration;
            _logger = logger;
            _appLifetime = appLifetime;

            if (string.IsNullOrEmpty(_configuration["az_local_logs_dir"]))
            {
                localLogdirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(localLogdirectory);
            }
            else
            {
                localLogdirectory = _configuration["az_local_logs_dir"];
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async void OnStarted()
        // private void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called.");

            // Perform post-startup activities here     
            _logger.LogInformation("Log directory is " + localLogdirectory);

            string hostName = Guid.NewGuid().ToString();

            DocumentCollectionInfo documentCollectionInfo = new DocumentCollectionInfo
            {
                Uri = new Uri(_configuration["az_cosmos_uri"]),
                MasterKey = _configuration["az_cosmos_key"],
                DatabaseName = _configuration["az_cosmos_db_name"],
                CollectionName = _configuration["az_cosmos_collection_name"]
            };

            DocumentCollectionInfo leaseCollectionInfo = new DocumentCollectionInfo
            {
                Uri = new Uri(_configuration["az_cosmos_uri"]),
                MasterKey = _configuration["az_cosmos_key"],
                DatabaseName = _configuration["az_cosmos_db_name"],
                CollectionName = _configuration["az_cosmos_lease_collection_name"]
            };

            DocumentFeedObserverFactory docObserverFactory = new DocumentFeedObserverFactory();
            ChangeFeedOptions feedOptions = new ChangeFeedOptions();
            feedOptions.StartFromBeginning = true;

            ChangeFeedProcessorOptions feedProcessorOptions = new ChangeFeedProcessorOptions();
            feedProcessorOptions.LeaseRenewInterval = TimeSpan.FromSeconds(15);

            this.builder
               .WithHostName(hostName)
               .WithFeedCollection(documentCollectionInfo)
               .WithLeaseCollection(leaseCollectionInfo)
               .WithProcessorOptions(feedProcessorOptions)
               .WithObserverFactory(new DocumentFeedObserverFactory());//;
//               .WithObserver<DocumentFeedObserver>();  //or just pass a observer

            iChangeFeedProcessor = await this.builder.BuildAsync();
            await iChangeFeedProcessor.StartAsync();
        }

        private async void OnStopping()
        // private void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called.");

            // Perform on-stopping activities here
            await iChangeFeedProcessor.StopAsync();
        }

        private void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called.");

            // Perform post-stopped activities here
            _logger.LogInformation("Log directory is " + localLogdirectory);

        }
    }
}