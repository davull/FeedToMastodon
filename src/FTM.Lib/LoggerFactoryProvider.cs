using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace FTM.Lib;

public static class LoggerFactoryProvider
{
    public static ILoggerFactory Create(IConfiguration appConfiguration)
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
}