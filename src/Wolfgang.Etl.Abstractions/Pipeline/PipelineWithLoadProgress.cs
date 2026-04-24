using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace Wolfgang.Etl.Abstractions;

/// <summary>
/// Internal <see cref="IPipelineWithLoadProgress{TProgress}"/> implementation. Holds the upstream
/// source, two loader entry points (with and without progress) as delegates, and an optional
/// captured <see cref="IProgress{TProgress}"/> sink. Which delegate actually runs is decided by
/// whether <see cref="WithProgress"/> has been called.
/// </summary>
internal sealed class PipelineWithLoadProgress<TItem, TProgress> : IPipelineWithLoadProgress<TProgress>
    where TItem : notnull
    where TProgress : notnull
{
    private readonly Func<CancellationToken, IAsyncEnumerable<TItem>> _upstream;
    private readonly Func<IAsyncEnumerable<TItem>, CancellationToken, Task> _noProgressLoad;
    private readonly Func<IAsyncEnumerable<TItem>, IProgress<TProgress>, CancellationToken, Task> _withProgressLoad;
    private IProgress<TProgress>? _progress;
    private int _runCount;


    internal PipelineWithLoadProgress
    (
        Func<CancellationToken, IAsyncEnumerable<TItem>> upstream,
        Func<IAsyncEnumerable<TItem>, CancellationToken, Task> noProgressLoad,
        Func<IAsyncEnumerable<TItem>, IProgress<TProgress>, CancellationToken, Task> withProgressLoad
    )
    {
        _upstream = upstream;
        _noProgressLoad = noProgressLoad;
        _withProgressLoad = withProgressLoad;
    }


    /// <inheritdoc/>
    public string? Name { get; private set; }


    /// <inheritdoc/>
    public IPipeline WithName(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        Name = name;
        return this;
    }


    /// <inheritdoc/>
    public IPipeline WithProgress(IProgress<TProgress> progress)
    {
        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        if (_progress is not null)
        {
            throw new InvalidOperationException
            (
                "WithProgress has already been called on this pipeline. Progress can only be set once."
            );
        }

        _progress = progress;
        return this;
    }


    /// <inheritdoc/>
    public Task RunAsync()
    {
        return RunAsync(CancellationToken.None);
    }


    /// <inheritdoc/>
    public Task RunAsync(CancellationToken token)
    {
        try
        {
            if (Interlocked.Exchange(ref _runCount, 1) != 0)
            {
                throw new InvalidOperationException
                (
                    "Pipeline has already been run. Construct a new pipeline for each run."
                );
            }

            return _progress is null
                ? _noProgressLoad(_upstream(token), token)
                : _withProgressLoad(_upstream(token), _progress, token);
        }
#pragma warning disable CA1031 // Do not catch general exception types — intentional: we forward every failure through the Task contract.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            return Task.FromException(ex);
        }
    }
}
