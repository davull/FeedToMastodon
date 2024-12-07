using FluentAssertions;
using NUnit.Framework;

namespace FTM.Lib.Tests;

public class ConfigTests
{
    private readonly Random _random = new(123);

    [TestCase("ftm.sqlite")]
    [TestCase("../ftm.sqlite")]
    [TestCase("../../ftm.sqlite")]
    public void DatabaseName_WithRelativePath_ShouldReturnFullPath(string env)
    {
        Environment.SetEnvironmentVariable(Config.DatabaseNameKey, env);

        Path.IsPathRooted(Config.DatabaseName).Should().BeTrue();
    }

    [Test]
    public void DatabaseName_WithAbsolutePath_ShouldReturnEnv()
    {
        var env = Environment.OSVersion.Platform == PlatformID.Unix
            ? "/home/user/ftm.sqlite"
            : "C:/user/ftm.sqlite";

        Environment.SetEnvironmentVariable(Config.DatabaseNameKey, env);

        Config.DatabaseName.Should().Be(env);
    }

    [TestCase("", false)]
    [TestCase("false", false)]
    [TestCase("False", false)]
    [TestCase("FALSE", false)]
    [TestCase("true", true)]
    [TestCase("True", true)]
    [TestCase("TRUE", true)]
    public void UseMastodonTestClient_Should_ReturnExpected(string env, bool expected)
    {
        Environment.SetEnvironmentVariable(Config.UseMastodonTestClientKey, env);

        Config.UseMastodonTestClient.Should().Be(expected);
    }

    [Test]
    public void WorkerStartDelay_ShouldReturn_BetweenZeroAndHalfLoopDelay()
    {
        for (var i = 0; i < 10_000; i++)
        {
            var loopDelay = TimeSpan.FromMinutes(_random.Next(1, 10));

            var actual = Config.WorkerStartDelay(loopDelay, _random);

            actual.Should()
                .BeGreaterOrEqualTo(TimeSpan.Zero)
                .And.BeLessThanOrEqualTo(loopDelay / 2);
        }
    }

    [Test]
    public void WorkerStartDelay_ShouldReturn_DifferentValues()
    {
        var loopDelay = TimeSpan.FromMinutes(10);

        var delays = Enumerable.Range(0, 100)
            .Select(_ => Config.WorkerStartDelay(loopDelay, _random))
            .Select(d => d.TotalMilliseconds)
            .ToList();

        delays.Should().OnlyHaveUniqueItems();
    }
}