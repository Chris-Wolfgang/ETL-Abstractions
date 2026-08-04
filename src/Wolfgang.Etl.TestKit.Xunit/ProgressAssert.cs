using System;
using System.Collections.Generic;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Provides xUnit-style assertions for validating sequences of progress reports captured by
/// a <see cref="ProgressCapture{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Each assertion throws an xUnit assertion exception (via the standard <c>Assert.*</c> API)
/// when the condition is not met, so failures integrate with xUnit reporting exactly like any
/// other assertion. These helpers replace the progress-validation boilerplate that downstream
/// ETL test suites would otherwise copy between projects.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var capture = new ProgressCapture&lt;Report&gt;();
///
/// await extractor.ExtractAsync(capture).ToListAsync();
///
/// ProgressAssert.HasReports(capture);
/// ProgressAssert.IsMonotonicallyIncreasing(capture, r =&gt; r.CurrentItemCount);
/// ProgressAssert.FinalReportSatisfies(capture, r =&gt; r.CurrentItemCount == 100);
/// </code>
/// </example>
public static class ProgressAssert
{
    /// <summary>
    /// Asserts that <paramref name="capture"/> recorded at least one progress report.
    /// </summary>
    /// <typeparam name="T">The type of progress value.</typeparam>
    /// <param name="capture">The capture to inspect.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="capture"/> is <see langword="null"/>.
    /// </exception>
    public static void HasReports<T>(ProgressCapture<T> capture)
    {
        if (capture is null)
        {
            throw new ArgumentNullException(nameof(capture));
        }

        Assert.NotEmpty(capture.Reports);
    }



    /// <summary>
    /// Asserts that <paramref name="capture"/> recorded exactly <paramref name="count"/>
    /// progress reports.
    /// </summary>
    /// <typeparam name="T">The type of progress value.</typeparam>
    /// <param name="capture">The capture to inspect.</param>
    /// <param name="count">The exact number of reports expected.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="capture"/> is <see langword="null"/>.
    /// </exception>
    public static void HasExactly<T>(ProgressCapture<T> capture, int count)
    {
        if (capture is null)
        {
            throw new ArgumentNullException(nameof(capture));
        }

        Assert.Equal(count, capture.Reports.Count);
    }



    /// <summary>
    /// Asserts that the value selected by <paramref name="selector"/> never decreases across
    /// consecutive reports.
    /// </summary>
    /// <typeparam name="T">The type of progress value.</typeparam>
    /// <typeparam name="TKey">The type of the selected, comparable key.</typeparam>
    /// <param name="capture">The capture to inspect.</param>
    /// <param name="selector">Selects the comparable value to test from each report.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="capture"/> or <paramref name="selector"/> is <see langword="null"/>.
    /// </exception>
    public static void IsMonotonicallyIncreasing<T, TKey>
    (
        ProgressCapture<T> capture,
        Func<T, TKey> selector
    )
        where TKey : IComparable<TKey>
    {
        if (capture is null)
        {
            throw new ArgumentNullException(nameof(capture));
        }

        if (selector is null)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        var reports = capture.Reports;

        for (var i = 1; i < reports.Count; i++)
        {
            var previous = selector(reports[i - 1]);
            var current = selector(reports[i]);

            Assert.True
            (
                current.CompareTo(previous) >= 0,
                $"Reports were not monotonically increasing: report at index {i} " +
                $"({current}) is less than the report at index {i - 1} ({previous})."
            );
        }
    }



    /// <summary>
    /// Asserts that the final (last) captured report satisfies <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="T">The type of progress value.</typeparam>
    /// <param name="capture">The capture to inspect.</param>
    /// <param name="predicate">The condition the final report must satisfy.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="capture"/> or <paramref name="predicate"/> is <see langword="null"/>.
    /// </exception>
    public static void FinalReportSatisfies<T>(ProgressCapture<T> capture, Func<T, bool> predicate)
    {
        if (capture is null)
        {
            throw new ArgumentNullException(nameof(capture));
        }

        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        var reports = capture.Reports;

        Assert.True
        (
            reports.Count > 0,
            "Expected at least one report so the final report could be evaluated, but none were captured."
        );

        Assert.True
        (
            predicate(reports[reports.Count - 1]),
            "The final report did not satisfy the predicate."
        );
    }



    /// <summary>
    /// Asserts that every captured report satisfies <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="T">The type of progress value.</typeparam>
    /// <param name="capture">The capture to inspect.</param>
    /// <param name="predicate">The condition every report must satisfy.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="capture"/> or <paramref name="predicate"/> is <see langword="null"/>.
    /// </exception>
    public static void AllReportsSatisfy<T>(ProgressCapture<T> capture, Func<T, bool> predicate)
    {
        if (capture is null)
        {
            throw new ArgumentNullException(nameof(capture));
        }

        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        IReadOnlyList<T> reports = capture.Reports;

        for (var i = 0; i < reports.Count; i++)
        {
            Assert.True
            (
                predicate(reports[i]),
                $"The report at index {i} did not satisfy the predicate."
            );
        }
    }
}
