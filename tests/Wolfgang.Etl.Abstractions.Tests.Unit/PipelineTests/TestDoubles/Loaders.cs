using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests.TestDoubles;

/// <summary>
/// Loader implementing only <see cref="ILoadAsync{T}"/>. No progress, no cancellation.
/// </summary>
internal sealed class BareLoader<T> : ILoadAsync<T>
    where T : notnull
{
    public List<T> Loaded { get; } = new();
    public int LoadAsyncCallCount { get; private set; }


    public async Task LoadAsync(IAsyncEnumerable<T> items)
    {
        LoadAsyncCallCount++;
        await foreach (var item in items.ConfigureAwait(false))
        {
            Loaded.Add(item);
        }
    }
}


/// <summary>
/// Loader implementing <see cref="ILoadWithCancellationAsync{T}"/> — cancellation only.
/// </summary>
internal sealed class CancelOnlyLoader<T> : ILoadWithCancellationAsync<T>
    where T : notnull
{
    public List<T> Loaded { get; } = new();
    public CancellationToken LastReceivedToken { get; private set; }
    public bool TokenOverloadWasCalled { get; private set; }


    public Task LoadAsync(IAsyncEnumerable<T> items)
        => throw new WrongOverloadCalledException("CancelOnlyLoader<T>.LoadAsync(items)");


    public async Task LoadAsync(IAsyncEnumerable<T> items, CancellationToken token)
    {
        TokenOverloadWasCalled = true;
        LastReceivedToken = token;
        await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
        {
            Loaded.Add(item);
        }
    }
}


/// <summary>
/// Loader implementing <see cref="ILoadWithProgressAsync{T, TProgress}"/> — progress only.
/// </summary>
internal sealed class ProgressOnlyLoader<T, TProgress> : ILoadWithProgressAsync<T, TProgress>
    where T : notnull
    where TProgress : notnull
{
    private readonly TProgress _reportValue;
    public List<T> Loaded { get; } = new();
    public IProgress<TProgress>? LastReceivedProgress { get; private set; }
    public bool ProgressOverloadWasCalled { get; private set; }
    public bool ParameterlessOverloadWasCalled { get; private set; }


    public ProgressOnlyLoader(TProgress reportValue)
    {
        _reportValue = reportValue;
    }


    public async Task LoadAsync(IAsyncEnumerable<T> items)
    {
        ParameterlessOverloadWasCalled = true;
        await foreach (var item in items.ConfigureAwait(false))
        {
            Loaded.Add(item);
        }
    }


    public async Task LoadAsync(IAsyncEnumerable<T> items, IProgress<TProgress> progress)
    {
        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        ProgressOverloadWasCalled = true;
        LastReceivedProgress = progress;
        await foreach (var item in items.ConfigureAwait(false))
        {
            progress.Report(_reportValue);
            Loaded.Add(item);
        }
    }
}


/// <summary>
/// Loader implementing <see cref="ILoadWithProgressAndCancellationAsync{T, TProgress}"/> —
/// both progress and cancellation.
/// </summary>
internal sealed class FullLoader<T, TProgress> : ILoadWithProgressAndCancellationAsync<T, TProgress>
    where T : notnull
    where TProgress : notnull
{
    private readonly TProgress _reportValue;
    public List<T> Loaded { get; } = new();
    public IProgress<TProgress>? LastReceivedProgress { get; private set; }
    public CancellationToken LastReceivedToken { get; private set; }
    public bool FullOverloadWasCalled { get; private set; }


    public FullLoader(TProgress reportValue)
    {
        _reportValue = reportValue;
    }


    public bool ParameterlessOverloadWasCalled { get; private set; }
    public bool TokenOnlyOverloadWasCalled { get; private set; }
    public bool ProgressOnlyOverloadWasCalled { get; private set; }


    public async Task LoadAsync(IAsyncEnumerable<T> items)
    {
        ParameterlessOverloadWasCalled = true;
        await foreach (var item in items.ConfigureAwait(false))
        {
            Loaded.Add(item);
        }
    }


    public async Task LoadAsync(IAsyncEnumerable<T> items, CancellationToken token)
    {
        TokenOnlyOverloadWasCalled = true;
        LastReceivedToken = token;
        await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
        {
            Loaded.Add(item);
        }
    }


    public async Task LoadAsync(IAsyncEnumerable<T> items, IProgress<TProgress> progress)
    {
        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        ProgressOnlyOverloadWasCalled = true;
        LastReceivedProgress = progress;
        await foreach (var item in items.ConfigureAwait(false))
        {
            progress.Report(_reportValue);
            Loaded.Add(item);
        }
    }


    public async Task LoadAsync
    (
        IAsyncEnumerable<T> items,
        IProgress<TProgress> progress,
        CancellationToken token
    )
    {
        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        FullOverloadWasCalled = true;
        LastReceivedProgress = progress;
        LastReceivedToken = token;
        await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
        {
            progress.Report(_reportValue);
            Loaded.Add(item);
        }
    }
}
