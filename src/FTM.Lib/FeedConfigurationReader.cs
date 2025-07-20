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
        var parser = new StreamIniDataParser(new FeedConfigurationIniParser());
        parser.Parser.Configuration.AllowDuplicateKeys = true;
        parser.Parser.Configuration.ConcatenateDuplicateKeys = true;
        return parser.ReadData(reader);
    }

    private static FeedConfiguration MapSection(SectionData section)
    {
        var title = section.SectionName;
        var feedUri = ReadFeedUrl();
        var summarySeparator = ReadSummarySeparators();
        var tags = ReadTags();
        var mastodonServer = ReadRequiredProperty("mastodon_server");
        var mastodonAccessToken = ReadRequiredProperty("mastodon_access_token");
        var workerLoopDelay = ReadWorkerLoopDelay();

        return new FeedConfiguration(title, feedUri, summarySeparator, tags,
            mastodonServer.TrimEnd('/'), mastodonAccessToken, workerLoopDelay);

        string ReadRequiredProperty(string key)
        {
            var value = section.Keys[key];
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"{key} is required");
            }

            return value;
        }

        Uri ReadFeedUrl()
        {
            var rawFeedUri = ReadRequiredProperty("feed_url");

            if (!Uri.TryCreate(rawFeedUri, UriKind.Absolute, out var parsedFeedUri))
            {
                throw new ArgumentException($"feed_url is not a valid URI, got {rawFeedUri}");
            }

            return parsedFeedUri;
        }

        string[] ReadSummarySeparators()
        {
            var sep = section.Keys["summary_separator"] ?? string.Empty;
            return sep
                .Split(FeedConfigurationIniParser.ConcatenateSeparator)
                .Select(s => s.Replace("\\n", "\n"))
                .ToArray();
        }

        string[] ReadTags()
        {
            var allTags = section.Keys["tags"] ?? string.Empty;
            return allTags
                .Split(FeedConfigurationIniParser.ConcatenateSeparator)
                .SelectMany(t => t.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
        }

        TimeSpan? ReadWorkerLoopDelay()
        {
            var rawWorkerLoopDelay = section.Keys["worker_loop_delay"] ?? string.Empty;

            return TimeSpan.TryParse(rawWorkerLoopDelay, CultureInfo.InvariantCulture, out var delay)
                ? delay
                : null;
        }
    }
}