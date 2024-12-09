using System.Diagnostics.CodeAnalysis;
using FTM.Lib;
using FTM.Lib.Data;
using FTM.Lib.Mastodon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace FTM.App;

[ExcludeFromCodeCoverage]
public class Application(
    ICollection<FeedConfiguration> feedConfigurations,
    IMastodonClient mastodonClient,
    ILoggerFactory loggerFactory)
{
    public static Application Create()
    {
        var feedConfigurations = FeedConfigurationReader.ReadConfiguration(Config.ConfigFileName);
        var appConfiguration = GetAppConfiguration();
        var mastodonClient = CreateMastodonClient();
        var loggerFactory = CreateLoggerFactory();

        return new Application(feedConfigurations, mastodonClient, loggerFactory);

        ILoggerFactory CreateLoggerFactory()
        {
            return LoggerFactory.Create(builder =>
            {
                builder
                    .AddSimpleConsole(options =>
                    {
                        options.SingleLine = true;
                        options.ColorBehavior = LoggerColorBehavior.Enabled;
                        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                    })
                    .AddConfiguration(appConfiguration.GetSection("Logging"));
            });
        }

        IMastodonClient CreateMastodonClient()
        {
            return Config.UseMastodonTestClient
                ? new MastodonTestClient()
                : new MastodonClient();
        }
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        Database.Initialize();

        Task[] workerTasks =
        [
            ..InitializeFeedWorkers(cancellationToken),
            InitializeStatisticsWorker(cancellationToken)
        ];
        await Task.WhenAll(workerTasks);
    }

    private List<Task> InitializeFeedWorkers(CancellationToken cancellationToken)
    {
        return feedConfigurations
            .Select(CreateWorkerContext)
            .Select(CreateWorker)
            .Select(w => w.Start(cancellationToken))
            .ToList();

        WorkerContext CreateWorkerContext(FeedConfiguration config)
        {
            return new WorkerContext
            {
                Configuration = config,
                Logger = loggerFactory.CreateLogger(config.Title),
                HttpClient = HttpClientProvider.CreateHttpClient()
            };
        }

        Worker CreateWorker(WorkerContext context) => new(context, mastodonClient);
    }

    private Task InitializeStatisticsWorker(CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("Statistics");
        var worker = new StatisticsWorker(logger);
        return worker.Start(cancellationToken);
    }

    private static IConfiguration GetAppConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.local.json", optional: true)
            .Build();
        return configuration;
    }
}