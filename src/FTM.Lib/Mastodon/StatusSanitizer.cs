using System.Net;
using HtmlAgilityPack;

namespace FTM.Lib.Mastodon;

public static class StatusSanitizer
{
    public static string? Sanitize(string? content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return content;
        }

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(content);

        return WebUtility.HtmlDecode(htmlDocument.DocumentNode.InnerText).Trim();
    }
}