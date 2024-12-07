﻿using FluentAssertions;
using NUnit.Framework;

namespace FTM.Lib.Tests;

public class WorkerContextTests
{
    [Test]
    public void SetLoopDelay_Should_Change_LoopDelay()
    {
        var sut = Dummies.WorkerContext(Dummies.FeedConfiguration());

        var someDelay = TimeSpan.FromSeconds(99);

        sut.LoopWaitDelay.Should().NotBe(someDelay);

        // Act
        sut.SetLoopDelay(someDelay);

        sut.LoopWaitDelay.Should().Be(someDelay);
    }

    [Test]
    public void ResetLoopDelay_Should_Reset_LoopDelay()
    {
        var sut = Dummies.WorkerContext(Dummies.FeedConfiguration());

        var someDelay = TimeSpan.FromSeconds(99);
        var initialDelay = sut.LoopWaitDelay;

        sut.LoopWaitDelay.Should().NotBe(someDelay);
        sut.SetLoopDelay(someDelay);
        sut.LoopWaitDelay.Should().Be(someDelay);

        // Act
        sut.ResetLoopDelay();

        sut.LoopWaitDelay.Should().Be(initialDelay);
    }
}