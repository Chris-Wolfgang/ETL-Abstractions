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


        public async Task RunAsync
        (
            IProgress<EtlPipelineProgress>? progress = null,
            CancellationToken token = default
        )
        {
            try
            {
                await _inner.RunAsync(progress, token).ConfigureAwait(false);
            }
            finally
            {
                await DisposeOwnedAsync().ConfigureAwait(false);
            }
        }


        // Reverse (LIFO) order, matching nested using / DI-scope disposal. IAsyncDisposable is
        // preferred where the concrete resource implements it (e.g. FileStream on net5.0+); on the
        // older frameworks the same FileStream matches only IDisposable, so it disposes synchronously.
        private async Task DisposeOwnedAsync()
        {
            for (var i = _ownedResources.Length - 1; i >= 0; i--)
            {
                switch (_ownedResources[i])
                {
                    case IAsyncDisposable asyncDisposable:
                        await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                        break;
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                }
            }
        }
    }
}
