using System;
using System.Threading;
using System.Threading.Tasks;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Extensions for terminal <see cref="IEtlPipelineSink"/> pipelines.
/// </summary>
public static class EtlPipelineSinkExtensions
{
    /// <summary>
    /// Wraps a sink so that the given resources — opened by the code that built the pipeline (for
    /// example a file stream a path-based loader factory created) — are disposed after the run
    /// completes, whether it succeeds or throws.
    /// </summary>
    /// <remarks>
    /// Every resource is disposed even when an earlier one throws while being disposed. If the run
    /// itself threw, that exception propagates unchanged; otherwise any disposal failures surface
    /// as a single <see cref="AggregateException"/> once all resources have been disposed.
    /// </remarks>
    /// <param name="sink">The sink to wrap.</param>
    /// <param name="ownedResources">
    /// The resources to dispose after the run. Each is disposed via <see cref="IAsyncDisposable"/>
    /// when supported, otherwise <see cref="IDisposable"/>; anything else (or <see langword="null"/>)
    /// is skipped. Resources are disposed in reverse of the order supplied (LIFO).
    /// </param>
    /// <returns>
    /// A sink that disposes <paramref name="ownedResources"/> after running, or the original
    /// <paramref name="sink"/> unchanged when no resources are supplied.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="sink"/> is <see langword="null"/>.</exception>
    public static IEtlPipelineSink DisposingOwned
    (
        this IEtlPipelineSink sink,
        params object[] ownedResources
    )
    {
        if (sink is null)
        {
            throw new ArgumentNullException(nameof(sink));
        }

        if (ownedResources is null || ownedResources.Length == 0)
        {
            return sink;
        }

        return new DisposingSink(sink, ownedResources);
    }



    private sealed class DisposingSink : IEtlPipelineSink
    {
        private readonly IEtlPipelineSink _inner;
        private readonly object[] _ownedResources;


        internal DisposingSink(IEtlPipelineSink inner, object[] ownedResources)
        {
            _inner = inner;
            _ownedResources = ownedResources;
        }


        public Task RunAsync
        (
            IProgress<EtlPipelineProgress>? progress = null,
            CancellationToken token = default
        )
        {
            // One disposal contract for both pipelines: reverse (LIFO) order, every resource is
            // disposed even when an earlier one throws, disposal failures are aggregated, and the
            // run's own exception — when there is one — is the exception the caller sees.
            // IAsyncDisposable is preferred where the concrete resource implements it (e.g.
            // FileStream on net5.0+); on the older frameworks the same FileStream matches only
            // IDisposable, so it disposes synchronously.
            return StageList.RunThenDisposeStagesAsync
            (
                runToken => _inner.RunAsync(progress, runToken),
                _ownedResources,
                token
            );
        }
    }
}
