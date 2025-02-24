using System.Text;

namespace FTM.Lib.Tests;

public class ConfigTests : TestBase
{
    private readonly Random _random = new(123);

    protected override async Task TearDown()
    {
        await base.TearDown();

        Environment.SetEnvironmentVariable(Config.DatabaseNameKey, "");
        Environment.SetEnvironmentVariable(Config.ConfigFileNameKey, "");
        Environment.SetEnvironmentVariable(Config.UseMastodonTestClientKey, "");
    }

    [TestCase("ftm.sqlite")]
    [TestCase("../ftm.sqlite")]
    [TestCase("../../ftm.sqlite")]
    public void DatabaseName_WithRelativePath_ShouldReturnFullPath(string env)
    {
        Environment.SetEnvironmentVariable(Config.DatabaseNameKey, env);

        Path.IsPathRooted(Config.DatabaseName).ShouldBeTrue();
    }

    [Test]
    public void DatabaseName_WithAbsolutePath_ShouldReturnEnv()
    {
        var env = Environment.OSVersion.Platform == PlatformID.Unix
            ? "/home/user/ftm.sqlite"
            : "C:/user/ftm.sqlite";

        Environment.SetEnvironmentVariable(Config.DatabaseNameKey, env);

        Config.DatabaseName.ShouldBe(env);
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

        Config.UseMastodonTestClient.ShouldBe(expected);
    }

    [Test]
    public void WorkerStartDelay_ShouldReturn_BetweenZeroAndHalfLoopDelay()
    {
        for (var i = 0; i < 10_000; i++)
        {
            var loopDelay = TimeSpan.FromMinutes(_random.Next(1, 10));

            var actual = Config.WorkerStartDelay(loopDelay, _random);

            actual.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
            actual.ShouldBeLessThanOrEqualTo(loopDelay / 2);
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

        delays.ShouldBeUnique();
    }

    [Test]
    [Explicit("This test is environment specific.")]
    public void Print_ShouldMatchSnapshot()
    {
        Environment.SetEnvironmentVariable(Config.DatabaseNameKey, "C:/user/ftm.sqlite");
        Environment.SetEnvironmentVariable(Config.ConfigFileNameKey, "C:/user/ftm.ini");
        Environment.SetEnvironmentVariable(Config.UseMastodonTestClientKey, "true");

        var output = new StringBuilder();

        Config.Print(s => output.AppendLine(s));

        output.ToString().MatchSnapshot();
    }
}