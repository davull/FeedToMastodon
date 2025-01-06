using System.Globalization;

namespace FTM.Lib;

public static class TimeSpanParser
{
    public static TimeSpan? Parse(string? raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return null;
        }

        var parts = raw.Split(':');
        if (parts.Length != 3)
        {
            return null;
        }

        var hh = int.Parse(parts[0], CultureInfo.InvariantCulture);
        var mm = int.Parse(parts[1], CultureInfo.InvariantCulture);
        var ss = int.Parse(parts[2], CultureInfo.InvariantCulture);

        return new TimeSpan(hh, mm, ss);
    }
}