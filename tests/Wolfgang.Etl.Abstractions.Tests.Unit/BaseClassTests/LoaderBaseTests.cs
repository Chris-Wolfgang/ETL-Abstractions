using System.Diagnostics.CodeAnalysis;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;
using Wolfgang.Etl.TestKit.Xunit;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

public class LoaderBaseTests
    : LoaderBaseContractTests<ListLoader, string, EtlProgress>
{
    protected override ListLoader CreateSut(int itemCount)
    {
        return new ListLoader();
    }



    protected override IReadOnlyList<string> CreateSourceItems()
    {
        return new[] { "1", "2", "3", "4", "5" };
    }



    protected override ListLoader CreateSutWithTimer(IProgressTimer timer)
    {
        return new ListLoader(timer);
    }



    [Fact]
    public void ReportingInterval_default_value_is_1000()
    {
        var sut = CreateSut(1);
        Assert.Equal(1_000, sut.ReportingInterval);
    }
}



[ExcludeFromCodeCoverage]
public class ListLoader : LoaderBase<string, EtlProgress>
{
    private readonly List<string> _buffer = [];
    private readonly IProgressTimer? _progressTimer;
    private bool _progressTimerWired;



    public ListLoader()
    {
    }



    internal ListLoader(IProgressTimer? progressTimer)
    {
        _progressTimer = progressTimer;
    }



    public IReadOnlyList<string> LoadedItems => _buffer;



    protected override async Task LoadWorkerAsync(IAsyncEnumerable<string> items, CancellationToken token)
    {
        var skipped = 0;
        var loaded = 0;

        await foreach (var item in items.WithCancellation(token))
        {
            token.ThrowIfCancellationRequested();

            if (skipped < SkipItemCount)
            {
                skipped++;
                IncrementCurrentSkippedItemCount();
                continue;
            }

            _buffer.Add(item);
            IncrementCurrentItemCount();
            loaded++;

            if (loaded >= MaximumItemCount)
            {
                break;
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
