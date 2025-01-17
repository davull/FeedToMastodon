namespace FTM.Lib.Tests;

public class WorkerContextTests
{
    [Test]
    public void SetLoopDelay_Should_Change_LoopDelay()
    {
        var sut = Dummies.WorkerContext(Dummies.FeedConfiguration());

        var someDelay = TimeSpan.FromSeconds(99);

        sut.LoopDelay.Should().NotBe(someDelay);

        // Act
        sut.SetLoopDelay(someDelay);

        sut.LoopDelay.Should().Be(someDelay);
    }

    [Test]
    public void ResetLoopDelay_Should_Reset_LoopDelay()
    {
        var initialDelay = TimeSpan.FromSeconds(11);
        var someDelay = TimeSpan.FromSeconds(99);

        var sut = Dummies.WorkerContext(Dummies.FeedConfiguration(), initialDelay);

        sut.LoopDelay.Should().Be(initialDelay);
        sut.LoopDelay.Should().NotBe(someDelay);
        sut.SetLoopDelay(someDelay);
        sut.LoopDelay.Should().Be(someDelay);

        // Act
        sut.ResetLoopDelay();

        sut.LoopDelay.Should().Be(initialDelay);
    }
}