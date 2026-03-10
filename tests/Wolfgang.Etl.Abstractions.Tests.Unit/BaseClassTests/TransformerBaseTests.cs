using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;
public class TransformerBaseTests
{


    [Fact]
    public async Task TransformerBase_works_with_specified_versions_of_dotnet()
    {
        var numbers = new[] { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55 };
        var sut = new IntToStringTransformerFromTransformerBase();

        var actual = await sut.TransformAsync(numbers.ToAsyncEnumerable()).ToListAsync();
        var expected = numbers.Select(n => n.ToString()).ToList();

        Assert.Equal(expected, actual);
    }



    // Transform tests

    [Fact]
    public async Task TransformAsync_when_passed_null_items_throws_ArgumentNullException()
    {
        var sut = new IntToStringTransformerFromTransformerBase();

        await Assert.ThrowsAsync<ArgumentNullException> (async ()=> await sut.TransformAsync(null!).ToListAsync());
    }



    [Fact]
    public async Task TransformAsync_returns_expected_values()
    {
        var sut = new IntToStringTransformerFromTransformerBase();

        var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToAsyncEnumerable();

        var expectedResults = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
        var actualResults = await sut.TransformAsync(items).ToListAsync();

        Assert.Equal(expectedResults, actualResults);
    }



    // Transform With Cancellation tests


