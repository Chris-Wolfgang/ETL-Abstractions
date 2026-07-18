using Example7_FluentPipeline.ETL;
using Wolfgang.Etl.Abstractions;

namespace Example7_FluentPipeline;

internal class Program
{
    private static async Task Main()
    {
        Console.WriteLine($"{ConsoleColors.Green}.NET Version: {Environment.Version}{ConsoleColors.Reset}\n");

        Console.WriteLine($"{ConsoleColors.Yellow}Starting ETL process via the fluent Pipeline API...{ConsoleColors.Reset}\n\n");

        // Example1 wires the extractor, transformer, and loader together by hand:
        //
        //     var source      = extractor.ExtractAsync();
        //     var transformed = transformer.TransformAsync(source);
        //     await loader.LoadAsync(transformed);
        //
        // The fluent Pipeline API composes the same three stages into a single
        // strongly-typed chain. The compiler enforces that each stage's output
        // type matches the next stage's input — a mismatch is a build error, not
        // a runtime surprise. Each stage is passed via its most-derived interface
        // so the right overload is selected (see the Pipeline XML docs).
        await Pipeline
            .Extract(new FibonacciExtractor())        // IExtractAsync<int>
            .Transform(new IntToStringTransformer())  // ITransformAsync<int, string>
            .Load(new ConsoleLoader())                // ILoadAsync<string>
            .WithName("fibonacci-demo")               // optional, purely informational
            .RunAsync();

        Console.WriteLine($"\n\n{ConsoleColors.Yellow}ETL process completed.{ConsoleColors.Reset}");
    }
}
