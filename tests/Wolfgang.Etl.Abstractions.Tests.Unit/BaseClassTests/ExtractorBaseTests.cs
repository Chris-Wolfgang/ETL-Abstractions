using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;
using Wolfgang.Etl.TestKit.Xunit;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

public class ExtractorBaseTests
    : ExtractorBaseContractTests<SequenceExtractor, int, EtlProgress>
{
    protected override SequenceExtractor CreateSut(int itemCount)
    {
        return new SequenceExtractor(itemCount);
    }



    protected override IReadOnlyList<int> CreateExpectedItems()
    {
        return new[] { 1, 2, 3, 4, 5 };
    }



    protected override SequenceExtractor CreateSutWithTimer(IProgressTimer timer)
    {
        return new SequenceExtractor(5, timer);
    }



    [Fact]
    public void ReportingInterval_default_value_is_1000()
    {
        var sut = CreateSut(1);
        Assert.Equal(1_000, sut.ReportingInterval);
    }
}



[ExcludeFromCodeCoverage]
public class SequenceExtractor : ExtractorBase<int, EtlProgress>
{
    private readonly int _itemCount;
    private readonly IProgressTimer? _progressTimer;
    private bool _progressTimerWired;



    public SequenceExtractor(int itemCount)
    {
        _itemCount = itemCount;
    }



    internal SequenceExtractor(int itemCount, IProgressTimer? progressTimer)
        : this(itemCount)
    {
        _progressTimer = progressTimer;
    }



    protected override async IAsyncEnumerable<int> ExtractWorkerAsync
    (
        [EnumeratorCancellation] CancellationToken token
    )
    {
        for (var i = 1; i <= _itemCount; ++i)
        {
            await Task.CompletedTask;
            token.ThrowIfCancellationRequested();

            if (i > SkipItemCount)
            {
                yield return i;
                IncrementCurrentItemCount();

                if (CurrentItemCount >= MaximumItemCount)
                {
                    yield break;
                }
            }
            else
            {
                IncrementCurrentSkippedItemCount();
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
