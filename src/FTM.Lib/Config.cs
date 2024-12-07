namespace FTM.Lib;

public static class Config
{
    public const string DatabaseNameKey = "FTM_DATABASE_NAME";
    public const string ConfigFileNameKey = "FTM_CONFIG_FILE_NAME";
    public const string UseMastodonTestClientKey = "FTM_USE_MASTODON_TEST_CLIENT";

    public static string DatabaseName => GetFilePath(DatabaseNameKey);

    public static string ConfigFileName => GetFilePath(ConfigFileNameKey);

    public static bool UseMastodonTestClient => GetBoolValue(UseMastodonTestClientKey);

    public static TimeSpan WorkerLoopDelay => TimeSpan.FromMinutes(1);

    public static TimeSpan RateLimitExceptionDelay => TimeSpan.FromMinutes(15);

    public static TimeSpan HttpRequestExceptionDelay => TimeSpan.FromMinutes(10);

    public static TimeSpan HttpClientRetryDelay => TimeSpan.FromSeconds(4);

    private static string GetFilePath(string key)
    {
        var path = GetStringValue(key);
        return Path.IsPathRooted(path) ? path : Path.GetFullPath(path);
    }

    private static bool GetBoolValue(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (bool.TryParse(value, out var result))
        {
            return result;
        }

        throw new InvalidOperationException(
            $"The environment variable '{key}' is not a valid boolean value.");
    }

    private static string GetStringValue(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"The environment variable '{key}' is not set.");
        }

        return value;
    }
}