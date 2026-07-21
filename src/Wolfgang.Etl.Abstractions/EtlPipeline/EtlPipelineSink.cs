using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Internal <see cref="IEtlPipelineSink"/> that feeds the composed record stream into a
/// <see cref="LoaderBase{TDestination, TProgress}"/>, counting delivered records and reporting
/// <see cref="EtlPipelineProgress"/> along the way.
/// </summary>
internal sealed class EtlPipelineSink<T, TProgress> : IEtlPipelineSink
    where T : notnull
    where TProgress : notnull
{
    private readonly Func<EtlRunState, CancellationToken, IAsyncEnumerable<T>> _factory;
    private readonly LoaderBase<T, TProgress> _loader;


    internal EtlPipelineSink
    (
        Func<EtlRunState, CancellationToken, IAsyncEnumerable<T>> factory,
        LoaderBase<T, TProgress> loader
    )
    {
        _factory = factory;
        _loader = loader;
    }


    /// <inheritdoc/>
    public async Task RunAsync(IProgress<EtlPipelineProgress>? progress = null, CancellationToken token = default)
    {
        var state = new EtlRunState();
        var stream = CountLoaded(_factory(state, token), state, progress, token);

        await _loader.LoadAsync(stream, token).ConfigureAwait(false);

        progress?.Report(state.Snapshot());
    }


    private static async IAsyncEnumerable<T> CountLoaded
    (
        IAsyncEnumerable<T> stream,
        EtlRunState state,
        IProgress<EtlPipelineProgress>? progress,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        await foreach (var item in stream.ConfigureAwait(false))
        {
            token.ThrowIfCancellationRequested();
            state.RecordsLoaded++;
            progress?.Report(state.Snapshot());
            yield return item;
        }
    }
}
