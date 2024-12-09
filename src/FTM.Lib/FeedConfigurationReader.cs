using System.Globalization;
using IniParser;
using IniParser.Model;

namespace FTM.Lib;

public static class FeedConfigurationReader
{
    public static ICollection<FeedConfiguration> ReadConfiguration(string configFilePath)
    {
        var ini = ReadIni(configFilePath);

        return ini.Sections
            .Select(MapSection)
            .ToList();
    }

    private static IniData ReadIni(string path)
    {
        using var reader = File.OpenText(path);
        var parser = new StreamIniDataParser();
        return parser.ReadData(reader);
    }

    private static FeedConfiguration MapSection(SectionData section)
    {
        var title = section.SectionName;
        var feedUri = ReadRequiredProperty("feed_url");

        if (!Uri.TryCreate(feedUri, UriKind.Absolute, out var parsedFeedUri))
        {
            throw new ArgumentException($"feed_url is not a valid URI, got {feedUri}");
        }

        var summarySeparator = section.Keys["summary_separator"] ?? string.Empty;
        summarySeparator = summarySeparator.Replace("\\n", "\n");

        var mastodonServer = ReadRequiredProperty("mastodon_server");
        var mastodonAccessToken = ReadRequiredProperty("mastodon_access_token");

        var workerLoopDelay = section.Keys["worker_loop_delay"] ?? string.Empty;
        var parsedWorkerLoopDelay = (TimeSpan?)null;

        if (TimeSpan.TryParseExact(workerLoopDelay, @"hh\:mm\:ss",
                CultureInfo.InvariantCulture, TimeSpanStyles.None, out var ts))
        {
            parsedWorkerLoopDelay = ts;
        }

        return new FeedConfiguration(title, parsedFeedUri, summarySeparator,
            mastodonServer.TrimEnd('/'), mastodonAccessToken, parsedWorkerLoopDelay);

        string ReadRequiredProperty(string key)
        {
            var value = section.Keys[key];
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"{key} is required");
            }

            return value;
        }
    }
}