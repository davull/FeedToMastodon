using System.Globalization;
using System.Xml.Linq;

namespace FTM.Lib.Feeds;

public abstract class FeedParserBase
{
    protected const string AtomNs = "http://www.w3.org/2005/Atom";

    public abstract bool CanRead(XDocument document);

    public abstract Feed ParseFeed(XDocument document);

    private static readonly string[] DateTimeFormats =
    [
        "ddd, dd MMM yyyy HH:mm:ss 'UTC'",
        "dd.MM.yyyy",
        "ddd, dd MMM yyyy HH:mm:ss zzz"
    ];

    private static readonly CultureInfo[] DateTimeCultures =
    [
        CultureInfo.CurrentCulture,
        CultureInfo.InvariantCulture,
        CultureInfo.GetCultureInfo("de"),
        CultureInfo.GetCultureInfo("en")
    ];

    internal static DateTimeOffset? TryGetDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, out var date))
        {
            return date;
        }

        if (DateTimeCultures.Any(culture =>
                DateTimeOffset.TryParseExact(value, DateTimeFormats,
                    culture, DateTimeStyles.None, out date)))
        {
            return date;
        }

        return null;
    }
}