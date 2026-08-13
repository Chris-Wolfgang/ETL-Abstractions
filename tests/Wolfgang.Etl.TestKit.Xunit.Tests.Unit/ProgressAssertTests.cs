using System;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit;
using Wolfgang.Etl.TestKit.Xunit;
using Xunit;
using Xunit.Sdk;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// Unit tests for <see cref="ProgressAssert"/>.
/// </summary>
public class ProgressAssertTests
{
    /// <summary>
    /// Casts a <see cref="ProgressCapture{T}"/> to <see cref="IProgress{T}"/> so its
    /// explicitly-implemented <see cref="IProgress{T}.Report"/> can be invoked.
    /// </summary>
    private static void Report<T>(ProgressCapture<T> capture, T value) =>
        ((IProgress<T>)capture).Report(value);



    // ------------------------------------------------------------------
    // HasReports
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="ProgressAssert.HasReports{T}"/> passes when at least one
    /// report was captured.
    /// </summary>
    [Fact]
    public void HasReports_when_reports_present_does_not_throw()
    {
        var capture = new ProgressCapture<int>();
        Report(capture, 1);

        var exception = Record.Exception(() => ProgressAssert.HasReports(capture));

        Assert.Null(exception);
    }



    /// <summary>
    /// Verifies that <see cref="ProgressAssert.HasReports{T}"/> fails when no reports were
    /// captured.
    /// </summary>
    [Fact]
    public void HasReports_when_no_reports_throws()
    {
        var capture = new ProgressCapture<int>();

        Assert.ThrowsAny<XunitException>(() => ProgressAssert.HasReports(capture));
    }



