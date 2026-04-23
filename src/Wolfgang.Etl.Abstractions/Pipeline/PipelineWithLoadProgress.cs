using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Internal <see cref="IPipelineWithLoadProgress{TProgress}"/> implementation. Holds the upstream
/// source, the progress-capable loader, and an optional bound <see cref="IProgress{TProgress}"/>.
/// </summary>
internal sealed class PipelineWithLoadProgress<TItem, TProgress> : IPipelineWithLoadProgress<TProgress>
    where TItem : notnull
    where TProgress : notnull
{
    private readonly Func<CancellationToken, IAsyncEnumerable<TItem>> _upstream;
    private readonly ILoadWithProgressAndCancellationAsync<TItem, TProgress> _loader;
    private IProgress<TProgress>? _progress;
    private int _runCount;


    internal PipelineWithLoadProgress
    (
        Func<CancellationToken, IAsyncEnumerable<TItem>> upstream,
        ILoadWithProgressAndCancellationAsync<TItem, TProgress> loader
    )
    {
        _upstream = upstream;
        _loader = loader;
    }


    public string? Name { get; private set; }


    public IPipeline WithName(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        Name = name;
        return this;
    }


    public IPipeline WithProgress(IProgress<TProgress> progress)
    {
        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        _progress = progress;
        return this;
    }


    public Task RunAsync()
    {
        return RunAsync(CancellationToken.None);
    }


    public Task RunAsync(CancellationToken token)
    {
        if (Interlocked.Exchange(ref _runCount, 1) != 0)
        {
            throw new InvalidOperationException
            (
                "Pipeline has already been run. Construct a new pipeline for each run."
            );
        }

        return _progress is null
            ? _loader.LoadAsync(_upstream(token), token)
            : _loader.LoadAsync(_upstream(token), _progress, token);
    }
}
