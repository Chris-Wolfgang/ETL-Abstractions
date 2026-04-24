using System;
using System.Collections.Generic;
using System.Threading;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.PipelineTests.TestDoubles;

/// <summary>
/// Extractor implementing only <see cref="IExtractAsync{T}"/> — no progress, no cancellation.
/// Used to verify the Pipeline API's no-progress, no-cancellation Extract overload.
/// </summary>
internal sealed class BareExtractor<T> : IExtractAsync<T>
    where T : notnull
{
    private readonly IReadOnlyList<T> _items;
    public int ExtractAsyncCallCount { get; private set; }


    public BareExtractor(IReadOnlyList<T> items)
    {
        _items = items;
    }


    public async IAsyncEnumerable<T> ExtractAsync()
    {
        ExtractAsyncCallCount++;
        foreach (var item in _items)
        {
            yield return item;
            await System.Threading.Tasks.Task.Yield();
        }
    }
}


/// <summary>
/// Extractor implementing <see cref="IExtractWithCancellationAsync{T}"/> — cancellation only.
/// Used to verify the Pipeline API's cancellation-only Extract overload and that the token
/// is forwarded.
/// </summary>
internal sealed class CancelOnlyExtractor<T> : IExtractWithCancellationAsync<T>
    where T : notnull
{
    private readonly IReadOnlyList<T> _items;
    public CancellationToken LastReceivedToken { get; private set; }
    public bool TokenOverloadWasCalled { get; private set; }


    public CancelOnlyExtractor(IReadOnlyList<T> items)
    {
        _items = items;
    }


    public IAsyncEnumerable<T> ExtractAsync() => throw new InvalidOperationException
    (
        "The pipeline should always call the token-taking overload for a cancellation-capable extractor."
    );


    public async IAsyncEnumerable<T> ExtractAsync
    (
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken token
    )
    {
        TokenOverloadWasCalled = true;
        LastReceivedToken = token;
        foreach (var item in _items)
        {
            token.ThrowIfCancellationRequested();
            yield return item;
            await System.Threading.Tasks.Task.Yield();
        }
    }
}


/// <summary>
/// Extractor implementing <see cref="IExtractWithProgressAsync{T, TProgress}"/> — progress only,
/// no cancellation. Used to verify that the Pipeline API forwards the progress sink and does
/// not forward a cancellation token.
/// </summary>
internal sealed class ProgressOnlyExtractor<T, TProgress> : IExtractWithProgressAsync<T, TProgress>
    where T : notnull
    where TProgress : notnull
{
    private readonly IReadOnlyList<T> _items;
    private readonly TProgress _reportValue;
    public IProgress<TProgress>? LastReceivedProgress { get; private set; }
    public bool ProgressOverloadWasCalled { get; private set; }
    public bool ParameterlessOverloadWasCalled { get; private set; }


    public ProgressOnlyExtractor(IReadOnlyList<T> items, TProgress reportValue)
    {
        _items = items;
        _reportValue = reportValue;
    }


    public async IAsyncEnumerable<T> ExtractAsync()
    {
        ParameterlessOverloadWasCalled = true;
        foreach (var item in _items)
        {
            yield return item;
            await System.Threading.Tasks.Task.Yield();
        }
    }


    public async IAsyncEnumerable<T> ExtractAsync(IProgress<TProgress> progress)
    {
        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        ProgressOverloadWasCalled = true;
        LastReceivedProgress = progress;
        foreach (var item in _items)
        {
            progress.Report(_reportValue);
            yield return item;
            await System.Threading.Tasks.Task.Yield();
        }
    }
}


/// <summary>
/// Extractor implementing <see cref="IExtractWithProgressAndCancellationAsync{T, TProgress}"/> —
/// both progress and cancellation. Used to verify the full-capability Extract overload.
/// </summary>
internal sealed class FullExtractor<T, TProgress> : IExtractWithProgressAndCancellationAsync<T, TProgress>
    where T : notnull
    where TProgress : notnull
{
    private readonly IReadOnlyList<T> _items;
    private readonly TProgress _reportValue;
    public IProgress<TProgress>? LastReceivedProgress { get; private set; }
    public CancellationToken LastReceivedToken { get; private set; }
    public bool FullOverloadWasCalled { get; private set; }


    public FullExtractor(IReadOnlyList<T> items, TProgress reportValue)
    {
        _items = items;
        _reportValue = reportValue;
    }


    public bool ParameterlessOverloadWasCalled { get; private set; }
    public bool TokenOnlyOverloadWasCalled { get; private set; }
    public bool ProgressOnlyOverloadWasCalled { get; private set; }


    public async IAsyncEnumerable<T> ExtractAsync()
    {
        ParameterlessOverloadWasCalled = true;
        foreach (var item in _items)
        {
            yield return item;
            await System.Threading.Tasks.Task.Yield();
        }
    }


    public async IAsyncEnumerable<T> ExtractAsync
    (
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken token
    )
    {
        TokenOnlyOverloadWasCalled = true;
        LastReceivedToken = token;
        foreach (var item in _items)
        {
            token.ThrowIfCancellationRequested();
            yield return item;
            await System.Threading.Tasks.Task.Yield();
        }
    }


    public async IAsyncEnumerable<T> ExtractAsync(IProgress<TProgress> progress)
    {
        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        ProgressOnlyOverloadWasCalled = true;
        LastReceivedProgress = progress;
        foreach (var item in _items)
        {
            progress.Report(_reportValue);
            yield return item;
            await System.Threading.Tasks.Task.Yield();
        }
    }


    public async IAsyncEnumerable<T> ExtractAsync
    (
        IProgress<TProgress> progress,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken token
    )
    {
        if (progress is null)
        {
            throw new ArgumentNullException(nameof(progress));
        }

        FullOverloadWasCalled = true;
        LastReceivedProgress = progress;
        LastReceivedToken = token;
        foreach (var item in _items)
        {
            token.ThrowIfCancellationRequested();
            progress.Report(_reportValue);
            yield return item;
            await System.Threading.Tasks.Task.Yield();
        }
    }
}
