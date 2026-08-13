using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract base class providing xUnit contract tests for any type that implements
/// <see cref="ILoadWithProgressAsync{TDestination, TProgress}"/>.
/// </summary>
/// <typeparam name="TSut">
/// The type under test. Must implement
/// <see cref="ILoadWithProgressAsync{TDestination, TProgress}"/>.
/// </typeparam>
/// <typeparam name="TItem">The type of item the loader consumes.</typeparam>
/// <typeparam name="TProgress">The type of the progress report.</typeparam>
/// <example>
/// <code>
/// public class MyLoaderProgressContractTests
///     : LoadWithProgressAsyncContractTests&lt;MyLoader, MyRecord, MyProgress&gt;
/// {
///     protected override MyLoader CreateSut(int itemCount) =>
///         new MyLoader(connectionString);
///
///     protected override IReadOnlyList&lt;MyRecord&gt; CreateSourceItems() =>
///         new List&lt;MyRecord&gt; { new("a"), new("b"), new("c") };
/// }
/// </code>
/// </example>
public abstract class LoadWithProgressAsyncContractTests<TSut, TItem, TProgress>
    where TSut : ILoadWithProgressAsync<TItem, TProgress>
    where TItem : notnull
    where TProgress : notnull
{
    // ------------------------------------------------------------------
    // Factory methods
    // ------------------------------------------------------------------

    /// <summary>Creates the system under test.</summary>
    /// <returns>A new, fully initialised instance of <typeparamref name="TSut"/>.</returns>
    /// <summary>
    /// Creates the system under test configured to yield exactly <paramref name="itemCount"/> items.
    /// </summary>
    /// <param name="itemCount">The number of items the SUT should yield. Pass 0 for an empty source.</param>
    protected abstract TSut CreateSut(int itemCount);

    private const int DefaultItemCount = 5;

    /// <summary>Creates the SUT with <see cref="DefaultItemCount"/> items.</summary>
    private TSut CreateSut() => CreateSut(DefaultItemCount);


    /// <summary>
    /// Returns the source items used to feed the loader. Must return at least 5 items.
    /// </summary>
    protected abstract IReadOnlyList<TItem> CreateSourceItems();




    // ------------------------------------------------------------------
    // Tests
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>LoadAsync(IAsyncEnumerable&lt;TItem&gt;, IProgress&lt;TProgress&gt;)</c>
    /// throws <see cref="ArgumentNullException"/> when <c>progress</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_null_progress_throws_ArgumentNullException_Async()
    {
        var sut = CreateSut();

        var ex = await Assert.ThrowsAsync<ArgumentNullException>
        (
            () => sut.LoadAsync(AsyncEnumerable.Empty<TItem>(), (IProgress<TProgress>)null!)
        ).ConfigureAwait(false);

        Assert.Equal("progress", ex.ParamName);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(IAsyncEnumerable&lt;TItem&gt;, IProgress&lt;TProgress&gt;)</c>
    /// completes without throwing when supplied valid arguments.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_completes_without_throwing_Async()
    {
        var sut = CreateSut();
        var progress = new Progress<TProgress>();

        var exception = await Record.ExceptionAsync
        (
            () => sut.LoadAsync(CreateSourceItems().ToAsyncEnumerable(), progress)
        ).ConfigureAwait(false);

        Assert.Null(exception);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(IAsyncEnumerable&lt;TItem&gt;, IProgress&lt;TProgress&gt;)</c>
    /// invokes the progress callback at least once during loading.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_invokes_callback_at_least_once_Async()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.LoadAsync(CreateSourceItems().ToAsyncEnumerable(), progress).ConfigureAwait(false);

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
}
