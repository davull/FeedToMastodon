using System.Net;
using System.Net.Http.Headers;

namespace FTM.Lib.Feeds;

public static class FeedHttpClient
{
    public record ReadStringResult(bool ContentHasChanged, string Content, string? ETag);

    private const string ETagHeader = "ETag";

    public static async Task<ReadStringResult> ReadString(
        Uri url, HttpClient httpClient, string? etag, CancellationToken cancellationToken)
    {
        if (!IsValidETag(etag))
        {
            etag = null;
        }

        using var request = CreateRequest(url, etag);
        using var response = await httpClient.SendAsync(request, cancellationToken);

        var newEtag = GetETag(response.Headers);

        if (response.StatusCode == HttpStatusCode.NotModified)
        {
            return new ReadStringResult(false, string.Empty, newEtag);
        }

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return new ReadStringResult(true, content, newEtag);

        string? GetETag(HttpHeaders headers)
        {
            return headers.TryGetValues(ETagHeader, out var values)
                ? values.FirstOrDefault()
                : null;
        }
    }

    private static HttpRequestMessage CreateRequest(Uri url, string? etag)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.IfNoneMatch.TryParseAdd(etag);
        request.Headers.UserAgent.ParseAdd(HttpClientProvider.UserAgent);

        return request;
    }

    internal static bool IsValidETag(string? etag)
        => EntityTagHeaderValue.TryParse(etag, out _);
}