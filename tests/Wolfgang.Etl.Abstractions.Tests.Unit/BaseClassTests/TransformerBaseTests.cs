using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;
using Wolfgang.Etl.TestKit.Xunit;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

public class TransformerBaseTests
    : TransformerBaseContractTests<IdentityTransformer, string, EtlProgress>
{
    protected override IdentityTransformer CreateSut(int itemCount)
    {
        return new IdentityTransformer();
    }



    protected override IReadOnlyList<string> CreateExpectedItems()
    {
        return new[] { "1", "2", "3", "4", "5" };
    }



    protected override IdentityTransformer CreateSutWithTimer(IProgressTimer timer)
    {
        return new IdentityTransformer(timer);
    }



    [Fact]
    public void ReportingInterval_default_value_is_1000()
    {
        var sut = CreateSut(1);
        Assert.Equal(1_000, sut.ReportingInterval);
    }
}



[ExcludeFromCodeCoverage]
public class IdentityTransformer : TransformerBase<string, string, EtlProgress>
{
    private readonly IProgressTimer? _progressTimer;
    private bool _progressTimerWired;



    public IdentityTransformer()
    {
    }



    internal IdentityTransformer(IProgressTimer? progressTimer)
    {
        _progressTimer = progressTimer;
    }



    protected override async IAsyncEnumerable<string> TransformWorkerAsync
    (
        IAsyncEnumerable<string> items,
        [EnumeratorCancellation] CancellationToken token
    )
    {
        var skipped = 0;
        var transformed = 0;

        await foreach (var item in items.WithCancellation(token))
        {
            token.ThrowIfCancellationRequested();

            if (skipped < SkipItemCount)
            {
                skipped++;
                IncrementCurrentSkippedItemCount();
                continue;
            }

            yield return item;
            IncrementCurrentItemCount();
            transformed++;

            if (transformed >= MaximumItemCount)
            {
                yield break;
            }
        }
    }



    protected override EtlProgress CreateProgressReport()
    {
        return new EtlProgress(CurrentItemCount);
    }



    protected override IProgressTimer CreateProgressTimer(IProgress<EtlProgress> progress)
    {
        if (_progressTimer == null)
        {
            return base.CreateProgressTimer(progress);
        }

        if (!_progressTimerWired)
        {
            _progressTimerWired = true;
            _progressTimer.Elapsed += () => progress.Report(CreateProgressReport());
        }

        return _progressTimer;
    }
}
