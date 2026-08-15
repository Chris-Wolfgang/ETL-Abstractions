using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// A fluent builder that composes an extract → (transform) → load pipeline from the
/// <c>Wolfgang.Etl.TestKit</c> doubles — optionally injecting a fault into the extractor or loader —
/// runs it through the <c>Wolfgang.Etl.Abstractions</c> <see cref="EtlPipeline"/>, and asserts the
/// final state (loaded items, aggregate error count, or a terminal exception) in one call.
/// </summary>
/// <typeparam name="T">The item type flowing through the pipeline. Must be <c>notnull</c>.</typeparam>
/// <remarks>
/// Start one with <see cref="EtlScenario.From{T}(T[])"/>. Injected faults default to being
/// <em>skipped</em> (routed through the base error hook and counted in
/// <see cref="EtlPipelineProgress.ErrorItemCount"/>), so the run completes; pass <c>skip: false</c> to
/// let a fault propagate and assert it with <see cref="RunAndAssertThrowsAsync{TException}"/>.
/// </remarks>
public sealed class EtlScenario<T>
    where T : notnull
{
    private readonly T[] _items;
    private (int Index, System.Exception Exception, bool Skip)? _extractorFault;
    private (int Index, System.Exception Exception, bool Skip)? _loaderFault;
    private TransformerBase<T, T, Report>? _transformer;



    internal EtlScenario(T[] items)
    {
        _items = items ?? throw new ArgumentNullException(nameof(items));
    }



    /// <summary>Injects a fault into the extractor at <paramref name="index"/>.</summary>
    /// <param name="index">The zero-based item index at which the extractor throws.</param>
    /// <param name="exception">The exception to throw.</param>
    /// <param name="skip">
    /// When <see langword="true"/> (default) the fault is skipped and counted as an error; when
    /// <see langword="false"/> it propagates and aborts the run.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
    public EtlScenario<T> WithExtractorFault(int index, Exception exception, bool skip = true)
    {
        _extractorFault = (index, exception ?? throw new ArgumentNullException(nameof(exception)), skip);
        return this;
    }



    /// <summary>Injects a fault into the loader at <paramref name="index"/>.</summary>
    /// <param name="index">The zero-based item index at which the loader throws.</param>
    /// <param name="exception">The exception to throw.</param>
    /// <param name="skip">
    /// When <see langword="true"/> (default) the fault is skipped and counted as an error; when
    /// <see langword="false"/> it propagates and aborts the run.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
    public EtlScenario<T> WithLoaderFault(int index, Exception exception, bool skip = true)
    {
        _loaderFault = (index, exception ?? throw new ArgumentNullException(nameof(exception)), skip);
        return this;
    }



    /// <summary>Inserts <paramref name="transformer"/> as a transform stage between source and sink.</summary>
    /// <param name="transformer">The transformer stage.</param>
    /// <exception cref="ArgumentNullException"><paramref name="transformer"/> is <see langword="null"/>.</exception>
    public EtlScenario<T> Through(TransformerBase<T, T, Report> transformer)
    {
        _transformer = transformer ?? throw new ArgumentNullException(nameof(transformer));
        return this;
    }



    /// <summary>
    /// Runs the composed pipeline and asserts the sink received <paramref name="expectedLoaded"/> and
    /// the pipeline reported <paramref name="expectedErrors"/> aggregate errors.
    /// </summary>
    /// <param name="expectedLoaded">The items the loader is expected to have received, in order.</param>
    /// <param name="expectedErrors">The expected aggregate <see cref="EtlPipelineProgress.ErrorItemCount"/>.</param>
    public async Task RunAndAssertAsync(IReadOnlyList<T> expectedLoaded, int expectedErrors = 0)
    {
        var loader  = BuildLoader();
        var capture = new ProgressCapture<EtlPipelineProgress>();

        await BuildSink(loader).RunAsync(capture).ConfigureAwait(false);

        Assert.Equal(expectedLoaded, loader.GetCollectedItems());
        Assert.NotNull(capture.FinalReport);
        Assert.Equal(expectedErrors, capture.FinalReport!.ErrorItemCount);
    }



    /// <summary>
    /// Runs the composed pipeline and asserts it throws <typeparamref name="TException"/> — use with a
    /// fault configured with <c>skip: false</c>.
    /// </summary>
    /// <typeparam name="TException">The expected terminal exception type.</typeparam>
    public Task RunAndAssertThrowsAsync<TException>()
        where TException : Exception
    {
        var sink = BuildSink(BuildLoader());

        return Assert.ThrowsAsync<TException>(() => sink.RunAsync());
    }



    private FaultyExtractor<T> BuildExtractor()
    {
        var extractor = new FaultyExtractor<T>(_items);

        if (_extractorFault is { } fault)
        {
            extractor.ThrowAt(fault.Index, fault.Exception);

            if (fault.Skip)
            {
                extractor.SkipErrors();
            }
        }

        return extractor;
    }



    private FaultyLoader<T> BuildLoader()
    {
        var loader = new FaultyLoader<T>(collectItems: true);

        if (_loaderFault is { } fault)
        {
            loader.ThrowAt(fault.Index, fault.Exception);

            if (fault.Skip)
            {
                loader.SkipErrors();
            }
        }

        return loader;
    }



    private IEtlPipelineSink BuildSink(FaultyLoader<T> loader)
    {
        var source = EtlPipeline
            .Create()
            .From(BuildExtractor());

        return _transformer is null
            ? source.To(loader)
            : source.Through(_transformer).To(loader);
    }
}
