
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.EtlPipelineTests;

public sealed class EtlPipelineSinkExtensionsTests
{
    private sealed class FakeSink : IEtlPipelineSink
    {
        private readonly Func<Task>? _onRun;

        public FakeSink(Func<Task>? onRun = null) => _onRun = onRun;

        public bool Ran { get; private set; }

        public async Task RunAsync(IProgress<EtlPipelineProgress>? progress = null, CancellationToken token = default)
        {
            Ran = true;
            if (_onRun is not null)
            {
                await _onRun().ConfigureAwait(false);
            }
        }
    }


    private sealed class TrackingDisposable : IDisposable
    {
        private readonly List<string> _log;
        private readonly string _name;

        public TrackingDisposable(List<string> log, string name)
        {
            _log = log;
            _name = name;
        }

        public void Dispose() => _log.Add(_name);
    }


    private sealed class TrackingAsyncDisposable : IAsyncDisposable
    {
        private readonly List<string> _log;
        private readonly string _name;

        public TrackingAsyncDisposable(List<string> log, string name)
        {
            _log = log;
            _name = name;
        }

        public ValueTask DisposeAsync()
        {
            _log.Add(_name);
            return default;
        }
    }


    [Fact]
    public async Task DisposingOwned_disposes_resources_after_a_successful_run()
    {
        var log = new List<string>();
        var sink = new FakeSink();

        await sink.DisposingOwned(new TrackingDisposable(log, "a")).RunAsync();

        Assert.True(sink.Ran);
        Assert.Equal(new[] { "a" }, log);
    }


    [Fact]
    public async Task DisposingOwned_disposes_in_reverse_order()
    {
        var log = new List<string>();
        var sink = new FakeSink();

        await sink
            .DisposingOwned(
                new TrackingDisposable(log, "first"),
                new TrackingAsyncDisposable(log, "second"))
            .RunAsync();

        Assert.Equal(new[] { "second", "first" }, log);
    }


    [Fact]
    public async Task DisposingOwned_disposes_even_when_the_run_throws()
    {
        var log = new List<string>();
        var sink = new FakeSink(() => throw new InvalidOperationException("boom"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sink.DisposingOwned(new TrackingDisposable(log, "a")).RunAsync());

        Assert.Equal(new[] { "a" }, log);
    }


    private sealed class ThrowingDisposable : IDisposable
    {
        public void Dispose() => throw new InvalidOperationException("dispose failed");
    }


    [Fact]
    public async Task DisposingOwned_when_a_resource_throws_on_dispose_still_disposes_the_rest()
    {
        var log = new List<string>();
        var sink = new FakeSink();

        var ex = await Assert.ThrowsAsync<AggregateException>
        (
            () => sink
                .DisposingOwned(new TrackingDisposable(log, "first"), new ThrowingDisposable())
                .RunAsync()
        );

        Assert.IsType<InvalidOperationException>(Assert.Single(ex.InnerExceptions));
        Assert.Equal(new[] { "first" }, log);
    }


    [Fact]
    public async Task DisposingOwned_when_run_and_dispose_both_throw_surfaces_the_run_exception()
    {
        var log = new List<string>();
        var sink = new FakeSink(() => throw new NotSupportedException("run failed"));

        var ex = await Assert.ThrowsAsync<NotSupportedException>
        (
            () => sink
                .DisposingOwned(new TrackingDisposable(log, "first"), new ThrowingDisposable())
                .RunAsync()
        );

        Assert.Equal("run failed", ex.Message);
        Assert.Equal(new[] { "first" }, log);
    }


    [Fact]
    public void DisposingOwned_with_no_resources_returns_the_same_sink()
    {
        var sink = new FakeSink();

        Assert.Same(sink, sink.DisposingOwned());
    }


    [Fact]
    public void DisposingOwned_null_sink_throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => ((IEtlPipelineSink)null!).DisposingOwned(new object()));
    }
}
