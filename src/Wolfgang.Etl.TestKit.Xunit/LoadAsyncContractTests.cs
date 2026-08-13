using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract base class providing xUnit contract tests for any type that implements
/// <see cref="ILoadAsync{TDestination}"/>.
/// </summary>
/// <typeparam name="TSut">
/// The type under test. Must implement <see cref="ILoadAsync{TDestination}"/>.
/// </typeparam>
/// <typeparam name="TItem">The type of item the loader consumes.</typeparam>
/// <remarks>
/// Inherit from this class and implement the abstract factory methods to verify that
/// your loader correctly satisfies the <see cref="ILoadAsync{TDestination}"/> contract.
/// </remarks>
/// <example>
/// <code>
/// public class MyLoaderContractTests
///     : LoadAsyncContractTests&lt;MyLoader, MyRecord&gt;
/// {
///     protected override MyLoader CreateSut(int itemCount) =>
///         new MyLoader(connectionString);
///
///     protected override IReadOnlyList&lt;MyRecord&gt; CreateSourceItems() =>
///         new List&lt;MyRecord&gt; { new("a"), new("b"), new("c") };
/// }
/// </code>
/// </example>
public abstract class LoadAsyncContractTests<TSut, TItem>
    where TSut : ILoadAsync<TItem>
    where TItem : notnull
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
    /// Verifies that <c>LoadAsync(IAsyncEnumerable&lt;TItem&gt;)</c> completes without
    /// throwing when supplied a valid sequence.
    /// </summary>
    [Fact]
    public async Task LoadAsync_completes_without_throwing_Async()
    {
        var sut = CreateSut();

        var exception = await Record.ExceptionAsync
        (
            () => sut.LoadAsync(CreateSourceItems().ToAsyncEnumerable())
        ).ConfigureAwait(false);

        Assert.Null(exception);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(IAsyncEnumerable&lt;TItem&gt;)</c> completes without
    /// throwing when supplied an empty sequence.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_empty_sequence_completes_without_throwing_Async()
    {
        var sut = CreateSut();

        var exception = await Record.ExceptionAsync
        (
            () => sut.LoadAsync(AsyncEnumerable.Empty<TItem>())
        ).ConfigureAwait(false);

        Assert.Null(exception);
    }
}
