using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.Abstractions.Tests.Unit;

/// <summary>
/// Covers the #93 middleware mechanism: <see cref="IItemMiddleware{T}"/>,
/// <see cref="MiddlewareResult{T}"/>, and the <c>WithMiddleware</c> stream decorators — including
/// composition inside an <see cref="EtlPipeline"/> <c>Through</c> stage.
/// </summary>
public class MiddlewareTests
{
    // ---------- MiddlewareResult value type ----------

    [Fact]
    public void MiddlewareResult_Continue_keeps_the_item()
    {
        var result = MiddlewareResult.Continue(42);

        Assert.False(result.Skip);
        Assert.Equal(42, result.Item);
    }


    [Fact]
    public void MiddlewareResult_Drop_marks_the_item_skipped()
    {
        var result = MiddlewareResult.Drop<int>();

        Assert.True(result.Skip);
    }


    [Fact]
    public void MiddlewareResult_has_value_equality()
    {
        var a = MiddlewareResult.Continue(7);
        var b = MiddlewareResult.Continue(7);
        var different = MiddlewareResult.Continue(8);
        var dropped = MiddlewareResult.Drop<int>();

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.NotEqual(a, different);
        Assert.True(a != different);
        Assert.NotEqual(a, dropped);
    }


    // ---------- single middleware ----------

    [Fact]
    public async Task WithMiddleware_transforms_each_item()
    {
        var items = await Drain(AsyncSource(1, 2, 3).WithMiddleware(new TimesTenMiddleware()));

        Assert.Equal(new[] { 10, 20, 30 }, items);
    }


    [Fact]
    public async Task WithMiddleware_drops_items_the_middleware_skips()
    {
        var items = await Drain(AsyncSource(1, 2, 3, 4).WithMiddleware(new DropOddMiddleware()));

        Assert.Equal(new[] { 2, 4 }, items);
    }


    [Fact]
    public async Task WithMiddleware_flows_the_cancellation_token_to_the_middleware()
    {
        using var cts = new CancellationTokenSource();
        var capturing = new TokenCapturingMiddleware();

        await Drain(AsyncSource(1).WithMiddleware(capturing), cts.Token);

        Assert.Equal(cts.Token, capturing.LastToken);
    }


    [Fact]
    public async Task WithMiddleware_composes_inside_an_EtlPipeline_Through_stage()
    {
        var stream = EtlPipeline
            .Create()
            .From(AsyncSource(1, 2, 3, 4))
            .Through(s => s.WithMiddleware(new DropOddMiddleware()))
            .Through(s => s.WithMiddleware(new TimesTenMiddleware()))
            .AsAsyncEnumerable();

        var items = await Drain(stream);

        Assert.Equal(new[] { 20, 40 }, items);
    }


    [Fact]
    public void WithMiddleware_when_source_is_null_throws_ArgumentNullException()
    {
        // Validation is eager, so the throw happens at the WithMiddleware call — not on enumeration.
        Assert.Throws<ArgumentNullException>(
            () => ((IAsyncEnumerable<int>)null!).WithMiddleware(new TimesTenMiddleware()));
    }


    [Fact]
    public void WithMiddleware_when_middleware_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => AsyncSource(1).WithMiddleware((IItemMiddleware<int>)null!));
    }


    // ---------- middleware chain ----------

    [Fact]
    public async Task WithMiddleware_chain_runs_in_registration_order_each_seeing_the_previous_output()
    {
        var log = new List<string>();
        var chain = new IItemMiddleware<int>[]
        {
            new RecordingMiddleware(log, "A", add: 10),
            new RecordingMiddleware(log, "B", add: 100),
        };

        var items = await Drain(AsyncSource(1).WithMiddleware(chain));

        Assert.Equal(new[] { 111 }, items);         // 1 -> +10 -> +100
        Assert.Equal(new[] { "A:1", "B:11" }, log);  // B saw A's output
    }


    [Fact]
    public async Task WithMiddleware_chain_stops_at_the_first_drop()
    {
        var log = new List<string>();
        var chain = new IItemMiddleware<int>[]
        {
            new DropOddMiddleware(),
            new RecordingMiddleware(log, "R", add: 0),
        };

        var items = await Drain(AsyncSource(1, 2, 3).WithMiddleware(chain));

        Assert.Equal(new[] { 2 }, items);        // odds dropped before reaching R
        Assert.Equal(new[] { "R:2" }, log);      // R only ran for the surviving even item
    }


    [Fact]
    public async Task WithMiddleware_empty_chain_passes_items_through()
    {
        var items = await Drain(AsyncSource(1, 2, 3).WithMiddleware(Array.Empty<IItemMiddleware<int>>()));

        Assert.Equal(new[] { 1, 2, 3 }, items);
    }


    [Fact]
    public void WithMiddleware_chain_when_source_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => ((IAsyncEnumerable<int>)null!).WithMiddleware(new IItemMiddleware<int>[] { new TimesTenMiddleware() }));
    }


    [Fact]
    public void WithMiddleware_chain_when_middlewares_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(
            () => AsyncSource(1).WithMiddleware((IEnumerable<IItemMiddleware<int>>)null!));

        // The explicit guard names "middlewares"; without it the fallback List ctor would name "collection".
        Assert.Equal("middlewares", ex.ParamName);
    }


    [Fact]
    public void WithMiddleware_chain_when_a_member_is_null_throws_ArgumentNullException()
    {
        var chain = new IItemMiddleware<int>[] { new TimesTenMiddleware(), null! };

        Assert.Throws<ArgumentNullException>(
            () => AsyncSource(1).WithMiddleware(chain));
    }


    // ---------- helpers ----------

    private static async IAsyncEnumerable<int> AsyncSource(params int[] items)
    {
        foreach (var item in items)
        {
            await Task.Yield();
            yield return item;
        }
    }


    private static async Task<List<int>> Drain(IAsyncEnumerable<int> source, CancellationToken token = default)
    {
        var result = new List<int>();
        await foreach (var item in source.WithCancellation(token).ConfigureAwait(false))
        {
            result.Add(item);
        }

        return result;
    }


    // ---------- doubles ----------

    private sealed class TimesTenMiddleware : IItemMiddleware<int>
    {
        public ValueTask<MiddlewareResult<int>> OnItemAsync(int item, CancellationToken token) =>
            new(MiddlewareResult.Continue(item * 10));
    }


    private sealed class DropOddMiddleware : IItemMiddleware<int>
    {
        public ValueTask<MiddlewareResult<int>> OnItemAsync(int item, CancellationToken token) =>
            new(item % 2 == 0 ? MiddlewareResult.Continue(item) : MiddlewareResult.Drop<int>());
    }


    private sealed class TokenCapturingMiddleware : IItemMiddleware<int>
    {
        public CancellationToken LastToken { get; private set; }

        public ValueTask<MiddlewareResult<int>> OnItemAsync(int item, CancellationToken token)
        {
            LastToken = token;
            return new(MiddlewareResult.Continue(item));
        }
    }


    private sealed class RecordingMiddleware : IItemMiddleware<int>
    {
        private readonly List<string> _log;
        private readonly string _name;
        private readonly int _add;

        public RecordingMiddleware(List<string> log, string name, int add)
        {
            _log = log;
            _name = name;
            _add = add;
        }

        public ValueTask<MiddlewareResult<int>> OnItemAsync(int item, CancellationToken token)
        {
            _log.Add($"{_name}:{item}");
            return new(MiddlewareResult.Continue(item + _add));
        }
    }
}
