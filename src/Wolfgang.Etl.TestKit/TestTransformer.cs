using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit;

/// <summary>
/// A pass-through transformer for use in tests, examples, and benchmarks.
/// Returns each item unchanged, with no transformation applied.
/// </summary>
/// <typeparam name="T">The type of item to transform.</typeparam>
/// <remarks>
/// <para>
/// Useful when a pipeline requires a transformer in the chain but the test or
/// benchmark is focused on the extractor or loader in isolation.
/// </para>
/// <para>
/// Set <see cref="TransformerBase{TSource,TDestination,TProgress}.SkipItemCount"/> to
/// skip the first N items before yielding. Set
/// <see cref="TransformerBase{TSource,TDestination,TProgress}.MaximumItemCount"/> to
/// stop after yielding that many items.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var extractor   = new TestExtractor&lt;MyRecord&gt;(items);
/// var transformer = new TestTransformer&lt;MyRecord&gt;();
/// var loader      = new TestLoader&lt;MyRecord&gt;(collectItems: true);
///
/// await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync()));
/// </code>
/// </example>
public class TestTransformer<T> : TransformerBase<T, T, Report>
    where T : notnull
{
    // ------------------------------------------------------------------
    // Fields
    // ------------------------------------------------------------------

    private readonly IProgressTimer? _progressTimer;
    private bool _progressTimerWired;
    private Action? _elapsedHandler;



    // ------------------------------------------------------------------
    // Constructors
    // ------------------------------------------------------------------

    /// <summary>
    /// Initializes a new <see cref="TestTransformer{T}"/> using the default
    /// base-class progress timer.
    /// </summary>
    public TestTransformer() { }



    /// <summary>
    /// Initializes a new <see cref="TestTransformer{T}"/> with the supplied
    /// <see cref="IProgressTimer"/> to drive progress callbacks.
    /// </summary>
    /// <param name="timer">
    /// The timer used to drive progress callbacks. Inject a
    /// <c>ManualProgressTimer</c> in tests to fire callbacks on demand.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="timer"/> is <see langword="null"/>.
    /// </exception>
    protected TestTransformer(IProgressTimer timer)
    {
        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));
    }



    // ------------------------------------------------------------------
    // TransformerBase overrides
    // ------------------------------------------------------------------

    /// <inheritdoc/>
    protected override IProgressTimer CreateProgressTimer(IProgress<Report> progress)
    {
        if (_progressTimer is null)
        {
            return base.CreateProgressTimer(progress);
        }

        if (!_progressTimerWired)
        {
            _progressTimerWired = true;
            _elapsedHandler = () => progress.Report(CreateProgressReport());
            _progressTimer.Elapsed += _elapsedHandler;
        }

        return _progressTimer;
    }



    /// <summary>
    /// Unsubscribes the <see cref="IProgressTimer.Elapsed"/> handler from an injected
    /// timer. The injected timer is owned by the caller and is therefore not disposed here.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> when called from <see cref="IDisposable.Dispose"/>;
    /// <see langword="false"/> when called from the finalizer.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && _progressTimer is not null && _elapsedHandler is not null)
        {
            _progressTimer.Elapsed -= _elapsedHandler;
            _elapsedHandler = null;
        }

        base.Dispose(disposing);
    }



    /// <inheritdoc/>
    protected override Report CreateProgressReport() =>
        new(CurrentItemCount, StartedAt, Elapsed);



    /// <inheritdoc/>
    protected override async IAsyncEnumerable<T> TransformWorkerAsync
    (
        IAsyncEnumerable<T> items,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        token.ThrowIfCancellationRequested();

        try
        {
            // Stryker disable once Boolean : equivalent — ConfigureAwait(false) vs (true) has no
            // observable effect on the transformed sequence in these single-threaded tests.
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
                    yield break;
                }

                IncrementCurrentItemCount();
                yield return item;
            }
        }
        finally
        {
            _progressTimer?.StopTimer();
        }
    }
}
