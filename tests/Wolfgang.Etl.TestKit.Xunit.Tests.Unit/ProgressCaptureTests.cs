using System;
using System.Collections.Generic;
using Wolfgang.Etl.TestKit.Xunit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Unit tests for <see cref="ProgressCapture{T}"/>.
/// </summary>
public class ProgressCaptureTests
{
    /// <summary>
    /// Casts a <see cref="ProgressCapture{T}"/> to <see cref="IProgress{T}"/> so its
    /// explicitly-implemented <see cref="IProgress{T}.Report"/> can be invoked.
    /// </summary>
    private static void Report<T>(ProgressCapture<T> capture, T value) =>
        ((IProgress<T>)capture).Report(value);



    /// <summary>
    /// Verifies that a freshly created capture has captured no reports.
    /// </summary>
    [Fact]
    public void Reports_when_nothing_reported_is_empty()
    {
        var capture = new ProgressCapture<int>();

        Assert.Empty(capture.Reports);
    }



    /// <summary>
    /// Verifies that <see cref="ProgressCapture{T}.Count"/> is zero before any report.
    /// </summary>
    [Fact]
    public void Count_when_nothing_reported_is_zero()
    {
        var capture = new ProgressCapture<int>();

        Assert.Equal(0, capture.Count);
    }



    /// <summary>
    /// Verifies that <see cref="ProgressCapture{T}.FinalReport"/> is the default value of a
    /// value type when nothing has been reported.
    /// </summary>
    [Fact]
    public void FinalReport_when_nothing_reported_is_default_value_type()
    {
        var capture = new ProgressCapture<int>();

        Assert.Equal(0, capture.FinalReport);
    }



    /// <summary>
    /// Verifies that <see cref="ProgressCapture{T}.FinalReport"/> is <see langword="null"/>
    /// for a reference type when nothing has been reported.
    /// </summary>
    [Fact]
    public void FinalReport_when_nothing_reported_is_null_reference_type()
    {
        var capture = new ProgressCapture<string>();

        Assert.Null(capture.FinalReport);
    }



    /// <summary>
    /// Verifies that reports are recorded in the order they are reported.
    /// </summary>
    [Fact]
    public void Reports_records_values_in_order()
    {
        var capture = new ProgressCapture<int>();

        Report(capture, 1);
        Report(capture, 2);
        Report(capture, 3);

        Assert.Equal(new[] { 1, 2, 3 }, capture.Reports);
    }



    /// <summary>
    /// Verifies that <see cref="ProgressCapture{T}.Count"/> reflects the number of reports.
    /// </summary>
    [Fact]
    public void Count_reflects_number_of_reports()
    {
        var capture = new ProgressCapture<int>();

        Report(capture, 10);
        Report(capture, 20);

        Assert.Equal(2, capture.Count);
    }



    /// <summary>
    /// Verifies that <see cref="ProgressCapture{T}.FinalReport"/> returns the last value.
    /// </summary>
    [Fact]
    public void FinalReport_returns_last_reported_value()
    {
        var capture = new ProgressCapture<int>();

        Report(capture, 5);
        Report(capture, 6);
        Report(capture, 7);

        Assert.Equal(7, capture.FinalReport);
    }



    /// <summary>
    /// Verifies that the explicit <see cref="IProgress{T}.Report"/> implementation runs
    /// synchronously on the calling thread.
    /// </summary>
    [Fact]
    public void Report_runs_synchronously_on_calling_thread()
    {
        var capture = new ProgressCapture<int>();

        Report(capture, 42);

        // If Report were posted to another thread, the value would not be visible yet.
        Assert.Equal(1, capture.Count);
    }
}
