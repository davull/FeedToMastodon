using FTM.Lib;
using FTM.Lib.Mastodon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FTM.App;

public static class ServiceCollectionExtensions
{
    public static void RegisterDependencies(this IServiceCollection services)
    {
        var appConfiguration = GetAppConfiguration();
        var loggerFactory = LoggerFactoryProvider.Create(appConfiguration);

        services.AddSingleton(loggerFactory);

        services.AddHostedService<StatisticsBackgroundService>();

        var feedWorkers = CreateFeedWorkers(appConfiguration, loggerFactory);
        foreach (var worker in feedWorkers)
        {
            services.AddSingleton<IHostedService, FeedBackgroundWorker>(_ => worker);
        }

        IConfiguration GetAppConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();
            return configuration;
        }
    }

    private static IEnumerable<FeedBackgroundWorker> CreateFeedWorkers(
        IConfiguration appConfiguration, ILoggerFactory loggerFactory)
    {
        var feedConfigurations = FeedConfigurationReader.ReadConfiguration(Config.ConfigFileName);
        var applicationOptions = GetAppOptions();
        var mastodonClient = CreateMastodonClient(loggerFactory);

        return feedConfigurations
            .Select(CreateWorkerContext)
            .Select(CreateWorker);

        WorkerContext CreateWorkerContext(FeedConfiguration config)
        {
            var workerLoopDelay = config.WorkerLoopDelay ?? applicationOptions.DefaultWorkerLoopDelay;
            return new WorkerContext(workerLoopDelay)
            {
                Configuration = config,
                Logger = loggerFactory.CreateLogger(config.Title),
                HttpClient = HttpClientProvider.CreateHttpClient()
            };
        }

        FeedBackgroundWorker CreateWorker(WorkerContext context) => new(context, mastodonClient);

        ApplicationOptions GetAppOptions()
        {
            try
            {
                return appConfiguration.Get<ApplicationOptions>()!;
            }
            catch
            {
                return ApplicationOptions.Default;
            }
        }

        IMastodonClient CreateMastodonClient(ILoggerFactory lf)
        {
            return Config.UseMastodonTestClient
                ? new MastodonTestClient()
                : new MastodonClient(lf.CreateLogger<MastodonClient>());
        }
    }
}