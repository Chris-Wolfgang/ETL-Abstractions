using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wolfgang.Etl.TestKit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

public class SnapshotTestLoaderTests
{
    // A plain class (not a record) so the test project compiles on the full framework
    // matrix — positional records need IsExternalInit, absent on net47x.
    private sealed class Order
    {
        public Order(int id, string customer)
        {
            Id       = id;
            Customer = customer;
        }



        public int Id { get; }



        public string Customer { get; }



        public override string ToString() => $"Order {{ Id = {Id}, Customer = {Customer} }}";
    }



    // ------------------------------------------------------------------
    // Constructor — argument validation
    // ------------------------------------------------------------------

    [Fact]
    public void Constructor_when_formatter_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new SnapshotTestLoader<int>(null!)
        );
    }



    // ------------------------------------------------------------------
    // Snapshot / LoadedItems before any load
    // ------------------------------------------------------------------

    [Fact]
    public void Snapshot_before_any_load_is_empty()
    {
        var loader = new SnapshotTestLoader<int>();

        Assert.Equal(string.Empty, loader.Snapshot);
    }



    [Fact]
    public void LoadedItems_before_any_load_is_empty()
    {
        var loader = new SnapshotTestLoader<int>();

        Assert.Empty(loader.LoadedItems);
    }



    // ------------------------------------------------------------------
    // Default formatter — ToString per item
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_default_formatter_renders_one_ToString_line_per_item()
    {
        var extractor = new TestExtractor<Order>
        (
            new List<Order> { new(1, "alpha"), new(2, "bravo") }
        );
        var loader = new SnapshotTestLoader<Order>();

        await loader.LoadAsync(extractor.ExtractAsync());

        Assert.Equal
        (
            "Order { Id = 1, Customer = alpha }\nOrder { Id = 2, Customer = bravo }",
            loader.Snapshot
        );
    }



    [Fact]
    public async Task LoadAsync_captures_all_items_in_order()
    {
        var extractor = new TestExtractor<int>(new List<int> { 3, 1, 2 });
        var loader    = new SnapshotTestLoader<int>();

        await loader.LoadAsync(extractor.ExtractAsync());

        Assert.Equal(new[] { 3, 1, 2 }, loader.LoadedItems);
    }



    // ------------------------------------------------------------------
    // Custom formatter — projection + scrubbing
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_custom_formatter_projects_and_scrubs_each_item()
    {
        var extractor = new TestExtractor<Order>
        (
            new List<Order> { new(1, "alpha"), new(2, "bravo") }
        );

        // Project only the customer and scrub the (non-deterministic) id to a placeholder.
        var loader = new SnapshotTestLoader<Order>(o => $"<id>|{o.Customer}");

        await loader.LoadAsync(extractor.ExtractAsync());

        Assert.Equal
        (
            "<id>|alpha\n<id>|bravo",
            loader.Snapshot
        );
    }



    // ------------------------------------------------------------------
    // Line feed is fixed \n regardless of platform
    // ------------------------------------------------------------------

    [Fact]
    public async Task Snapshot_joins_lines_with_a_line_feed_not_Environment_NewLine()
    {
        var extractor = new TestExtractor<int>(new List<int> { 1, 2 });
        var loader    = new SnapshotTestLoader<int>();

        await loader.LoadAsync(extractor.ExtractAsync());

        Assert.Equal("1\n2", loader.Snapshot);
    }



    // ------------------------------------------------------------------
    // SkipItemCount / MaximumItemCount
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_when_SkipItemCount_is_set_skips_the_first_N_items()
    {
        var extractor = new TestExtractor<int>(new List<int> { 1, 2, 3, 4 });
        var loader    = new SnapshotTestLoader<int> { SkipItemCount = 2 };

        await loader.LoadAsync(extractor.ExtractAsync());

        Assert.Equal(new[] { 3, 4 }, loader.LoadedItems);
    }



    [Fact]
    public async Task LoadAsync_when_MaximumItemCount_is_set_captures_at_most_that_many_items()
    {
        var extractor = new TestExtractor<int>(new List<int> { 1, 2, 3, 4 });
        var loader    = new SnapshotTestLoader<int> { MaximumItemCount = 2 };

        await loader.LoadAsync(extractor.ExtractAsync());

        Assert.Equal(new[] { 1, 2 }, loader.LoadedItems);
    }



    // ------------------------------------------------------------------
    // Buffer reset between runs
    // ------------------------------------------------------------------

    [Fact]
    public async Task LoadAsync_clears_the_buffer_before_each_run()
    {
        var loader = new SnapshotTestLoader<int>();

        await loader.LoadAsync(new TestExtractor<int>(new List<int> { 1, 2, 3 }).ExtractAsync());
        await loader.LoadAsync(new TestExtractor<int>(new List<int> { 9 }).ExtractAsync());

        Assert.Equal(new[] { 9 }, loader.LoadedItems);
        Assert.Equal("9", loader.Snapshot);
    }
}
