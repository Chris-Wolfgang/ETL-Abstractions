using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit;

/// <summary>
/// Covers the #84 per-item error-handling mechanism on the base classes: the protected
/// <c>HandleItemError</c> helper and <c>OnItemError</c> policy hook, the <see cref="ItemErrorContext"/>
/// and <see cref="ItemErrorAction"/> vocabulary, error counting (a failure is never silent), and the
/// pipeline-level <c>RecordsErrored</c> surface.
/// </summary>
public sealed class ItemErrorHandlingTests
{
    // ---- ItemErrorContext ----

    [Fact]
    public void ItemErrorContext_exposes_its_values()
    {
        var ex = new InvalidOperationException("boom");
        var context = new ItemErrorContext(7, ex, () => "raw line");

        Assert.Equal(7, context.RecordNumber);
        Assert.Same(ex, context.Exception);
        Assert.Equal("raw line", context.RawContent!());
    }


    [Fact]
    public void ItemErrorContext_null_exception_throws()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new ItemErrorContext(1, null!));
        Assert.Equal("exception", ex.ParamName);
    }


    [Fact]
    public void ItemErrorContext_rawContent_is_optional_and_lazy()
    {
        // Default is null (a stage that supplies nothing) ...
        Assert.Null(new ItemErrorContext(1, new Exception()).RawContent);

        // ... and when supplied, it is not evaluated at construction — only when invoked.
        var invoked = false;
        var context = new ItemErrorContext(1, new Exception(), () => { invoked = true; return "x"; });
        Assert.False(invoked);
        _ = context.RawContent!();
        Assert.True(invoked);
    }


    // ---- HandleItemError / OnItemError ----

    [Fact]
    public void HandleItemError_defaults_to_Abort_and_does_not_count()
    {
        var sut = new DefaultPolicyExtractor();

        var action = sut.Handle(new ItemErrorContext(1, new Exception()));

        Assert.Equal(ItemErrorAction.Abort, action);
        Assert.Equal(0, sut.CurrentErrorItemCount);
    }


    [Fact]
    public void HandleItemError_when_policy_skips_returns_Skip_and_increments_the_error_count()
    {
        var sut = new ConfigurableExtractor { Policy = ItemErrorAction.Skip };

        var action = sut.Handle(new ItemErrorContext(1, new Exception()));

        Assert.Equal(ItemErrorAction.Skip, action);
        Assert.Equal(1, sut.CurrentErrorItemCount);
    }


    [Fact]
    public void HandleItemError_when_policy_aborts_returns_Abort_and_does_not_count()
    {
        var sut = new ConfigurableExtractor { Policy = ItemErrorAction.Abort };

        var action = sut.Handle(new ItemErrorContext(1, new Exception()));

        Assert.Equal(ItemErrorAction.Abort, action);
        Assert.Equal(0, sut.CurrentErrorItemCount);
    }


    [Fact]
    public void HandleItemError_passes_the_context_to_the_policy()
    {
        var sut = new ConfigurableExtractor { Policy = ItemErrorAction.Skip };
        var context = new ItemErrorContext(42, new Exception());

        _ = sut.Handle(context);

        Assert.Same(context, sut.LastContext);
        Assert.Equal(1, sut.OnItemErrorCallCount);
    }


    [Fact]
    public void HandleItemError_null_context_throws()
    {
        var sut = new ConfigurableExtractor();
        var ex = Assert.Throws<ArgumentNullException>(() => sut.Handle(null!));
        Assert.Equal("context", ex.ParamName);
    }


    // ---- Worker-level behaviour (the intended usage pattern) ----

    [Fact]
    public async Task Worker_skips_bad_records_and_yields_the_good_ones()
    {
        var extractor = new ParsingExtractor(new[] { "1", "bad", "3", "nope", "5" })
        {
            Policy = ItemErrorAction.Skip,
        };

        var yielded = await Drain(extractor.ExtractAsync());

        Assert.Equal(new[] { 1, 3, 5 }, yielded);
        Assert.Equal(3, extractor.CurrentItemCount);
        Assert.Equal(2, extractor.CurrentErrorItemCount);   // the two bad rows — not silent
    }


    [Fact]
    public async Task Worker_aborts_on_the_first_bad_record_by_default()
    {
        var extractor = new ParsingExtractor(new[] { "1", "bad", "3" });   // default policy = Abort

        await Assert.ThrowsAsync<FormatException>(() => Drain(extractor.ExtractAsync()));
    }


    // ---- Pipeline RecordsErrored surface ----

    [Fact]
    public async Task Pipeline_RecordsErrored_reflects_the_extractor_skips()
    {
        var reports = new List<EtlPipelineProgress>();
        var progress = new SynchronousProgress<EtlPipelineProgress>(reports.Add);
        var extractor = new ParsingExtractor(new[] { "1", "bad", "3", "nope", "5" })
        {
            Policy = ItemErrorAction.Skip,
        };

        await EtlPipeline
            .Create()
            .From(extractor)
            .To(new CollectingLoader())
            .RunAsync(progress);

        var final = reports[^1];
        Assert.Equal(3, final.RecordsExtracted);
        Assert.Equal(3, final.RecordsLoaded);
        Assert.Equal(2, final.RecordsErrored);
    }


    [Fact]
    public async Task Pipeline_RecordsErrored_is_zero_for_a_raw_stream_source()
    {
        var reports = new List<EtlPipelineProgress>();
        var progress = new SynchronousProgress<EtlPipelineProgress>(reports.Add);

        await EtlPipeline
            .Create()
            .From(AsyncSource(1, 2, 3))
            .To(new CollectingLoader())
            .RunAsync(progress);

        Assert.Equal(0, reports[^1].RecordsErrored);
    }


    // ---- helpers / doubles ----

    private static async IAsyncEnumerable<int> AsyncSource(params int[] items)
    {
        foreach (var item in items)
        {
            await Task.Yield();
            yield return item;
        }
    }


    private static async Task<List<int>> Drain(IAsyncEnumerable<int> source)
    {
        var result = new List<int>();
        await foreach (var item in source)
        {
            result.Add(item);
        }

        return result;
    }


    // Overrides OnItemError so the Skip/count path is exercised; exposes the protected helper.
    [ExcludeFromCodeCoverage]
    private sealed class ConfigurableExtractor : ExtractorBase<int, EtlProgress>
    {
        public ItemErrorAction Policy { get; set; } = ItemErrorAction.Abort;

        public ItemErrorContext? LastContext { get; private set; }

        public int OnItemErrorCallCount { get; private set; }

        public ItemErrorAction Handle(ItemErrorContext context) => HandleItemError(context);

        protected override ItemErrorAction OnItemError(ItemErrorContext context)
        {
            OnItemErrorCallCount++;
            LastContext = context;
            return Policy;
        }

        protected override async IAsyncEnumerable<int> ExtractWorkerAsync(
            [EnumeratorCancellation] CancellationToken token)
        {
            await Task.CompletedTask;
            yield break;
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    // Does NOT override OnItemError — exercises the base default (Abort, no count).
    [ExcludeFromCodeCoverage]
    private sealed class DefaultPolicyExtractor : ExtractorBase<int, EtlProgress>
    {
        public ItemErrorAction Handle(ItemErrorContext context) => HandleItemError(context);

        protected override async IAsyncEnumerable<int> ExtractWorkerAsync(
            [EnumeratorCancellation] CancellationToken token)
        {
            await Task.CompletedTask;
            yield break;
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    // A realistic worker: parse each line, skip or abort on failure per policy.
    [ExcludeFromCodeCoverage]
    private sealed class ParsingExtractor : ExtractorBase<int, EtlProgress>
    {
        private readonly string[] _lines;

        public ParsingExtractor(string[] lines) => _lines = lines;

        public ItemErrorAction Policy { get; set; } = ItemErrorAction.Abort;

        protected override ItemErrorAction OnItemError(ItemErrorContext context) => Policy;

        protected override async IAsyncEnumerable<int> ExtractWorkerAsync(
            [EnumeratorCancellation] CancellationToken token)
        {
            long recordNumber = 0;
            foreach (var line in _lines)
            {
                recordNumber++;
                await Task.Yield();

                int value;
                try
                {
                    value = int.Parse(line, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (FormatException ex)
                {
                    if (HandleItemError(new ItemErrorContext(recordNumber, ex, () => line)) == ItemErrorAction.Abort)
                    {
                        throw;
                    }

                    continue;
                }

                IncrementCurrentItemCount();
                yield return value;
            }
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }


    [ExcludeFromCodeCoverage]
    private sealed class CollectingLoader : LoaderBase<int, EtlProgress>
    {
        protected override async Task LoadWorkerAsync(IAsyncEnumerable<int> items, CancellationToken token)
        {
            await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
            {
                _ = item;
                IncrementCurrentItemCount();
            }
        }

        protected override EtlProgress CreateProgressReport() => new(CurrentItemCount);
    }
}
