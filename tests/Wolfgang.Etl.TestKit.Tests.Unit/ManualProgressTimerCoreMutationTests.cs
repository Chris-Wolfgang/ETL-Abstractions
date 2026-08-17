using System;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

/// <summary>
/// Mutation-hardening tests (#346) for <see cref="ManualProgressTimerCore"/>: they pin the exact
/// text of the "timer has not started" exception, whose message is built from two string literals.
/// </summary>
public class ManualProgressTimerCoreMutationTests
{
    [Fact]
    public void Tick_before_the_run_starts_reports_the_full_reason()
    {
        var timer = new ManualProgressTimerCore();

        var ex = Assert.Throws<InvalidOperationException>
        (
            () => timer.Tick()
        );

        // Assert against text from BOTH concatenated literals so neither can be blanked.
        Assert.Equal
        (
            "The progress timer has not started. Begin the run (start enumeration / invoke the " +
            "loader) before calling Tick().",
            ex.Message
        );
    }
}
