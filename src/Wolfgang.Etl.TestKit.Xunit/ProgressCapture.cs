using System;
using System.Collections.Generic;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// An <see cref="IProgress{T}"/> implementation that captures every reported value,
/// synchronously, on the calling thread.
/// </summary>
/// <typeparam name="T">The type of progress value.</typeparam>
/// <remarks>
/// <para>
/// Like <see cref="SynchronousProgress{T}"/>, <see cref="ProgressCapture{T}"/> reports on
/// the same thread that calls <see cref="IProgress{T}.Report"/>, making progress assertions
/// in tests fully deterministic without delays or manual synchronisation. Unlike
/// <see cref="SynchronousProgress{T}"/>, it also records every reported value so the full
/// sequence can be inspected after the operation under test completes.
/// </para>
/// <para>
/// This type subsumes the manual <c>new List&lt;T&gt;()</c> plus
/// <c>new SynchronousProgress&lt;T&gt;(r =&gt; list.Add(r))</c> wiring that test code would
/// otherwise repeat. Pair it with <see cref="ProgressAssert"/> for one-liner verification of
/// the captured sequence.
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
public sealed class ProgressCapture<T> : IProgress<T>
{
    private readonly object _gate = new object();

    private readonly List<T> _reports = new List<T>();



    /// <summary>
    /// Gets all captured reports, in the order they were reported.
    /// </summary>
    public IReadOnlyList<T> Reports
    {
        get
        {
            lock (_gate)
            {
                return _reports.ToArray();
            }
        }
    }



    /// <summary>
    /// Gets the last captured report, or <see langword="default"/> if no report has been
    /// captured.
    /// </summary>
    public T? FinalReport
    {
        get
        {
            lock (_gate)
            {
                return _reports.Count == 0 ? default : _reports[_reports.Count - 1];
            }
        }
    }



    /// <summary>
    /// Gets the number of reports captured so far.
    /// </summary>
    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _reports.Count;
            }
        }
    }



    /// <summary>
    /// Captures <paramref name="value"/> by appending it to <see cref="Reports"/>.
    /// </summary>
    /// <param name="value">The progress value to capture.</param>
    void IProgress<T>.Report(T value)
    {
        lock (_gate)
        {
            _reports.Add(value);
        }
    }
}
