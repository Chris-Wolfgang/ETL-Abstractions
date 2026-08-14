using System;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

public class ProgressTimerExtensionsTests
{
    // ------------------------------------------------------------------
    // Null-argument guards not already covered elsewhere
    // (the extractor overload + the three timer-null guards live in
    //  ManualProgressTimerCoreTests).
    // ------------------------------------------------------------------

    [Fact]
    public void WithManualProgressTimer_when_loader_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => ((LoaderBase<int, Report>)null!).WithManualProgressTimer(new ManualProgressTimerCore())
        );
    }



    [Fact]
    public void WithManualProgressTimer_when_transformer_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => ((TransformerBase<int, int, Report>)null!).WithManualProgressTimer(new ManualProgressTimerCore())
        );
    }



    // ------------------------------------------------------------------
    // Loader / transformer overloads attach the timer and return the stage
    // ------------------------------------------------------------------

    [Fact]
    public void WithManualProgressTimer_loader_overload_attaches_and_returns_the_loader()
    {
        var loader = new TestLoader<int>(collectItems: false);

        var returned = loader.WithManualProgressTimer(new ManualProgressTimerCore());

        Assert.Same(loader, returned);
    }



    [Fact]
    public void WithManualProgressTimer_transformer_overload_attaches_and_returns_the_transformer()
    {
        var transformer = new TestTransformer<int>();

        var returned = transformer.WithManualProgressTimer(new ManualProgressTimerCore());

        Assert.Same(transformer, returned);
    }
}
