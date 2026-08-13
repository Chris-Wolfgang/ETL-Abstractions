using System;
using Wolfgang.Etl.TestKit.Xunit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Unit tests for <see cref="SynchronousProgress{T}"/>.
/// </summary>
public class SynchronousProgressTests
{
    // ------------------------------------------------------------------
    // Constructor
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that the constructor throws <see cref="ArgumentNullException"/>
    /// when <c>handler</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public void Constructor_with_null_handler_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SynchronousProgress<int>(null!));
    }



    // ------------------------------------------------------------------
    // Report
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="SynchronousProgress{T}.Report"/> invokes the
    /// handler synchronously with the reported value.
    /// </summary>
    [Fact]
    public void Report_invokes_handler_with_reported_value()
    {
        int? received = null;
        var progress = new SynchronousProgress<int>(v => received = v);

        progress.Report(42);

        Assert.Equal(42, received);
    }

    /// <summary>
    /// Verifies that <see cref="SynchronousProgress{T}.Report"/> invokes the
    /// handler synchronously — i.e. the value is available immediately after
    /// the call returns, without any awaiting.
    /// </summary>
    [Fact]
    public void Report_invokes_handler_synchronously()
    {
        var invokedOnCallingThread = false;
        var callingThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

        var progress = new SynchronousProgress<int>(_ =>
            invokedOnCallingThread =
                System.Threading.Thread.CurrentThread.ManagedThreadId == callingThreadId);

        progress.Report(1);

        Assert.True(invokedOnCallingThread);
    }

    /// <summary>
    /// Verifies that <see cref="SynchronousProgress{T}.Report"/> can be called
    /// multiple times, invoking the handler each time.
    /// </summary>
    [Fact]
    public void Report_invokes_handler_on_each_call()
    {
        var count = 0;
        var progress = new SynchronousProgress<int>(_ => count++);

        progress.Report(1);
        progress.Report(2);
        progress.Report(3);

        Assert.Equal(3, count);
    }

    /// <summary>
    /// Verifies that <see cref="SynchronousProgress{T}.Report"/> works correctly
    /// with a reference type value.
    /// </summary>
    [Fact]
    public void Report_works_with_reference_type()
    {
        string? received = null;
        var progress = new SynchronousProgress<string>(v => received = v);

        progress.Report("hello");

        Assert.Equal("hello", received);
    }

    /// <summary>
    /// Verifies that <see cref="SynchronousProgress{T}.Report"/> passes the
    /// default value of a value type correctly.
    /// </summary>
    [Fact]
    public void Report_passes_default_value_type()
    {
        int? received = null;
        var progress = new SynchronousProgress<int>(v => received = v);

        progress.Report(default);

        Assert.Equal(0, received);
    }
}
