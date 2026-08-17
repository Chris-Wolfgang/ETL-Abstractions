using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

/// <summary>
/// Mutation-hardening tests (#346) for <see cref="SnapshotTestLoader{T}"/>: they pin the two
/// cancellation checks in <c>LoadWorkerAsync</c> — the pre-loop guard and the per-item guard — that
/// the behavioural tests leave as surviving statement mutants.
/// </summary>
public class SnapshotTestLoaderMutationTests
{
    [Fact]
    public async Task LoadAsync_with_empty_source_and_a_pre_cancelled_token_still_throws()
    {
        // With no items the per-item ThrowIfCancellationRequested never runs, so the pre-loop check is
        // the only cancellation guard. The source deliberately ignores the token, so dropping that
        // pre-loop statement would let an already-cancelled run complete silently.
        var loader = new SnapshotTestLoader<int>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            () => loader.LoadAsync(EmptyIgnoringCancellation(), cts.Token)
        );
    }



    [Fact]
    public async Task LoadAsync_cancelled_while_receiving_items_throws_from_the_per_item_check()
    {
        // The source cancels the token as its first item is produced — after the pre-loop check has
        // passed — so only the per-item ThrowIfCancellationRequested can observe it. Without that
        // check both items would be buffered and the load would complete normally.
        var loader = new SnapshotTestLoader<int>();
        using var cts = new CancellationTokenSource();

        await Assert.ThrowsAnyAsync<OperationCanceledException>
        (
            () => loader.LoadAsync(CancelOnFirst(cts), cts.Token)
        );

        // The item that triggered cancellation was never captured.
        Assert.Empty(loader.LoadedItems);
    }



    private static async IAsyncEnumerable<int> EmptyIgnoringCancellation()
    {
        await Task.CompletedTask;
        yield break;
    }



    private static async IAsyncEnumerable<int> CancelOnFirst(CancellationTokenSource cts)
    {
        cts.Cancel();
        yield return 1;
        yield return 2;
        await Task.CompletedTask;
    }
}
