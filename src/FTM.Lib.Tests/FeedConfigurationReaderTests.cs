using FTM.Lib.Tests.Extensions;

namespace FTM.Lib.Tests;

public class FeedConfigurationReaderTests : TestBase
{
    [Test]
    public void InvalidConfigFile_Should_Throw()
    {
        var filePath = FilePath("invalid-feedconfig.ini");

        var action = () => FeedConfigurationReader.ReadConfiguration(filePath);

        action.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("mastodon_server");
    }

    [Test]
    public void InvalidUrl_Should_Throw()
    {
        var filePath = FilePath("invalid-url-feedconfig.ini");

        var action = () => FeedConfigurationReader.ReadConfiguration(filePath);

        action.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("feed_url is not a valid URI");
    }

    [Test]
    public void EmptyConfigFile_Should_ReturnEmptyList()
    {
        var filePath = FilePath("empty-feedconfig.ini");

        var actual = FeedConfigurationReader.ReadConfiguration(filePath);

        actual.ShouldBeEmpty();
    }

    [Test]
    public void InvalidWorkerLoopDelay_Should_ReturnNullTimeSpan()
    {
        var filePath = FilePath("invalid-workerloopdelay-feedconfig.ini");

        var actual = FeedConfigurationReader.ReadConfiguration(filePath);

        actual.ShouldNotBeNullOrEmpty();

        foreach (var config in actual)
        {
            config.WorkerLoopDelay.ShouldBeNull();
        }
    }

    [Test]
    public void ValidConfigFile_Should_ReturnConfig()
    {
        var filePath = FilePath("valid-feedconfig.ini");

        var actual = FeedConfigurationReader.ReadConfiguration(filePath);

        actual.ShouldNotBeNullOrEmpty();
        actual.MatchSnapshot();
    }

    [Test]
    public void LargeWorkerLoopDelay_Should_ReturnConfig()
    {
        var filePath = FilePath("valid-large-loop-delay.ini");

        var actual = FeedConfigurationReader.ReadConfiguration(filePath);

        actual.ShouldNotBeNullOrEmpty();

        foreach (var config in actual)
        {
            config.WorkerLoopDelay.ShouldNotBeNull();
        }

        var snapshot = actual.Select(a => new
            { a.Title, a.WorkerLoopDelay });
        snapshot.MatchSnapshot();
    }

    private static string FilePath(string fileName) =>
        Path.Combine(PathHelper.GetCurrentFileDirectory(),
            "TestData", fileName);
}