using FTM.Lib.Mastodon;

namespace FTM.Lib.Tests.Mastodon;

public class LinkLengthProviderTests
{
    [Test]
    public void GetRelevantLength_NullLink_Should_ReturnLength0()
    {
        Uri? link = null;

        var actual = LinkLengthProvider.GetRelevantLength(link);
        actual.ShouldBe(0);
    }

    [TestCase("https://www.google.de")]
    [TestCase("https://www.google.de?q=123")]
    [TestCase("https://www.google.de?q=123&lang=en")]
    [TestCase("https://www.google.de?q=123&lang=en#top")]
    public void GetRelevantLength_NotNullLink_ShouldReturn23(string uri)
    {
        var link = new Uri(uri);

        var actual = LinkLengthProvider.GetRelevantLength(link);
        actual.ShouldBe(23);
    }
}