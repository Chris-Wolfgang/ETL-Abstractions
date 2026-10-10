using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

/// <summary>
/// Pins the members of the <see cref="CountingProgressTimer"/> double that the duplicate-subscription
/// tests do not reach on their own.
/// </summary>
public class CountingProgressTimerTests
{
    [Fact]
    public void Start_when_called_neither_raises_Elapsed_nor_counts_as_StopTimer_or_Dispose()
    {
        using var timer = new CountingProgressTimer();
        var raised = 0;
        timer.Elapsed += () => raised++;

        timer.Start(10);

        Assert.Equal(0, raised);
        Assert.Equal(0, timer.StopTimerCallCount);
        Assert.Equal(0, timer.DisposeCallCount);
    }
}
