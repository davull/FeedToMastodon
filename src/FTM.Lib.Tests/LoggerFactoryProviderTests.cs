using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FTM.Lib.Tests;

public class LoggerFactoryProviderTests
{
    [Test]
    public void Should_Create_Logger()
    {
        var factory = LoggerFactoryProvider.Create(CreateConfiguration(LogLevel.Information));
        var logger = factory.CreateLogger("MyCategory");

        logger.ShouldNotBeNull();
    }

    [TestCase(LogLevel.Debug)]
    [TestCase(LogLevel.Information)]
    [TestCase(LogLevel.Warning)]
    [TestCase(LogLevel.Error)]
    [TestCase(LogLevel.Critical)]
    public void Should_Respect_LogLevel(LogLevel logLevel)
    {
        var appConfig = CreateConfiguration(logLevel);
        var factory = LoggerFactoryProvider.Create(appConfig);
        var logger = factory.CreateLogger("MyCategory");

        var disabledLogLevels = Enum.GetValues<LogLevel>()
            .Where(l => l < logLevel)
            .Except([LogLevel.None]);
        var enabledLogLevels = Enum.GetValues<LogLevel>()
            .Where(l => l >= logLevel)
            .Except([LogLevel.None]);

        foreach (var level in disabledLogLevels)
        {
            logger.IsEnabled(level).ShouldBeFalse();
        }

        foreach (var level in enabledLogLevels)
        {
            logger.IsEnabled(level).ShouldBeTrue();
        }
    }

    [Test]
    public void Trace_Should_Log_Everything()
    {
        var appConfig = CreateConfiguration(LogLevel.Trace);
        var factory = LoggerFactoryProvider.Create(appConfig);
        var logger = factory.CreateLogger("MyCategory");

        var enabledLogLevels = Enum.GetValues<LogLevel>()
            .Except([LogLevel.None]);

        foreach (var level in enabledLogLevels)
        {
            logger.IsEnabled(level).ShouldBeTrue();
        }
    }

    [Test]
    public void Should_FallbackToInformation_WoConfiguration()
    {
        var factory = LoggerFactoryProvider.Create(CreateConfiguration(null));
        var logger = factory.CreateLogger("MyCategory");

        logger.IsEnabled(LogLevel.Debug).ShouldBeFalse();
        logger.IsEnabled(LogLevel.Information).ShouldBeTrue();
    }

    private static IConfiguration CreateConfiguration(LogLevel? logLevel)
    {
        var builder = new ConfigurationBuilder();

        if (logLevel.HasValue)
        {
            builder.AddInMemoryCollection([
                new KeyValuePair<string, string?>("Logging:LogLevel:Default", $"{logLevel}")
            ]);
        }

        return builder.Build();
    }
}