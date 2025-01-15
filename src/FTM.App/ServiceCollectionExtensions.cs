using FTM.Lib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FTM.App;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterDependencies(this IServiceCollection services)
    {
        var appConfiguration = GetAppConfiguration();
        var loggerFactory = LoggerFactoryProvider.Create(appConfiguration);

        services.AddSingleton(loggerFactory);

        return services;
    }

    public static IServiceCollection RegisterWorkers(this IServiceCollection services)
    {
        services.AddHostedService<StatisticsBackgroundService>();

        return services;
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