using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace FTM.Lib.Mastodon;

public class MastodonClient : IMastodonClient
{
    public async Task<string> PostStatus(MastodonStatus status, WorkerContext context,
        CancellationToken cancellationToken)
    {
        // https://docs.joinmastodon.org/methods/statuses/

        var uri = new Uri($"{context.Configuration.MastodonServer}/api/v1/statuses");

        var request = CreateStatusRequest(uri, status, context.Configuration.MastodonAccessToken);
        return await SendRequest(request, context.HttpClient, cancellationToken);
    }

    internal static HttpRequestMessage CreateStatusRequest(Uri uri,
        MastodonStatus status, string mastodonAccessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, uri)
            { Content = GetStatusContent(status) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", mastodonAccessToken);
        request.Headers.Add("Idempotency-Key", CreateIdempotencyKey(status));

        return request;
    }

    internal static string CreateIdempotencyKey(MastodonStatus status)
    {
        var raw = string.Join("|", status.Status, status.Language, status.Visibility);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        var base64 = Convert.ToBase64String(hash);
        return base64;
    }

    internal static FormUrlEncodedContent GetStatusContent(MastodonStatus status)
    {
        var fields = new Dictionary<string, string>
        {
            { "status", status.Status }
        };

        if (status.Language is not null)
        {
            fields.Add("language", status.Language);
        }

        if (status.Visibility is not null)
        {
            fields.Add("visibility", Map(status.Visibility.Value));
        }

        return new FormUrlEncodedContent(fields);

        string Map(MastodonStatusVisibility visibility)
        {
            return visibility switch
            {
                MastodonStatusVisibility.Public => "public",
                MastodonStatusVisibility.Unlisted => "unlisted",
                MastodonStatusVisibility.Private => "private",
                MastodonStatusVisibility.Direct => "direct",
                _ => throw new ArgumentOutOfRangeException(nameof(visibility), visibility, null)
            };
        }
    }

    private static async Task<string> SendRequest(HttpRequestMessage request,
        HttpClient httpClient, CancellationToken cancellationToken)
    {
        using var response = await httpClient.SendAsync(request, cancellationToken);

        HandleRateLimit(response);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Failed to post status: {content}");
        }

        var postStatusResponse = await response.Content.ReadFromJsonAsync<PostStatusResponse>(cancellationToken);
        return postStatusResponse!.Id;
    }

    private static void HandleRateLimit(HttpResponseMessage response)
    {
        if (response.StatusCode != HttpStatusCode.TooManyRequests) // 429 Too Many Requests
        {
            return;
        }

        var limit = int.Parse(response.Headers.GetValues("X-Ratelimit-Limit").First());
        var remaining = int.Parse(response.Headers.GetValues("X-Ratelimit-Remaining").First());
        var reset = DateTime.Parse(response.Headers.GetValues("X-Ratelimit-Reset").First(),
            CultureInfo.InvariantCulture);

        throw new RateLimitException(limit, remaining, reset);
    }
}