using System;
using Wolfgang.Etl.TestKit.Xunit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Unit tests for <see cref="ManualProgressTimer"/>.
/// </summary>
public class ManualProgressTimerTests
{
    // ------------------------------------------------------------------
    // Fire
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="ManualProgressTimer.Fire"/> raises
    /// <see cref="ManualProgressTimer.Elapsed"/> synchronously.
    /// </summary>
    [Fact]
    public void Fire_raises_Elapsed_event()
    {
        using var timer = new ManualProgressTimer();
        var raised = false;
        timer.Elapsed += () => raised = true;

        timer.Fire();

        Assert.True(raised);
    }

    /// <summary>
    /// Verifies that <see cref="ManualProgressTimer.Fire"/> raises
    /// <see cref="ManualProgressTimer.Elapsed"/> once per call.
    /// </summary>
    [Fact]
    public void Fire_raises_Elapsed_once_per_call()
    {
        using var timer = new ManualProgressTimer();
        var count = 0;
        timer.Elapsed += () => count++;

        timer.Fire();
        timer.Fire();

        Assert.Equal(2, count);
    }

    /// <summary>
    /// Verifies that <see cref="ManualProgressTimer.Fire"/> does not throw
    /// when no handlers are subscribed to <see cref="ManualProgressTimer.Elapsed"/>.
    /// </summary>
    [Fact]
    public void Fire_with_no_subscribers_does_not_throw()
    {
        using var timer = new ManualProgressTimer();

        var exception = Record.Exception(() => timer.Fire());

        Assert.Null(exception);
    }

    /// <summary>
    /// Verifies that <see cref="ManualProgressTimer.Fire"/> throws
    /// <see cref="ObjectDisposedException"/> after the timer has been disposed.
    /// </summary>
    [Fact]
    public void Fire_after_dispose_throws_ObjectDisposedException()
    {
        var timer = new ManualProgressTimer();
        timer.Dispose();

        Assert.Throws<ObjectDisposedException>(() => timer.Fire());
    }



    // ------------------------------------------------------------------
    // Start
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="ManualProgressTimer.Start"/> is a no-op
    /// and does not throw.
    /// </summary>
    [Fact]
    public void Start_is_noop_and_does_not_throw()
    {
        using var timer = new ManualProgressTimer();

        var exception = Record.Exception(() => timer.Start(1000));

        Assert.Null(exception);
    }



    // ------------------------------------------------------------------
    // StopTimer
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="ManualProgressTimer.StopTimer"/> is a no-op
    /// and does not throw.
    /// </summary>
    [Fact]
    public void StopTimer_is_noop_and_does_not_throw()
    {
        using var timer = new ManualProgressTimer();

        var exception = Record.Exception(() => timer.StopTimer());

        Assert.Null(exception);
    }



    // ------------------------------------------------------------------
    // Dispose
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="ManualProgressTimer.Dispose"/> clears all
    /// <see cref="ManualProgressTimer.Elapsed"/> subscribers.
    /// </summary>
    [Fact]
    public void Dispose_clears_Elapsed_subscribers()
    {
        var timer = new ManualProgressTimer();
        var raised = false;
        timer.Elapsed += () => raised = true;

        timer.Dispose();

        Assert.Throws<ObjectDisposedException>(() => timer.Fire());
        Assert.False(raised);
    }

    /// <summary>
    /// Verifies that <see cref="ManualProgressTimer.Dispose"/> can be called
    /// multiple times without throwing.
    /// </summary>
    [Fact]
    public void Dispose_can_be_called_multiple_times()
    {
        var timer = new ManualProgressTimer();

        timer.Dispose();
        var exception = Record.Exception(() => timer.Dispose());

        Assert.Null(exception);
    }
}
