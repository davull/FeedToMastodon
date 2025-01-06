using System.Net;
using System.Net.Http.Headers;

namespace FTM.Lib.Feeds;

public static class FeedHttpClient
{
    public record ReadStringResult(bool ContentHasChanged, string Content, string? ETag);

    private const string ETagHeader = "ETag";
    private const string IfNoneMatchHeader = "If-None-Match";

    public static async Task<ReadStringResult> ReadString(
        Uri url, HttpClient httpClient, string? etag, CancellationToken cancellationToken)
    {
        if (!IsValidETag(etag))
        {
            etag = null;
        }

        using var request = CreateRequest();
        using var response = await httpClient.SendAsync(request, cancellationToken);

        var newEtag = GetETag(response.Headers);

        if (response.StatusCode == HttpStatusCode.NotModified)
        {
            return new ReadStringResult(false, string.Empty, newEtag);
        }

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return new ReadStringResult(true, content, newEtag);

        HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = url,
                Headers = { { IfNoneMatchHeader, etag } }
            };
        }

        string? GetETag(HttpHeaders headers)
        {
            return headers.TryGetValues(ETagHeader, out var values)
                ? values.FirstOrDefault()
                : null;
        }
    }

    internal static bool IsValidETag(string? etag)
    {
        if (string.IsNullOrEmpty(etag))
        {
            return false;
        }

        if ((etag.StartsWith('"') || etag.StartsWith("W/\"")) && etag.EndsWith('"'))
        {
            return true;
        }

        return false;
    }
}