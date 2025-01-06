using System.Globalization;

namespace FTM.Lib;

public static class TimeSpanParser
{
    public static TimeSpan? Parse(string? raw)
    {
        return TimeSpan.TryParseExact(raw, @"hh\:mm\:ss",
            CultureInfo.InvariantCulture, TimeSpanStyles.None, out var delay)
            ? delay
            : null;
    }
}