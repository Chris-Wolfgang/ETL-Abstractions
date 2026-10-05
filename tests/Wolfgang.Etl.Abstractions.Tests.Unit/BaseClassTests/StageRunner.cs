using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests;

/// <summary>
/// The items and progress reports one run of a stage produced.
/// </summary>
internal sealed class StageRun<TItem, TProgress>
{
    public List<TItem> Items { get; } = new();



    public List<TProgress> Reports { get; } = new();
}



/// <summary>
/// Runs a stage once, end to end, through its progress overload, so every member a run
/// touches executes: the worker and <c>CreateProgressReport</c> (the base reports once more
/// when the run finishes). Used by the tests that pin what a test double does when it is
/// actually run, for doubles whose own tests only construct, configure or dispose them.
/// </summary>
internal static class StageRunner
{
    public static async Task<StageRun<TSource, TProgress>> ExtractAsync<TSource, TProgress>
    (
        ExtractorBase<TSource, TProgress> extractor
    )
        where TSource : notnull
        where TProgress : notnull
    {
        var run = new StageRun<TSource, TProgress>();
        var progress = new SynchronousProgress<TProgress>(run.Reports.Add);

        run.Items.AddRange(await extractor.ExtractAsync(progress).ToListAsync());

        return run;
    }



    public static async Task<StageRun<TDestination, TProgress>> LoadAsync<TDestination, TProgress>
    (
        LoaderBase<TDestination, TProgress> loader,
        IAsyncEnumerable<TDestination> items
    )
        where TDestination : notnull
        where TProgress : notnull
    {
        var run = new StageRun<TDestination, TProgress>();
        var progress = new SynchronousProgress<TProgress>(run.Reports.Add);

        await loader.LoadAsync(items, progress);

        return run;
    }



    public static async Task<StageRun<TDestination, TProgress>> TransformAsync<TSource, TDestination, TProgress>
    (
        TransformerBase<TSource, TDestination, TProgress> transformer,
        IAsyncEnumerable<TSource> items
    )
        where TSource : notnull
        where TDestination : notnull
        where TProgress : notnull
    {
        var run = new StageRun<TDestination, TProgress>();
        var progress = new SynchronousProgress<TProgress>(run.Reports.Add);

        run.Items.AddRange(await transformer.TransformAsync(items, progress).ToListAsync());

        return run;
    }
}
