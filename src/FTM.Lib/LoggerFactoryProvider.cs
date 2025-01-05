using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace FTM.Lib;

public static class LoggerFactoryProvider
{
    public static ILoggerFactory Create(IConfiguration appConfiguration)
    {
        var minimumLogLevel = MapLogLevel(GetLogLevel(appConfiguration));

        return LoggerFactory.Create(builder =>
        {
            const string template =
                "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}";

            var logFilePath = Path.Combine(AppContext.BaseDirectory, "logs", "log-.txt");

            var logger = new LoggerConfiguration()
                .MinimumLevel.Is(minimumLogLevel)
                .WriteTo.Console(
                    theme: AnsiConsoleTheme.Code,
                    outputTemplate: template)
                .WriteTo.File(logFilePath,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: template)
                .CreateLogger();

            builder.AddSerilog(logger, dispose: true);
        });
    }

    private static LogLevel GetLogLevel(IConfiguration appConfiguration)
    {
        var value = appConfiguration.GetValue<LogLevel?>("Logging:LogLevel:Default");
        return value ?? LogLevel.Information;
    }

    private static LogEventLevel MapLogLevel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Critical => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }
}