namespace FTM.Lib.Feeds;

public static class FeedHttpClient
{
    public static async Task<string> ReadString(Uri url, HttpClient httpClient)
    {
        using var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return content;
    }
}