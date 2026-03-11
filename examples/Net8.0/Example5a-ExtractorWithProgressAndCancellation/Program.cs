using Example5a_ExtractorWithProgressAndCancellation.ETL;

namespace Example5a_ExtractorWithProgressAndCancellation
{
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
                Console.WriteLine($"Extracted {ConsoleColors.Cyan}{p.CurrentCount}{ConsoleColors.Reset} items.");
            });

            // Set a cancellation token to cancel the extraction after 1 second
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            var token = cts.Token;

            // Best practice is to only use one progress reporter per ETL process. Using multiple progress reporters
            // can lead to confusion and inconsistent reporting. This example passes the progress reporter to
            // the extractor, but you could also pass it to the transformer or loader depending on your needs.
            var sourceItems = extractor.ExtractAsync(progress, token);
            var transformedItems = transformer.TransformAsync(sourceItems, token);
            await loader.LoadAsync(transformedItems, token);

            Console.WriteLine($"\n\n{ConsoleColors.Yellow}ETL process completed.{ConsoleColors.Reset}");
        }
    }


    internal record EtlProgress(int CurrentCount);



    internal class ConsoleColors
    {
        public const string Green = "\u001b[32m";
        public const string Yellow = "\u001b[33m";
        public const string Reset = "\u001b[0m";
        public const string Red = "\u001b[31m";
        public const string Cyan = "\u001b[36m";
    }
}
