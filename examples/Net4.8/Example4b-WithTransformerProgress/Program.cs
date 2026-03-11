using System;
using System.Threading.Tasks;
using Example4b_WithTransformerProgress.ETL;

namespace Example4b_WithTransformerProgress;

internal class Program
{
    private static async Task Main()
    {
        // Print assembly version
        var assembly = typeof(Program).Assembly;
        var assemblyVersion = assembly.GetName().Version;
        Console.WriteLine($"{ConsoleColors.Green}Assembly Version: {assemblyVersion}{ConsoleColors.Reset}\n");

        // Print .NET Framework version
        var frameworkVersion = Environment.Version;
        Console.WriteLine($"{ConsoleColors.Green}.NET Version: {frameworkVersion}{ConsoleColors.Reset}\n");


        var extractor = new FibonacciExtractor();
        var transformer = new IntToStringTransformer();
        var loader = new ConsoleLoader();

        Console.WriteLine($"{ConsoleColors.Yellow} Starting ETL process...{ConsoleColors.Reset}\n\n");

        var progress = new Progress<EtlProgress>(p =>
        {
            Console.WriteLine($"Transformed {ConsoleColors.Cyan}{p.CurrentCount}{ConsoleColors.Reset} items.");
        });

        // Best practice is to only use one progress reporter per ETL process. Using multiple progress reporters
        // can lead to confusion and inconsistent reporting.
        var sourceItems = extractor.ExtractAsync();
        var transformedItems = transformer.TransformAsync(sourceItems, progress);
        await loader.LoadAsync(transformedItems, progress);

        Console.WriteLine($"\n\n{ConsoleColors.Yellow}ETL process completed.{ConsoleColors.Reset}");
    }
}
