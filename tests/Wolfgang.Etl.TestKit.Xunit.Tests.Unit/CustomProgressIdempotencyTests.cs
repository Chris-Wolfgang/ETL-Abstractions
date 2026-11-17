using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

/// <summary>
/// A progress type that is not <see cref="Report"/>. The two-parameter idempotency bases cast the
/// SUT to a <see cref="Report"/>-progress stage and fail on a stage that reports this type; the
/// three-parameter bases read the count from the stage itself.
/// </summary>
public sealed record CustomProgress
{
    // A constructor and a get-only property, not a positional record: the older target frameworks
    // have no IsExternalInit, which an init accessor needs.
    public CustomProgress(int itemCount) => ItemCount = itemCount;



    public int ItemCount { get; }
}



public sealed class CustomProgressLoader : LoaderBase<int, CustomProgress>
{
    private readonly List<int> _loaded = new();



    public IReadOnlyList<int> Loaded => _loaded.ToArray();



    protected override async Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token)
    {
        _loaded.Clear();

        await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
        {
            _loaded.Add(item);
            IncrementCurrentItemCount();
        }
    }



    protected override CustomProgress CreateProgressReport() => new(CurrentItemCount);
}



public sealed class CustomProgressTransformer : TransformerBase<int, int, CustomProgress>
{
    protected override async IAsyncEnumerable<int> TransformWorkerAsync
    (
        IAsyncEnumerable<int> items,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
        {
            IncrementCurrentItemCount();
            yield return item;
        }
    }



    protected override CustomProgress CreateProgressReport() => new(CurrentItemCount);
}



public class CustomProgressLoaderIdempotencyTests
    : IdempotentLoaderContractTests<CustomProgressLoader, int, CustomProgress>
{
    protected override CustomProgressLoader CreateSut(int itemCount) => new();

    protected override IReadOnlyList<int> CreateSourceItems() =>
        Enumerable.Range(1, 5).ToList();

    protected override IReadOnlyList<int>? TryGetLoadedItems(CustomProgressLoader sut) =>
        sut?.Loaded;



    [Fact]
    public async Task LoadAsync_with_progress_reports_the_custom_progress_type()
    {
        var sut = CreateSut(5);
        CustomProgress? last = null;

        await sut.LoadAsync
        (
            CreateSourceItems().ToAsyncEnumerable(),
            new SynchronousProgress<CustomProgress>(p => last = p)
        );

        Assert.NotNull(last);
        Assert.Equal(5, last.ItemCount);
    }



    [Fact]
    public void GetCurrentItemCount_when_sut_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => GetCurrentItemCount(null!));
    }
}



public class CustomProgressTransformerIdempotencyTests
    : IdempotentTransformerContractTests<CustomProgressTransformer, int, CustomProgress>
{
    protected override CustomProgressTransformer CreateSut(int itemCount) => new();

    protected override IReadOnlyList<int> CreateExpectedItems() =>
        Enumerable.Range(1, 5).ToList();



    [Fact]
    public async Task TransformAsync_with_progress_reports_the_custom_progress_type()
    {
        var sut = CreateSut(5);
        CustomProgress? last = null;

        await sut
            .TransformAsync
            (
                CreateExpectedItems().ToAsyncEnumerable(),
                new SynchronousProgress<CustomProgress>(p => last = p)
            )
            .ToListAsync();

        Assert.NotNull(last);
        Assert.Equal(5, last.ItemCount);
    }



    [Fact]
    public void GetCurrentItemCount_when_sut_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => GetCurrentItemCount(null!));
    }
}
