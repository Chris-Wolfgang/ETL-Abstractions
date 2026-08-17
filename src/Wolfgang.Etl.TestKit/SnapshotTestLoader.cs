using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit;

/// <summary>
/// An in-memory loader that captures every item it receives and renders them as a single,
/// deterministic, diff-friendly <see cref="Snapshot"/> string — ready to hand to an approval /
/// snapshot-testing framework such as <see href="https://github.com/VerifyTests/Verify">Verify</see>.
/// </summary>
/// <typeparam name="T">The type of item to load. Must be <c>notnull</c>.</typeparam>
/// <remarks>
/// <para>
/// This loader is deliberately <em>capture-only</em>: it does no file I/O and takes no dependency on
/// any snapshot framework, so referencing <c>Wolfgang.Etl.TestKit</c> never pulls one in. Run your
/// pipeline into it, then pass <see cref="Snapshot"/> (or <see cref="LoadedItems"/>) to your own
/// <c>Verify(...)</c> call in a dedicated snapshot test project — the fleet convention. The framework
/// owns the golden <c>.verified.txt</c> file, the diff, and the approval workflow; this loader only
/// produces the content to lock in.
/// </para>
/// <para>
/// <see cref="Snapshot"/> is one formatted line per item, joined by <c>\n</c> (a fixed line feed, not
/// <see cref="Environment.NewLine"/>, so snapshots are stable across operating systems). Items are
/// formatted with the delegate supplied to the constructor; the default is <see cref="object.ToString"/>,
/// which is diff-friendly for <c>record</c> types. Supply a custom formatter to project only the fields
/// you care about and to <em>scrub</em> non-deterministic values (timestamps, GUIDs, auto-increment
/// IDs) that would otherwise make the snapshot unstable.
/// </para>
/// <para>
/// Timing and progress are intentionally excluded from the snapshot — they are non-deterministic. Set
/// <see cref="LoaderBase{TDestination,TProgress}.SkipItemCount"/> to skip the first N items and
/// <see cref="LoaderBase{TDestination,TProgress}.MaximumItemCount"/> to cap how many are captured; each
/// new <c>LoadAsync</c> call clears the buffer before it begins.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // In a dedicated net10.0 snapshot test project that references Verify.Xunit:
/// var loader = new SnapshotTestLoader&lt;OrderRecord&gt;(
///     o =&gt; $"{o.Id}|{o.Customer}|{o.Total:0.00}");   // project + scrub as needed
///
/// await loader.LoadAsync(pipeline.ExtractAsync());
///
/// await Verify(loader.Snapshot);   // Verify owns the .verified.txt golden file + diff
/// </code>
/// </example>
public class SnapshotTestLoader<T> : LoaderBase<T, Report>
    where T : notnull
{
    // ------------------------------------------------------------------
    // Fields
    // ------------------------------------------------------------------

    private readonly List<T> _buffer = new List<T>();
    private readonly Func<T, string> _formatter;



    // ------------------------------------------------------------------
    // Constructors
    // ------------------------------------------------------------------

    /// <summary>
    /// Initializes a new <see cref="SnapshotTestLoader{T}"/> that formats each captured item with
    /// its <see cref="object.ToString"/> representation.
    /// </summary>
    public SnapshotTestLoader()
        : this(item => item.ToString() ?? string.Empty)
    {
    }



    /// <summary>
    /// Initializes a new <see cref="SnapshotTestLoader{T}"/> that formats each captured item with the
    /// supplied delegate.
    /// </summary>
    /// <param name="formatter">
    /// Projects a captured item to the single snapshot line that represents it. Use this to select
    /// only the fields under test and to scrub non-deterministic values (timestamps, GUIDs,
    /// auto-increment IDs) so the snapshot stays stable across runs.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="formatter"/> is <see langword="null"/>.</exception>
    public SnapshotTestLoader(Func<T, string> formatter)
    {
        _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
    }



    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    /// <summary>
    /// Gets the items captured by the most recent load, in the order they were received.
    /// </summary>
    /// <returns>
    /// A point-in-time copy of the captured items. Empty before the first load, and reset at the start
    /// of every <c>LoadAsync</c> call.
    /// </returns>
    public IReadOnlyList<T> LoadedItems => _buffer.ToArray();



    /// <summary>
    /// Gets the deterministic snapshot of the captured items: one formatted line per item, joined by a
    /// line feed (<c>\n</c>). Hand this to a snapshot / approval framework to lock in the pipeline's output.
    /// </summary>
    /// <value>
    /// The formatted, newline-joined snapshot, or <see cref="string.Empty"/> when no items have been
    /// captured.
    /// </value>
    public string Snapshot => string.Join("\n", _buffer.Select(_formatter));



    // ------------------------------------------------------------------
    // LoaderBase overrides
    // ------------------------------------------------------------------

    /// <inheritdoc/>
    protected override Report CreateProgressReport() =>
        new(CurrentItemCount, StartedAt, Elapsed);



    /// <inheritdoc/>
    protected override async Task LoadWorkerAsync
    (
        IAsyncEnumerable<T> items,
        CancellationToken token
    )
    {
        token.ThrowIfCancellationRequested();

        _buffer.Clear();

        // Stryker disable once Boolean : ConfigureAwait(false) is required on net462/netstandard2.0;
        // (true) is behaviourally identical under the test host (no synchronization context), so the
        // mutant is equivalent and unkillable.
        await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
        {
            token.ThrowIfCancellationRequested();

            if (CurrentSkippedItemCount < SkipItemCount)
            {
                IncrementCurrentSkippedItemCount();
                continue;
            }

            if (CurrentItemCount >= MaximumItemCount)
            {
                break;
            }

            _buffer.Add(item);

            IncrementCurrentItemCount();
        }
    }
}
