using Example2_WithCancellationToken.ETL;

namespace Example2_WithCancellationToken;

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

        // Set a cancellation token to cancel the extraction after 1 second
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var token = cts.Token;

        try
        {
            var sourceItems = extractor.ExtractAsync(token);
            var transformedItems = transformer.TransformAsync(sourceItems, token);
            await loader.LoadAsync(transformedItems, token);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        Console.WriteLine($"\n\n{ConsoleColors.Yellow}ETL process completed.{ConsoleColors.Reset}");
    }
}