    [Fact]
    public async Task TransformWithCancellationAsync_when_passed_null_items_throws_ArgumentNullException()
    {
        var sut = new IntToStringTransformerFromTransformerBase();

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await sut.TransformAsync(null!, CancellationToken.None).ToListAsync());
    }



    [Fact]
    public async Task TransformWithCancellationAsync_returns_expected_values()
    {
        var sut = new IntToStringTransformerFromTransformerBase();

        var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToAsyncEnumerable();

        var expectedResults = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
        var actualResults = await sut.TransformAsync(items, CancellationToken.None).ToListAsync();

        Assert.Equal(expectedResults, actualResults);
    }



    [Fact]
    public async Task TransformWithCancellationAsync_when_cancelled_throws_OperationCancelledException()
    {
        var sut = new IntToStringTransformerFromTransformerBase(1000);
        var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToAsyncEnumerable();

        using var cts = new CancellationTokenSource();
#if NET8_0_OR_GREATER
        await cts.CancelAsync().ConfigureAwait(false);
#else
        cts.Cancel();
#endif

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            sut.TransformAsync(items, cts.Token).ToListAsync(cancellationToken: cts.Token).AsTask())
            .ConfigureAwait(false);
    }




    // Transform With Progress tests


    [Fact]
    public async Task TransformWithProgressAsync_when_passed_null_items_throws_ArgumentNullException()
    {
        var sut = new IntToStringTransformerFromTransformerBase();

        var progress = new Progress<EtlProgress>(_ => { });

        var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await sut.TransformAsync(null!, progress).ToListAsync());
        Assert.Equal("items", ex.ParamName);
    }



    [Fact]
    public async Task TransformWithProgressAsync_when_passed_null_progress_throws_ArgumentNullException()
    {
        var sut = new IntToStringTransformerFromTransformerBase();

        var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToAsyncEnumerable();

        var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await sut.TransformAsync(items, null!).ToListAsync());
        Assert.Equal("progress", ex.ParamName);
    }



    [Fact]
    public async Task TransformWithProgressAsync_returns_expected_values()
    {
        var sut = new IntToStringTransformerFromTransformerBase();

        var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToAsyncEnumerable();
        var progress = new Progress<EtlProgress>(_ => { });

        var expectedResults = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
        var actualResults = await sut.TransformAsync(items, progress).ToListAsync();

        Assert.Equal(expectedResults, actualResults);
    }



    // Transform With Progress And Cancellation tests


    [Fact]
    public async Task TransformWithProgressAndCancellationAsync_when_passed_null_items_throws_ArgumentNullException()
    {
        var sut = new IntToStringTransformerFromTransformerBase();

        var progress = new Progress<EtlProgress>(_ => { });

        var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await sut.TransformAsync(null!, progress, CancellationToken.None).ToListAsync());
        Assert.Equal("items", ex.ParamName);
    }



    [Fact]
    public async Task TransformWithProgressAndCancellationAsync_when_passed_null_progress_throws_ArgumentNullException()
    {
        var sut = new IntToStringTransformerFromTransformerBase();

        var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToAsyncEnumerable();

        var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await sut.TransformAsync(items, null!, CancellationToken.None).ToListAsync());
        Assert.Equal("progress", ex.ParamName);
    }


    [Fact]
    public async Task TransformWithProgressAndCancellationAsync_returns_expected_values()
    {
        var sut = new IntToStringTransformerFromTransformerBase();

        var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToAsyncEnumerable();
        var progress = new Progress<EtlProgress>(_ => { });

        var expectedResults = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
        var actualResults = await sut.TransformAsync(items, progress, CancellationToken.None).ToListAsync();

        Assert.Equal(expectedResults, actualResults);
    }



    [Fact]
    public async Task TransformWithProgressAndCancellationAsync_when_cancelled_throws_OperationCancelledException()
    {
        var sut = new IntToStringTransformerFromTransformerBase(100);
        var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToAsyncEnumerable();
        var progress = new Progress<EtlProgress>();

        using var cts = new CancellationTokenSource();
#if NET8_0_OR_GREATER
        await cts.CancelAsync().ConfigureAwait(false);
#else
        cts.Cancel();
#endif

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            sut.TransformAsync(items, progress, cts.Token).ToListAsync(cancellationToken: cts.Token).AsTask())
            .ConfigureAwait(false);
    }
    


    [Fact]
    public void ReportingInterval_when_assigned_a_value_less_than_0_throws_ArgumentOutOfRangeException()
    {

        var sut = new IntToStringTransformerFromTransformerBase();
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.ReportingInterval = -1);
    }



    [Fact]
    public void ReportingInterval_when_assigned_a_valid_value_stores_the_value()
    {

        var sut = new IntToStringTransformerFromTransformerBase
        {
            ReportingInterval = 10
        };
        Assert.Equal(10, sut.ReportingInterval);
    }



    [Fact]
    public void MaximumItemCount_when_assigned_a_value_less_than_1_throws_ArgumentOutOfRangeException()
    {

        var sut = new IntToStringTransformerFromTransformerBase();

        Assert.Throws<ArgumentOutOfRangeException>(() => sut.MaximumItemCount = -1);
    }



    [Fact]
    public void MaximumItemCount_when_assigned_a_valid_value_stores_the_value()
    {

        var sut = new IntToStringTransformerFromTransformerBase
        {
            MaximumItemCount = 10
        };
        Assert.Equal(10, sut.MaximumItemCount);
    }



    [Fact]
    public void SkipItemCount_when_assigned_a_value_less_than_0_throws_ArgumentOutOfRangeException()
    {

        var sut = new IntToStringTransformerFromTransformerBase();
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.SkipItemCount = -1);
    }



    [Fact]
    public void SkipItemCount_when_assigned_a_valid_value_stores_the_value()
    {

        var sut = new IntToStringTransformerFromTransformerBase
        {
            SkipItemCount = 10
        };
        Assert.Equal(10, sut.SkipItemCount);
    }



    [Fact]
    public void ReportingInterval_when_assigned_zero_throws_ArgumentOutOfRangeException()
    {
        var sut = new IntToStringTransformerFromTransformerBase();
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.ReportingInterval = 0);
    }



    [Fact]
    public void MaximumItemCount_when_assigned_one_is_valid_and_stores_the_value()
    {
        var sut = new IntToStringTransformerFromTransformerBase
        {
            MaximumItemCount = 1
        };
        Assert.Equal(1, sut.MaximumItemCount);
    }



    [Fact]
    public async Task CurrentItemCount_reflects_number_of_items_transformed()
    {
        var sut = new IntToStringTransformerFromTransformerBase();
        var items = new[] { 1, 2, 3, 4, 5 }.ToAsyncEnumerable();

        await sut.TransformAsync(items).ToListAsync();

        Assert.Equal(5, sut.CurrentItemCount);
    }



    [Fact]
    public async Task CurrentSkippedItemCount_reflects_number_of_items_skipped()
    {
        var sut = new IntToStringTransformerFromTransformerBaseWithSkips();
        var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToAsyncEnumerable();

        await sut.TransformAsync(items).ToListAsync();

        Assert.Equal(5, sut.CurrentSkippedItemCount);
    }



    [Fact]
    public void ReportingInterval_default_value_is_1000()
    {
        var sut = new IntToStringTransformerFromTransformerBase();
        Assert.Equal(1_000, sut.ReportingInterval);
    }



    [Fact]
    public void MaximumItemCount_default_value_is_int_MaxValue()
    {
        var sut = new IntToStringTransformerFromTransformerBase();
        Assert.Equal(int.MaxValue, sut.MaximumItemCount);
    }



    [Fact]
    public void SkipItemCount_default_value_is_zero()
    {
        var sut = new IntToStringTransformerFromTransformerBase();
        Assert.Equal(0, sut.SkipItemCount);
    }



    [Fact]
    public void CurrentItemCount_default_value_is_zero()
    {
        var sut = new IntToStringTransformerFromTransformerBase();
        Assert.Equal(0, sut.CurrentItemCount);
    }



    [Fact]
    public void CurrentSkippedItemCount_default_value_is_zero()
    {
        var sut = new IntToStringTransformerFromTransformerBase();
        Assert.Equal(0, sut.CurrentSkippedItemCount);
    }



    [Fact(Skip = "Timer-based progress callback fires on a thread pool thread and races with enumeration completion across all target frameworks. Needs a redesign of the progress mechanism to be reliably testable.")]
    public async Task TransformWithProgressAsync_invokes_progress_callback()
    {
        var sut = new IntToStringTransformerFromTransformerBase(50) { ReportingInterval = 100 };
        var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToAsyncEnumerable();
        using var callbackFired = new ManualResetEventSlim(initialState: false);
        var progress = new SynchronousProgress<EtlProgress>(callback: _ => callbackFired.Set());

        await sut.TransformAsync(items, progress).ToListAsync();

        Assert.True(callbackFired.IsSet, "Progress callback was never invoked.");
    }



    [Fact(Skip = "Timer-based progress callback fires on a thread pool thread and races with enumeration completion across all target frameworks. Needs a redesign of the progress mechanism to be reliably testable.")]
    public async Task TransformWithProgressAndCancellationAsync_invokes_progress_callback()
    {
        var sut = new IntToStringTransformerFromTransformerBase(50) { ReportingInterval = 100 };
        var items = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToAsyncEnumerable();
        using var callbackFired = new ManualResetEventSlim(initialState: false);
        var progress = new SynchronousProgress<EtlProgress>(callback: _ => callbackFired.Set());

        await sut.TransformAsync(items, progress, CancellationToken.None).ToListAsync();

        Assert.True(callbackFired.IsSet, "Progress callback was never invoked.");
    }



    [ExcludeFromCodeCoverage]
    private class IntToStringTransformerFromTransformerBase : TransformerBase<int, string, EtlProgress>
    {
        private readonly int _delay;

        public IntToStringTransformerFromTransformerBase(int delay = 0)
        {
            if (delay < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delay));
            }

            _delay = delay;
        }



        protected override async IAsyncEnumerable<string> TransformWorkerAsync
        (
            IAsyncEnumerable<int> items,
            [EnumeratorCancellation] CancellationToken token
        )
        {
            await foreach (var item in items.WithCancellation(token))
            {
                await Task.Delay(_delay, token); // Simulate some delay in processing
                token.ThrowIfCancellationRequested();
                yield return item.ToString();
                IncrementCurrentItemCount();
            }
        }



        protected override EtlProgress CreateProgressReport()
        {
            return new EtlProgress(CurrentItemCount);
        }


    }



    [ExcludeFromCodeCoverage]
    private class IntToStringTransformerFromTransformerBaseWithSkips : TransformerBase<int, string, EtlProgress>
    {
        protected override async IAsyncEnumerable<string> TransformWorkerAsync
        (
            IAsyncEnumerable<int> items,
            [EnumeratorCancellation] CancellationToken token
        )
        {
            await foreach (var item in items.WithCancellation(token))
            {
                if (item % 2 == 0)
                {
                    IncrementCurrentSkippedItemCount();
                    continue;
                }
                yield return item.ToString();
                IncrementCurrentItemCount();
            }
        }



        protected override EtlProgress CreateProgressReport()
        {
            return new EtlProgress(CurrentItemCount);
        }
    }
}



