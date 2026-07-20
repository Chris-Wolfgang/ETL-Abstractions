using Example8_EtlPipeline.ETL;
using Wolfgang.Etl.Abstractions;

namespace Example8_EtlPipeline;

internal class Program
{
    private static async Task Main()
    {
        Console.WriteLine($"{ConsoleColors.Green}.NET Version: {Environment.Version}{ConsoleColors.Reset}\n");

        Console.WriteLine($"{ConsoleColors.Yellow}Building a generic EtlPipeline with three chained Through stages...{ConsoleColors.Reset}\n\n");

        // Report EtlPipelineProgress as records flow through. The core counts the
        // two ends of the pipeline: RecordsExtracted at the source, RecordsLoaded
        // at the sink — regardless of how many stages sit in between.
        var progress = new Progress<EtlPipelineProgress>(p =>
            Console.WriteLine(
                $"{ConsoleColors.Green}[progress]{ConsoleColors.Reset} " +
                $"extracted={p.RecordsExtracted} loaded={p.RecordsLoaded} " +
                $"elapsed={p.Elapsed.TotalMilliseconds:F0}ms"));

        // From a raw async stream, pipe through three transformer stages, then load.
        // Each Through returns IEtlPipeline<TOut>, so the element type flows
        // string -> int -> double -> string across the chain and the compiler enforces
        // that each stage's output matches the next stage's input.
        await EtlPipeline.Create().From(RawNumbers())
            .Through(new ParseIntTransformer())   // string -> int
            .Through(new DoubleTransformer())     // int    -> double
            .Through(new FormatTransformer())     // double -> string
            .To(new ConsoleLoader())              // LoaderBase<string, Report>
            .RunAsync(progress);

        Console.WriteLine($"\n\n{ConsoleColors.Yellow}Pipeline completed.{ConsoleColors.Reset}");
    }


    // Any IAsyncEnumerable<T> is a valid source via EtlPipeline.Create().From(...).
    // An ExtractorBase<T, TProgress> works too: EtlPipeline.Create().From(myExtractor).
    private static async IAsyncEnumerable<string> RawNumbers()
    {
        for (var i = 1; i <= 8; i++)
        {
            await Task.Delay(50);
            yield return i.ToString();
        }
    }
}