    /// <summary>
    /// Verifies that <see cref="ProgressAssert.HasReports{T}"/> guards against a null capture.
    /// </summary>
    [Fact]
    public void HasReports_when_capture_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ProgressAssert.HasReports<int>(null!));
    }



    // ------------------------------------------------------------------
    // HasExactly
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="ProgressAssert.HasExactly{T}"/> passes when the count matches.
    /// </summary>
    [Fact]
    public void HasExactly_when_count_matches_does_not_throw()
    {
        var capture = new ProgressCapture<int>();
        Report(capture, 1);
        Report(capture, 2);

        var exception = Record.Exception(() => ProgressAssert.HasExactly(capture, 2));

        Assert.Null(exception);
    }



    /// <summary>
    /// Verifies that <see cref="ProgressAssert.HasExactly{T}"/> fails when the count differs.
    /// </summary>
    [Fact]
    public void HasExactly_when_count_differs_throws()
    {
        var capture = new ProgressCapture<int>();
        Report(capture, 1);

        Assert.ThrowsAny<XunitException>(() => ProgressAssert.HasExactly(capture, 5));
    }



    /// <summary>
    /// Verifies that <see cref="ProgressAssert.HasExactly{T}"/> guards against a null capture.
    /// </summary>
    [Fact]
    public void HasExactly_when_capture_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ProgressAssert.HasExactly<int>(null!, 1));
    }



    // ------------------------------------------------------------------
    // IsMonotonicallyIncreasing
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="ProgressAssert.IsMonotonicallyIncreasing{T, TKey}"/> passes
    /// when selected values never decrease (including equal consecutive values).
    /// </summary>
    [Fact]
    public void IsMonotonicallyIncreasing_when_non_decreasing_does_not_throw()
    {
        var capture = new ProgressCapture<int>();
        Report(capture, 1);
        Report(capture, 1);
        Report(capture, 3);

        var exception = Record.Exception(() =>
            ProgressAssert.IsMonotonicallyIncreasing(capture, v => v));

        Assert.Null(exception);
    }



    /// <summary>
    /// Verifies that <see cref="ProgressAssert.IsMonotonicallyIncreasing{T, TKey}"/> fails
    /// when a selected value decreases.
    /// </summary>
    [Fact]
    public void IsMonotonicallyIncreasing_when_decreasing_throws()
    {
        var capture = new ProgressCapture<int>();
        Report(capture, 1);
        Report(capture, 5);
        Report(capture, 4);

        Assert.ThrowsAny<XunitException>(() =>
            ProgressAssert.IsMonotonicallyIncreasing(capture, v => v));
    }



    /// <summary>
    /// Verifies that <see cref="ProgressAssert.IsMonotonicallyIncreasing{T, TKey}"/> guards
    /// against a null capture.
    /// </summary>
    [Fact]
    public void IsMonotonicallyIncreasing_when_capture_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ProgressAssert.IsMonotonicallyIncreasing<int, int>(null!, v => v));
    }



    /// <summary>
    /// Verifies that <see cref="ProgressAssert.IsMonotonicallyIncreasing{T, TKey}"/> guards
    /// against a null selector.
    /// </summary>
    [Fact]
    public void IsMonotonicallyIncreasing_when_selector_null_throws_ArgumentNullException()
    {
        var capture = new ProgressCapture<int>();

        Assert.Throws<ArgumentNullException>(() =>
            ProgressAssert.IsMonotonicallyIncreasing<int, int>(capture, null!));
    }



    // ------------------------------------------------------------------
    // FinalReportSatisfies
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="ProgressAssert.FinalReportSatisfies{T}"/> passes when the
    /// final report matches the predicate.
    /// </summary>
    [Fact]
    public void FinalReportSatisfies_when_final_matches_does_not_throw()
    {
        var capture = new ProgressCapture<int>();
        Report(capture, 1);
        Report(capture, 100);

        var exception = Record.Exception(() =>
            ProgressAssert.FinalReportSatisfies(capture, v => v == 100));

        Assert.Null(exception);
    }



    /// <summary>
    /// Verifies that <see cref="ProgressAssert.FinalReportSatisfies{T}"/> fails when the
    /// final report does not match the predicate.
    /// </summary>
    [Fact]
    public void FinalReportSatisfies_when_final_does_not_match_throws()
    {
        var capture = new ProgressCapture<int>();
        Report(capture, 100);
        Report(capture, 50);

        Assert.ThrowsAny<XunitException>(() =>
            ProgressAssert.FinalReportSatisfies(capture, v => v == 100));
    }



    /// <summary>
    /// Verifies that <see cref="ProgressAssert.FinalReportSatisfies{T}"/> fails clearly when
    /// there are no reports.
    /// </summary>
    [Fact]
    public void FinalReportSatisfies_when_no_reports_throws()
    {
        var capture = new ProgressCapture<int>();

        Assert.ThrowsAny<XunitException>(() =>
            ProgressAssert.FinalReportSatisfies(capture, _ => true));
    }



    /// <summary>
    /// Verifies that <see cref="ProgressAssert.FinalReportSatisfies{T}"/> guards against a
    /// null capture.
    /// </summary>
    [Fact]
    public void FinalReportSatisfies_when_capture_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ProgressAssert.FinalReportSatisfies<int>(null!, _ => true));
    }



    /// <summary>
    /// Verifies that <see cref="ProgressAssert.FinalReportSatisfies{T}"/> guards against a
    /// null predicate.
    /// </summary>
    [Fact]
    public void FinalReportSatisfies_when_predicate_null_throws_ArgumentNullException()
    {
        var capture = new ProgressCapture<int>();

        Assert.Throws<ArgumentNullException>(() =>
            ProgressAssert.FinalReportSatisfies<int>(capture, null!));
    }



    // ------------------------------------------------------------------
    // AllReportsSatisfy
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="ProgressAssert.AllReportsSatisfy{T}"/> passes when every
    /// report matches the predicate.
    /// </summary>
    [Fact]
    public void AllReportsSatisfy_when_all_match_does_not_throw()
    {
        var capture = new ProgressCapture<int>();
        Report(capture, 2);
        Report(capture, 4);
        Report(capture, 6);

        var exception = Record.Exception(() =>
            ProgressAssert.AllReportsSatisfy(capture, v => v % 2 == 0));

        Assert.Null(exception);
    }



    /// <summary>
    /// Verifies that <see cref="ProgressAssert.AllReportsSatisfy{T}"/> fails when any report
    /// does not match the predicate.
    /// </summary>
    [Fact]
    public void AllReportsSatisfy_when_one_does_not_match_throws()
    {
        var capture = new ProgressCapture<int>();
        Report(capture, 2);
        Report(capture, 3);
        Report(capture, 4);

        Assert.ThrowsAny<XunitException>(() =>
            ProgressAssert.AllReportsSatisfy(capture, v => v % 2 == 0));
    }



    /// <summary>
    /// Verifies that <see cref="ProgressAssert.AllReportsSatisfy{T}"/> guards against a null
    /// capture.
    /// </summary>
    [Fact]
    public void AllReportsSatisfy_when_capture_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ProgressAssert.AllReportsSatisfy<int>(null!, _ => true));
    }



    /// <summary>
    /// Verifies that <see cref="ProgressAssert.AllReportsSatisfy{T}"/> guards against a null
    /// predicate.
    /// </summary>
    [Fact]
    public void AllReportsSatisfy_when_predicate_null_throws_ArgumentNullException()
    {
        var capture = new ProgressCapture<int>();

        Assert.Throws<ArgumentNullException>(() =>
            ProgressAssert.AllReportsSatisfy<int>(capture, null!));
    }



    // ------------------------------------------------------------------
    // End-to-end with a real TestExtractor
    // ------------------------------------------------------------------

    /// <summary>
    /// Drives a real <see cref="TestExtractor{T}"/> through
    /// <c>ExtractAsync(IProgress&lt;Report&gt;)</c> with a <see cref="ProgressCapture{T}"/>
    /// and verifies the captured progress is monotonically increasing with a sensible final
    /// item count.
    /// </summary>
    [Fact]
    public async Task ProgressCapture_with_TestExtractor_captures_monotonic_progress_Async()
    {
        var items = Enumerable.Range(1, 50).ToList();
        var extractor = new TestExtractor<int>(items);
        var capture = new ProgressCapture<Report>();

        var extracted = await extractor.ExtractAsync(capture).ToListAsync().ConfigureAwait(false);

        Assert.Equal(items, extracted);
        ProgressAssert.HasReports(capture);
        ProgressAssert.IsMonotonicallyIncreasing(capture, r => r.CurrentItemCount);
        ProgressAssert.AllReportsSatisfy(capture, r => r.CurrentItemCount >= 0);
        ProgressAssert.FinalReportSatisfies(capture, r => r.CurrentItemCount == items.Count);
    }
}
