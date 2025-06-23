using System;
using System.Threading.Tasks;
using Example4a_WithExtractorProgress.ETL;
using Example4c_WithLoaderProgress.ETL;

namespace Example4a_WithExtractorProgress
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

            // Best practice is to only use one progress reporter per ETL process. Using multiple progress reporters
            // can lead to confusion and inconsistent reporting.
            var sourceItems = extractor.ExtractAsync(progress);
            var transformedItems = transformer.TransformAsync(sourceItems);
            await loader.LoadAsync(transformedItems);

            Console.WriteLine($"\n\n{ConsoleColors.Yellow}ETL process completed.{ConsoleColors.Reset}");
        }
    }



    internal class EtlProgress(int currentCount)
    {
        public int CurrentCount { get; } = currentCount;
    }



    internal class ConsoleColors
    {
        public const string Green = "\u001b[32m";
        public const string Yellow = "\u001b[33m";
        public const string Reset = "\u001b[0m";
        public const string Red = "\u001b[31m";
        public const string Cyan = "\u001b[36m";
    }
}
