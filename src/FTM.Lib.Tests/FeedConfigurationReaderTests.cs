using FluentAssertions;
using NUnit.Framework;
using Snapshooter.NUnit;

namespace FTM.Lib.Tests;

public class FeedConfigurationReaderTests : TestBase
{
    [Test]
    public void InvalidConfigFile_Should_Throw()
    {
        var filePath = FilePath("invalid-feedconfig.ini");

        var action = () => FeedConfigurationReader.ReadConfiguration(filePath);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*mastodon_server*");
    }

    [Test]
    public void InvalidUrl_Should_Throw()
    {
        var filePath = FilePath("invalid-url-feedconfig.ini");

        var action = () => FeedConfigurationReader.ReadConfiguration(filePath);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*feed_url is not a valid URI*");
    }

    [Test]
    public void EmptyConfigFile_Should_ReturnEmptyList()
    {
        var filePath = FilePath("empty-feedconfig.ini");

        var actual = FeedConfigurationReader.ReadConfiguration(filePath);

        actual.Should().BeEmpty();
    }

    [Test]
    public void InvalidWorkerLoopDelay_Should_ReturnNullTimeSpan()
    {
        var filePath = FilePath("invalid-workerloopdelay-feedconfig.ini");

        var actual = FeedConfigurationReader.ReadConfiguration(filePath);

        actual.Should().NotBeNullOrEmpty();
        actual.Should().AllSatisfy(
            config => config.WorkerLoopDelay.Should().BeNull());
    }

    [Test]
    public void ValidConfigFile_Should_ReturnConfig()
    {
        var filePath = FilePath("valid-feedconfig.ini");

        var actual = FeedConfigurationReader.ReadConfiguration(filePath);

        actual.Should().NotBeNullOrEmpty();
        actual.Should().MatchSnapshot();
    }

    [Test]
    public void LargeWorkerLoopDelay_Should_ReturnConfig()
    {
        var filePath = FilePath("valid-large-loop-delay.ini");

        var actual = FeedConfigurationReader.ReadConfiguration(filePath);

        actual.Should().NotBeNullOrEmpty();

        actual.Should().AllSatisfy(
            config => config.WorkerLoopDelay.Should().NotBeNull());
    }

    private static string FilePath(string fileName) =>
        Path.Combine(PathHelper.GetCurrentFileDirectory(),
            "TestData", fileName);
}