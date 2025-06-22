using Example3_ExtractorWithGracefulCancellation.ETL;
using Example3_WithGracefulCancellation.ETL;

namespace Example3_ExtractorWithGracefulCancellation
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

            // Set a cancellation token to cancel the extraction after 1 second
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

            var sourceItems = extractor.ExtractAsync(cts.Token);
            var transformedItems = transformer.TransformAsync(sourceItems);
            await loader.LoadAsync(transformedItems);

            Console.WriteLine($"\n\n{ConsoleColors.Yellow}ETL process completed.{ConsoleColors.Reset}");
        }
    }


    internal class ConsoleColors
    {
        public const string Green = "\u001b[32m";
        public const string Yellow = "\u001b[33m";
        public const string Reset = "\u001b[0m";
        public const string Red = "\u001b[31m";
    }
}
