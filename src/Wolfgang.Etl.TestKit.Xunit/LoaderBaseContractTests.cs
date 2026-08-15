using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract base class providing xUnit contract tests for any type that inherits
/// from <see cref="LoaderBase{TDestination,TProgress}"/>.
/// </summary>
/// <typeparam name="TSut">
/// The type under test. Must inherit from
/// <see cref="LoaderBase{TItem, TProgress}"/>.
/// </typeparam>
/// <typeparam name="TItem">The type of item the loader consumes.</typeparam>
/// <typeparam name="TProgress">The type of the progress report.</typeparam>
/// <remarks>
/// <para>
/// Inherit from this class to get a comprehensive suite of contract tests covering all
/// public behaviour defined by <see cref="LoaderBase{TDestination, TProgress}"/>:
/// </para>
/// <list type="bullet">
///   <item><description>All four <c>LoadAsync</c> overloads complete successfully.</description></item>
///   <item><description>Null guards on <c>items</c> and <c>progress</c> arguments.</description></item>
///   <item><description>Cancellation is honoured on all cancellable overloads.</description></item>
///   <item><description><c>CurrentItemCount</c> is incremented as items are loaded.</description></item>
///   <item><description><c>ReportingInterval</c> rejects values less than 1.</description></item>
///   <item><description><c>MaximumItemCount</c> stops loading at the specified limit.</description></item>
///   <item><description><c>MaximumItemCount</c> rejects values less than 1.</description></item>
///   <item><description><c>SkipItemCount</c> skips the specified number of items.</description></item>
///   <item><description><c>SkipItemCount</c> rejects values less than 0.</description></item>
///   <item><description>Progress callbacks fire when the timer fires.</description></item>
/// </list>
/// <para>
/// You are responsible for implementing <c>MaximumItemCount</c> and
/// <c>SkipItemCount</c> behaviour in your <c>LoadWorkerAsync</c> override.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyLoaderContractTests
///     : LoaderBaseContractTests&lt;MyLoader, MyRecord, MyProgress&gt;
/// {
///     protected override MyLoader CreateSut(int itemCount) =>
///         new MyLoader(connectionString);
///
///     protected override IReadOnlyList&lt;MyRecord&gt; CreateSourceItems() =>
///         new List&lt;MyRecord&gt; { new("a"), new("b"), new("c"), new("d"), new("e") };
/// }
/// </code>
/// </example>
public abstract class LoaderBaseContractTests<TSut, TItem, TProgress>
    where TSut : LoaderBase<TItem, TProgress>
    where TItem : notnull
    where TProgress : notnull
{
    // ------------------------------------------------------------------
    // Factory methods
    // ------------------------------------------------------------------

    /// <summary>
    /// Creates the system under test configured to handle exactly <paramref name="itemCount"/> items.
    /// The items fed to the loader must match the first <paramref name="itemCount"/> items
    /// returned by <see cref="CreateSourceItems"/>.
    /// </summary>
    /// <param name="itemCount">The number of items the SUT should handle. Pass 0 for an empty source.</param>
    protected abstract TSut CreateSut(int itemCount);

    /// <summary>
    /// <b>Deprecated.</b> The contract now drives progress timing via
    /// <see cref="ManualProgressTimerCore"/> and <c>WithManualProgressTimer</c>, which need no
    /// per-component timer plumbing — so overriding this is no longer required. Retained for source
    /// compatibility with existing overrides; remove your override (and the component's
    /// <c>IProgressTimer</c>-injection ctor) and it will be dropped in a future major version.
    /// </summary>
    /// <param name="timer">Unused by the contract.</param>
    /// <returns>A new instance of <typeparamref name="TSut"/> (in existing overrides only).</returns>
    /// <exception cref="NotSupportedException">
    /// Always, if the base (non-overridden) implementation is invoked — the contract no longer calls it.
    /// </exception>
    protected virtual TSut CreateSutWithTimer(IProgressTimer timer) =>
        throw new NotSupportedException
        (
            "CreateSutWithTimer is no longer used by the contract; progress timing is driven via " +
            "ManualProgressTimerCore + WithManualProgressTimer. Remove this override."
        );

    /// <summary>
    /// Returns the source items used to feed the loader. Must return at least 5 items.
    /// The first <c>itemCount</c> items from this list are fed when
    /// <c>CreateSut(itemCount)</c> is called.
    /// </summary>
    protected abstract IReadOnlyList<TItem> CreateSourceItems();


    private const int DefaultItemCount = 5;

    /// <summary>Creates the SUT with <see cref="DefaultItemCount"/> items.</summary>
    private TSut CreateSut() => CreateSut(DefaultItemCount);

    /// <summary>Creates the SUT with an empty source.</summary>
    private TSut CreateSutWithNoItems() => CreateSut(0);

    /// <summary>Returns the default input sequence as an async enumerable.</summary>
    private IAsyncEnumerable<TItem> CreateInputItemsAsync() =>
        CreateSourceItems().ToAsyncEnumerable();

    /// <summary>
    /// Returns an input sequence that pauses after the first item until
    /// <paramref name="gate"/> is released, keeping the load pipeline alive
    /// so the test can call <see cref="ManualProgressTimerCore.Tick"/> mid-flight.
    /// </summary>
    private async IAsyncEnumerable<TItem> CreateGatedInputItemsAsync(TaskCompletionSource<bool> gate)
    {
        var items = CreateSourceItems();
        yield return items[0];
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks — gate is a test-controlled TaskCompletionSource
        await gate.Task.ConfigureAwait(false);
#pragma warning restore VSTHRD003
        for (var i = 1; i < items.Count; i++)
        {
            yield return items[i];
        }
    }



    // ------------------------------------------------------------------
    // LoadAsync(IAsyncEnumerable) — basic load
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>LoadAsync(items)</c> throws <see cref="ArgumentNullException"/>
    /// when <c>items</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_null_items_throws_ArgumentNullException_Async()
    {
        var sut = CreateSut();

        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.LoadAsync((IAsyncEnumerable<TItem>)null!)).ConfigureAwait(false);

        Assert.Equal("items", ex.ParamName);
    }

    /// <summary>
    /// Verifies that <c>LoadAsync(items)</c> completes without error when valid
    /// items are supplied.
    /// </summary>
    [Fact]
    public async Task LoadAsync_completes_successfully_Async()
    {
        var sut = CreateSut();

        var exception = await Record.ExceptionAsync
        (
            () => sut.LoadAsync(CreateInputItemsAsync())
        ).ConfigureAwait(false);

        Assert.Null(exception);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items)</c> completes without error when the source
    /// contains no items.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_empty_source_completes_without_error_Async()
    {
        var sut = CreateSutWithNoItems();

        var exception = await Record.ExceptionAsync
        (
            () => sut.LoadAsync(AsyncEnumerable.Empty<TItem>())
        ).ConfigureAwait(false);

        Assert.Null(exception);
    }



    /// <summary>
    /// Verifies that <c>CurrentItemCount</c> equals the number of items loaded
    /// after a full load via <c>LoadAsync(items)</c>.
    /// </summary>
    [Fact]
    public async Task LoadAsync_increments_CurrentItemCount_for_each_item_Async()
    {
        var sut = CreateSut();
        var expected = CreateSourceItems();

        await sut.LoadAsync(CreateInputItemsAsync()).ConfigureAwait(false);

        Assert.Equal(expected.Count, sut.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // LoadAsync(IAsyncEnumerable, CancellationToken) — cancellation
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>LoadAsync(items, CancellationToken)</c> throws
    /// <see cref="ArgumentNullException"/> when <c>items</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_token_and_null_items_throws_ArgumentNullException_Async()
    {
        var sut = CreateSut();

        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.LoadAsync((IAsyncEnumerable<TItem>)null!, CancellationToken.None)).ConfigureAwait(false);

        Assert.Equal("items", ex.ParamName);
    }

    /// <summary>
    /// Verifies that <c>LoadAsync(items, CancellationToken)</c> completes when passed
    /// <see cref="CancellationToken.None"/>.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_token_completes_successfully_Async()
    {
        var sut = CreateSut();

        var exception = await Record.ExceptionAsync
        (
            () => sut.LoadAsync(CreateInputItemsAsync(), CancellationToken.None)
        ).ConfigureAwait(false);

        Assert.Null(exception);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items, CancellationToken)</c> completes without error
    /// when the source contains no items.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_token_and_empty_source_completes_without_error_Async()
    {
        var sut = CreateSutWithNoItems();

        var exception = await Record.ExceptionAsync
        (
            () => sut.LoadAsync(AsyncEnumerable.Empty<TItem>(), CancellationToken.None)
        ).ConfigureAwait(false);

        Assert.Null(exception);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items, CancellationToken)</c> throws
    /// <see cref="OperationCanceledException"/> when the token is already cancelled.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_cancelled_token_throws_OperationCanceledException_Async()
    {
        var sut = CreateSut();
        using var cts = new CancellationTokenSource();
#if NET8_0_OR_GREATER
        await cts.CancelAsync().ConfigureAwait(false);
#else
        cts.Cancel();
#endif

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            sut.LoadAsync(CreateInputItemsAsync(), cts.Token)).ConfigureAwait(false);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items, CancellationToken)</c> throws
    /// <see cref="OperationCanceledException"/> and stops loading when the token is
    /// cancelled mid-sequence.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_cancellation_token_stops_when_token_is_cancelled_Async()
    {
        var sut = CreateSut();
        var source = CreateSourceItems();
        Assert.True(source.Count >= 3, "CreateSourceItems() must return at least 3 items.");

        using var cts = new CancellationTokenSource();
        var itemsLoaded = 0;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await sut.LoadAsync(CancellingSequenceAsync(), cts.Token).ConfigureAwait(false);

            async IAsyncEnumerable<TItem> CancellingSequenceAsync()
            {
                foreach (var item in source)
                {
                    itemsLoaded++;
                    if (itemsLoaded == 1)
                    {
#if NET8_0_OR_GREATER
                        await cts.CancelAsync().ConfigureAwait(false);
#else
                        cts.Cancel();
#endif
                    }
                    yield return item;
                }
            }
        }).ConfigureAwait(false);

        Assert.Equal(1, itemsLoaded);
    }



    // ------------------------------------------------------------------
    // LoadAsync(IAsyncEnumerable, IProgress) — progress only
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>LoadAsync(items, progress)</c> throws
    /// <see cref="ArgumentNullException"/> when <c>items</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_and_null_items_throws_ArgumentNullException_Async()
    {
        var sut = CreateSut();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.LoadAsync((IAsyncEnumerable<TItem>)null!, progress)).ConfigureAwait(false);

        Assert.Equal("items", ex.ParamName);
    }

    /// <summary>
    /// Verifies that <c>LoadAsync(items, progress)</c> throws
    /// <see cref="ArgumentNullException"/> when <c>progress</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_null_progress_throws_ArgumentNullException_Async()
    {
        var sut = CreateSut();

        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.LoadAsync(CreateInputItemsAsync(), (IProgress<TProgress>)null!)).ConfigureAwait(false);

        Assert.Equal("progress", ex.ParamName);
    }

    /// <summary>
    /// Verifies that <c>LoadAsync(items, progress)</c> completes when valid arguments
    /// are supplied.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_completes_successfully_Async()
    {
        var sut = CreateSut();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        var exception = await Record.ExceptionAsync
        (
            () => sut.LoadAsync(CreateInputItemsAsync(), progress)
        ).ConfigureAwait(false);

        Assert.Null(exception);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items, progress)</c> completes without error when
    /// the source contains no items.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_and_empty_source_completes_without_error_Async()
    {
        var sut = CreateSutWithNoItems();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        var exception = await Record.ExceptionAsync
        (
            () => sut.LoadAsync(AsyncEnumerable.Empty<TItem>(), progress)
        ).ConfigureAwait(false);

        Assert.Null(exception);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items, progress)</c> invokes the progress callback
    /// when the <see cref="IProgressTimer"/> fires.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_invokes_callback_when_timer_fires_Async()
    {
        var timer = new ManualProgressTimerCore();
        var sut = CreateSut();
        sut.WithManualProgressTimer(timer);
        TProgress? captured = default;
        var progress = new SynchronousProgress<TProgress>(r => captured = r);
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var task = sut.LoadAsync(CreateGatedInputItemsAsync(gate), progress);
        timer.Tick();
        gate.SetResult(true);
        await task.ConfigureAwait(false);

        Assert.NotNull(captured);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items, IProgress&lt;TProgress&gt;)</c> invokes the progress
    /// callback at least once during loading when using a standard <see cref="Progress{T}"/>.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_invokes_callback_at_least_once_Async()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.LoadAsync(CreateInputItemsAsync(), progress).ConfigureAwait(false);

        Assert.True(callbackCount >= 1);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items, IProgress&lt;TProgress&gt;)</c> invokes the
    /// progress callback at least once even when the source is empty.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_and_empty_source_invokes_callback_at_least_once_Async()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.LoadAsync(AsyncEnumerable.Empty<TItem>(), progress).ConfigureAwait(false);

        Assert.True(callbackCount >= 1);
    }



    // ------------------------------------------------------------------
    // LoadAsync(IAsyncEnumerable, IProgress, CancellationToken)
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>LoadAsync(items, progress, CancellationToken)</c> throws
    /// <see cref="ArgumentNullException"/> when <c>items</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_and_token_and_null_items_throws_ArgumentNullException_Async()
    {
        var sut = CreateSut();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.LoadAsync((IAsyncEnumerable<TItem>)null!, progress, CancellationToken.None)).ConfigureAwait(false);

        Assert.Equal("items", ex.ParamName);
    }

    /// <summary>
    /// Verifies that <c>LoadAsync(items, progress, CancellationToken)</c> throws
    /// <see cref="ArgumentNullException"/> when <c>progress</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_and_token_and_null_progress_throws_ArgumentNullException_Async()
    {
        var sut = CreateSut();

        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.LoadAsync(CreateInputItemsAsync(), (IProgress<TProgress>)null!, CancellationToken.None)).ConfigureAwait(false);

        Assert.Equal("progress", ex.ParamName);
    }

    /// <summary>
    /// Verifies that <c>LoadAsync(items, progress, CancellationToken)</c> completes
    /// when valid arguments are supplied.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_and_token_completes_successfully_Async()
    {
        var sut = CreateSut();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        var exception = await Record.ExceptionAsync
        (
            () => sut.LoadAsync(CreateInputItemsAsync(), progress, CancellationToken.None)
        ).ConfigureAwait(false);

        Assert.Null(exception);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items, progress, CancellationToken)</c> completes
    /// without error when the source contains no items.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_and_token_and_empty_source_completes_without_error_Async()
    {
        var sut = CreateSutWithNoItems();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        var exception = await Record.ExceptionAsync
        (
            () => sut.LoadAsync(AsyncEnumerable.Empty<TItem>(), progress, CancellationToken.None)
        ).ConfigureAwait(false);

        Assert.Null(exception);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items, progress, CancellationToken)</c> throws
    /// <see cref="OperationCanceledException"/> when the token is already cancelled.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_and_cancelled_token_throws_OperationCanceledException_Async()
    {
        var sut = CreateSut();
        var progress = new SynchronousProgress<TProgress>(_ => { });
        using var cts = new CancellationTokenSource();
#if NET8_0_OR_GREATER
        await cts.CancelAsync().ConfigureAwait(false);
#else
        cts.Cancel();
#endif

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            sut.LoadAsync(CreateInputItemsAsync(), progress, cts.Token)).ConfigureAwait(false);
    }

    /// <summary>
    /// Verifies that <c>LoadAsync(items, progress, CancellationToken)</c> invokes the
    /// progress callback when the <see cref="IProgressTimer"/> fires.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_and_token_invokes_callback_when_timer_fires_Async()
    {
        var timer = new ManualProgressTimerCore();
        var sut = CreateSut();
        sut.WithManualProgressTimer(timer);
        TProgress? captured = default;
        var progress = new SynchronousProgress<TProgress>(r => captured = r);
        var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var task = sut.LoadAsync(CreateGatedInputItemsAsync(gate), progress, CancellationToken.None);
        timer.Tick();
        gate.SetResult(true);
        await task.ConfigureAwait(false);

        Assert.NotNull(captured);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items, IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// invokes the progress callback at least once during loading when using a standard
    /// <see cref="Progress{T}"/>.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_and_token_invokes_callback_at_least_once_Async()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.LoadAsync(CreateInputItemsAsync(), progress, CancellationToken.None).ConfigureAwait(false);

        Assert.True(callbackCount >= 1);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items, IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// invokes the progress callback at least once even when the source is empty.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_and_token_and_empty_source_invokes_callback_at_least_once_Async()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.LoadAsync(AsyncEnumerable.Empty<TItem>(), progress, CancellationToken.None).ConfigureAwait(false);

        Assert.True(callbackCount >= 1);
    }



    // ------------------------------------------------------------------
    // ReportingInterval
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>ReportingInterval</c> defaults to 1000.
    /// </summary>
    [Fact]
    public void ReportingInterval_defaults_to_1000()
    {
        var sut = CreateSut();
        Assert.Equal(1_000, sut.ReportingInterval);
    }

    /// <summary>
    /// Verifies that setting <c>ReportingInterval</c> to a positive value succeeds.
    /// </summary>
    [Fact]
    public void ReportingInterval_can_be_set_to_positive_value()
    {
        var sut = CreateSut();
        sut.ReportingInterval = 1;
        Assert.Equal(1, sut.ReportingInterval);
    }

    /// <summary>
    /// Verifies that setting <c>ReportingInterval</c> to zero throws
    /// <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [Fact]
    public void ReportingInterval_set_to_zero_throws_ArgumentOutOfRangeException()
    {
        var sut = CreateSut();
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.ReportingInterval = 0);
    }

    /// <summary>
    /// Verifies that setting <c>ReportingInterval</c> to a negative value throws
    /// <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [Fact]
    public void ReportingInterval_set_to_negative_throws_ArgumentOutOfRangeException()
    {
        var sut = CreateSut();
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.ReportingInterval = -1);
    }



    // ------------------------------------------------------------------
    // MaximumItemCount
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>MaximumItemCount</c> defaults to <see cref="int.MaxValue"/>.
    /// </summary>
    [Fact]
    public void MaximumItemCount_defaults_to_int_MaxValue()
    {
        var sut = CreateSut();
        Assert.Equal(int.MaxValue, sut.MaximumItemCount);
    }

    /// <summary>
    /// Verifies that setting <c>MaximumItemCount</c> to a positive value succeeds.
    /// </summary>
    [Fact]
    public void MaximumItemCount_can_be_set_to_positive_value()
    {
        var sut = CreateSut();
        sut.MaximumItemCount = 10;
        Assert.Equal(10, sut.MaximumItemCount);
    }

    /// <summary>
    /// Verifies that setting <c>MaximumItemCount</c> to zero throws
    /// <see cref="ArgumentOutOfRangeException"/> — the minimum for a loader is 1.
    /// </summary>
    [Fact]
    public void MaximumItemCount_set_to_zero_throws_ArgumentOutOfRangeException()
    {
        var sut = CreateSut();
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.MaximumItemCount = 0);
    }

    /// <summary>
    /// Verifies that setting <c>MaximumItemCount</c> to a negative value throws
    /// <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [Fact]
    public void MaximumItemCount_set_to_negative_throws_ArgumentOutOfRangeException()
    {
        var sut = CreateSut();
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.MaximumItemCount = -1);
    }

    /// <summary>
    /// Verifies that the loader stops consuming items once <c>MaximumItemCount</c>
    /// is reached.
    /// </summary>
    [Fact]
    public async Task LoadAsync_stops_at_MaximumItemCount_Async()
    {
        var sut = CreateSut();
        var expected = CreateSourceItems();
        Assert.True(expected.Count >= 3, "CreateSourceItems() must return at least 3 items.");

        sut.MaximumItemCount = 1;

        await sut.LoadAsync(CreateInputItemsAsync()).ConfigureAwait(false);

        Assert.Equal(1, sut.CurrentItemCount);
    }

    /// <summary>
    /// Verifies that the loader stops pulling items from its source once
    /// <c>MaximumItemCount</c> is reached, rather than draining the whole source
    /// and discarding the surplus. A loader that over-reads holds upstream
    /// resources (connections, cursors) far longer than necessary.
    /// </summary>
    /// <remarks>
    /// The source is wrapped in a counter. Once the load completes with a limit
    /// of <c>M</c>, the wrapper must have observed at most <c>M + 1</c> pulls —
    /// the <c>+1</c> allows for the single read that discovers the limit has
    /// been reached.
    /// </remarks>
    [Fact]
    public async Task LoadAsync_stops_reading_source_at_MaximumItemCount_Async()
    {
        var sut = CreateSut();
        var source = CreateSourceItems();
        Assert.True(source.Count >= 3, "CreateSourceItems() must return at least 3 items.");

        var pulled = 0;

        async IAsyncEnumerable<TItem> CountingSourceAsync()
        {
            await Task.CompletedTask.ConfigureAwait(false);
            foreach (var item in source)
            {
                pulled++;
                yield return item;
            }
        }

        sut.MaximumItemCount = source.Count - 2;

        await sut.LoadAsync(CountingSourceAsync()).ConfigureAwait(false);

        Assert.Equal(sut.MaximumItemCount, sut.CurrentItemCount);
        Assert.True
        (
            pulled <= sut.MaximumItemCount + 1,
            $"Expected at most {sut.MaximumItemCount + 1} source pulls but the source was read {pulled} times."
        );
    }

    /// <summary>
    /// Verifies that when <c>MaximumItemCount</c> exceeds the sequence length, all items
    /// are still loaded.
    /// </summary>
    [Fact]
    public async Task LoadAsync_loads_all_items_when_MaximumItemCount_exceeds_sequence_length_Async()
    {
        var sut = CreateSut();
        var expected = CreateSourceItems();
        sut.MaximumItemCount = expected.Count + 100;

        await sut.LoadAsync(CreateInputItemsAsync()).ConfigureAwait(false);

        Assert.Equal(expected.Count, sut.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // SkipItemCount
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>SkipItemCount</c> defaults to zero.
    /// </summary>
    [Fact]
    public void SkipItemCount_defaults_to_zero()
    {
        var sut = CreateSut();
        Assert.Equal(0, sut.SkipItemCount);
    }

    /// <summary>
    /// Verifies that setting <c>SkipItemCount</c> to a positive value succeeds.
    /// </summary>
    [Fact]
    public void SkipItemCount_can_be_set_to_positive_value()
    {
        var sut = CreateSut();
        sut.SkipItemCount = 5;
        Assert.Equal(5, sut.SkipItemCount);
    }

    /// <summary>
    /// Verifies that setting <c>SkipItemCount</c> to a negative value throws
    /// <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [Fact]
    public void SkipItemCount_set_to_negative_throws_ArgumentOutOfRangeException()
    {
        var sut = CreateSut();
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.SkipItemCount = -1);
    }

    /// <summary>
    /// Verifies that the loader skips the specified number of items before loading.
    /// </summary>
    [Fact]
    public async Task LoadAsync_skips_items_up_to_SkipItemCount_Async()
    {
        var sut = CreateSut();
        var expected = CreateSourceItems();
        Assert.True(expected.Count >= 3, "CreateSourceItems() must return at least 3 items.");

        sut.SkipItemCount = 1;

        await sut.LoadAsync(CreateInputItemsAsync()).ConfigureAwait(false);

        Assert.Equal(expected.Count - 1, sut.CurrentItemCount);
        Assert.Equal(1, sut.CurrentSkippedItemCount);
    }

    /// <summary>
    /// Verifies that <c>CurrentItemCount</c> is zero on a freshly created loader, before any load
    /// has run.
    /// </summary>
    [Fact]
    public void CurrentItemCount_defaults_to_zero()
    {
        var sut = CreateSut();

        Assert.Equal(0, sut.CurrentItemCount);
    }

    /// <summary>
    /// Verifies that <c>CurrentSkippedItemCount</c> is zero on a freshly created loader, before any
    /// load has run.
    /// </summary>
    [Fact]
    public void CurrentSkippedItemCount_defaults_to_zero()
    {
        var sut = CreateSut();

        Assert.Equal(0, sut.CurrentSkippedItemCount);
    }

    /// <summary>
    /// Verifies that <c>CurrentSkippedItemCount</c> reflects the exact number of items skipped by
    /// <c>SkipItemCount</c> after a run.
    /// </summary>
    [Fact]
    public async Task LoadAsync_CurrentSkippedItemCount_reflects_the_number_of_items_skipped_Async()
    {
        var sut = CreateSut();
        var expected = CreateSourceItems();
        Assert.True(expected.Count >= 3, "CreateSourceItems() must return at least 3 items.");

        sut.SkipItemCount = 2;

        await sut.LoadAsync(CreateInputItemsAsync()).ConfigureAwait(false);

        Assert.Equal(2, sut.CurrentSkippedItemCount);
    }

    /// <summary>
    /// Verifies that <c>CurrentErrorItemCount</c> is zero on a freshly created loader, before any
    /// item has failed (Abstractions 0.18.0 error hook).
    /// </summary>
    [Fact]
    public void CurrentErrorItemCount_defaults_to_zero()
    {
        var sut = CreateSut();

        Assert.Equal(0, sut.CurrentErrorItemCount);
    }



    // ------------------------------------------------------------------
    // No over-read (issue #49)
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that once <c>MaximumItemCount</c> is reached the loader stops pulling from its
    /// source rather than draining it — at most M+1 reads (the +1 discovers the limit).
    /// </summary>
    [Fact]
    public async Task LoadAsync_does_not_over_read_past_MaximumItemCount_Async()
    {
        var sut = CreateSut();
        sut.MaximumItemCount = 3;
        var counter = new PullCounter();

        await sut.LoadAsync(counter.CountAsync(CreateInputItemsAsync())).ConfigureAwait(false);

        Assert.True(counter.Count <= 4, $"Expected at most 4 upstream reads, saw {counter.Count}.");
    }

    // Cancel() runs synchronously to cancel mid-enumeration; CancelAsync is net8.0+ only and
    // this base targets net462+.
#pragma warning disable CA1849, VSTHRD103
    /// <summary>
    /// Verifies that cancelling mid-run stops the loader pulling from its source at the next
    /// check, rather than draining the already-available items.
    /// </summary>
    [Fact]
    public async Task LoadAsync_stops_reading_on_cancellation_Async()
    {
        var sut = CreateSut();
        using var cts = new CancellationTokenSource();
        var counter = new PullCounter();
        var source = counter.CountAsync
        (
            CreateInputItemsAsync(),
            onPull: () => { if (counter.Count == 3) { cts.Cancel(); } },
            token: cts.Token
        );

        try
        {
            await sut.LoadAsync(source, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }

        Assert.True(counter.Count <= 4, $"Expected at most 4 upstream reads, saw {counter.Count}.");
    }
#pragma warning restore CA1849, VSTHRD103

    /// <summary>
    /// Verifies that a pre-cancelled token short-circuits the loader before it pulls any item
    /// from its source.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_a_pre_cancelled_token_reads_nothing_Async()
    {
        var sut = CreateSut();
        var token = new CancellationToken(canceled: true);
        var counter = new PullCounter();

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            () => sut.LoadAsync(counter.CountAsync(CreateInputItemsAsync(), token: token), token)
        ).ConfigureAwait(false);

        Assert.Equal(0, counter.Count);
    }
}
