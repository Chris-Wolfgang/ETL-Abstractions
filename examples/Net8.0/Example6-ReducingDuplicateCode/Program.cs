using Example6_ReducingDuplicateCode.ETL;

namespace Example6_ReducingDuplicateCode;
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


        await EtlWithNoProgressOrCancellation().ConfigureAwait(false);

        
        await EtlWithCancellationToken().ConfigureAwait(false);


        await EtlWithProgress().ConfigureAwait(false);


        await EtlWithProgressAndCancellationToken().ConfigureAwait(false);


        Console.WriteLine($"\n\n{ConsoleColors.Yellow}All ETLs completed.{ConsoleColors.Reset}");
    }


    
    private static async Task EtlWithNoProgressOrCancellation()
    {
        var extractor = new FibonacciExtractor();
        var transformer = new IntToStringTransformer();
        var loader = new ConsoleLoader();

        Console.WriteLine($"{ConsoleColors.Yellow} Starting ETL process with no progress or cancellation...{ConsoleColors.Reset}\n\n");

        var sourceItems = extractor.ExtractAsync();
        var transformedItems = transformer.TransformAsync(sourceItems);
        await loader.LoadAsync(transformedItems).ConfigureAwait(false);

        Console.WriteLine($"\n\n{ConsoleColors.Yellow}ETL process completed.{ConsoleColors.Reset}");

    }




    private static async Task EtlWithCancellationToken()
    {
        Console.WriteLine($"{ConsoleColors.Yellow} Starting ETL process wit cancellation...{ConsoleColors.Reset}\n\n");

        // Set a cancellation token to cancel the extraction after 1 second
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var token = cts.Token;

        var extractor = new FibonacciExtractor();
        var transformer = new IntToStringTransformer();
        var loader = new ConsoleLoader();


        var sourceItems = extractor.ExtractAsync(token);
        var transformedItems = transformer.TransformAsync(sourceItems, token);
        await loader.LoadAsync(transformedItems, token).ConfigureAwait(false);

        Console.WriteLine($"\n\n{ConsoleColors.Yellow}ETL process completed.{ConsoleColors.Reset}");

    }




    private static async Task EtlWithProgress()
    {
        Console.WriteLine($"{ConsoleColors.Yellow} Starting ETL process wit cancellation...{ConsoleColors.Reset}\n\n");

        var progress = new Progress<EtlProgress>(p =>
        {
            Console.WriteLine($"Loaded {ConsoleColors.Cyan}{p.CurrentCount}{ConsoleColors.Reset} items.");
        });


        var extractor = new FibonacciExtractor();
        var transformer = new IntToStringTransformer();
        var loader = new ConsoleLoader();

        // Best practice is to only use one progress reporter per ETL process. Using multiple progress reporters
        // can lead to confusion and inconsistent reporting. This example passes the progress reporter to
        // the loader, but you could also pass it to the extractor or transformer depending on your needs.
        var sourceItems = extractor.ExtractAsync();
        var transformedItems = transformer.TransformAsync(sourceItems);
        await loader.LoadAsync(transformedItems, progress).ConfigureAwait(false);

        Console.WriteLine($"\n\n{ConsoleColors.Yellow}ETL process completed.{ConsoleColors.Reset}");

    }



    private static async Task EtlWithProgressAndCancellationToken()
    {
        Console.WriteLine($"{ConsoleColors.Yellow} Starting ETL process wit cancellation...{ConsoleColors.Reset}\n\n");

        // Set a cancellation token to cancel the extraction after 1 second
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var token = cts.Token;

        var progress = new Progress<EtlProgress>(p =>
        {
            Console.WriteLine($"Loaded {ConsoleColors.Cyan}{p.CurrentCount}{ConsoleColors.Reset} items.");
        });


        var extractor = new FibonacciExtractor();
        var transformer = new IntToStringTransformer();
        var loader = new ConsoleLoader();

        // Best practice is to only use one progress reporter per ETL process. Using multiple progress reporters
        // can lead to confusion and inconsistent reporting. This example passes the progress reporter to
        // the loader, but you could also pass it to the extractor or transformer depending on your needs.
        var sourceItems = extractor.ExtractAsync(token);
        var transformedItems = transformer.TransformAsync(sourceItems, token);
        await loader.LoadAsync(transformedItems, progress, token).ConfigureAwait(false);

        Console.WriteLine($"\n\n{ConsoleColors.Yellow}ETL process completed.{ConsoleColors.Reset}");

    }
}
