using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace FTM.Lib.Tests;

public class LoggerFactoryProviderTests
{
    [Test]
    public void Should_Create_Logger()
    {
        var factory = LoggerFactoryProvider.Create(CreateConfiguration());
        var logger = factory.CreateLogger("MyCategory");

        logger.Should().NotBeNull();
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

        using var _ = new AssertionScope();

        disabledLogLevels.Should()
            .AllSatisfy(l => logger.IsEnabled(l).Should().BeFalse());

        enabledLogLevels.Should()
            .AllSatisfy(l => logger.IsEnabled(l).Should().BeTrue());
    }

    [Test]
    public void Trace_Should_Log_Everything()
    {
        var appConfig = CreateConfiguration(LogLevel.Trace);
        var factory = LoggerFactoryProvider.Create(appConfig);
        var logger = factory.CreateLogger("MyCategory");

        var enabledLogLevels = Enum.GetValues<LogLevel>()
            .Except([LogLevel.None]);

        enabledLogLevels.Should()
            .AllSatisfy(l => logger.IsEnabled(l).Should().BeTrue());
    }

    [Test]
    public void None_Should_Log_Nothing()
    {
        var appConfig = CreateConfiguration(LogLevel.None);
        var factory = LoggerFactoryProvider.Create(appConfig);
        var logger = factory.CreateLogger("MyCategory");

        var logLevels = Enum.GetValues<LogLevel>();

        logLevels.Should()
            .AllSatisfy(l => logger.IsEnabled(l).Should().BeFalse());
    }

    private static IConfiguration CreateConfiguration(LogLevel logLevel = LogLevel.Information)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection([
                new KeyValuePair<string, string?>("Logging:LogLevel:Default", $"{logLevel}")
            ]).Build();
    }
}