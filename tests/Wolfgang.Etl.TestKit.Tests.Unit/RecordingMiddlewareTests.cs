using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

public class RecordingMiddlewareTests
{
    [Fact]
    public void Constructor_when_policy_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new RecordingMiddleware<int>(null!)
        );
    }



    [Fact]
    public async Task WithMiddleware_default_records_every_item_and_keeps_them_flowing()
    {
        var recorder = new RecordingMiddleware<int>();
        var source   = new[] { 1, 2, 3 }.ToAsyncEnumerable();

        var kept = await source.WithMiddleware(recorder, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        Assert.Equal(new[] { 1, 2, 3 }, kept);
        Assert.Equal(new[] { 1, 2, 3 }, recorder.Observed);
    }



    [Fact]
    public async Task WithMiddleware_transform_policy_maps_each_item()
    {
        var shaping = new RecordingMiddleware<int>(i => MiddlewareResult.Continue(i * 10));
        var source  = new[] { 1, 2, 3 }.ToAsyncEnumerable();

        var kept = await source.WithMiddleware(shaping, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        Assert.Equal(new[] { 10, 20, 30 }, kept);
        // Observed reflects the items as received, before transformation.
        Assert.Equal(new[] { 1, 2, 3 }, shaping.Observed);
    }



    [Fact]
    public async Task WithMiddleware_drop_policy_filters_but_still_observes_all()
    {
        var evensOnly = new RecordingMiddleware<int>
        (
            i => i % 2 == 0 ? MiddlewareResult.Continue(i) : MiddlewareResult.Drop<int>()
        );
        var source = new[] { 1, 2, 3, 4, 5, 6 }.ToAsyncEnumerable();

        var kept = await source.WithMiddleware(evensOnly, CancellationToken.None).ToListAsync().ConfigureAwait(false);

        Assert.Equal(new[] { 2, 4, 6 }, kept);
        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6 }, evensOnly.Observed);
    }



    [Fact]
    public async Task WithMiddleware_applies_multiple_middlewares_in_order()
    {
        var first  = new RecordingMiddleware<int>(i => MiddlewareResult.Continue(i + 1));
        var second = new RecordingMiddleware<int>();
        var source = new[] { 1, 2, 3 }.ToAsyncEnumerable();

        var kept = await source
            .WithMiddleware(new IItemMiddleware<int>[] { first, second }, CancellationToken.None)
            .ToListAsync()
            .ConfigureAwait(false);

        Assert.Equal(new[] { 2, 3, 4 }, kept);
        // `second` sees the output of `first`.
        Assert.Equal(new[] { 1, 2, 3 }, first.Observed);
        Assert.Equal(new[] { 2, 3, 4 }, second.Observed);
    }
}
