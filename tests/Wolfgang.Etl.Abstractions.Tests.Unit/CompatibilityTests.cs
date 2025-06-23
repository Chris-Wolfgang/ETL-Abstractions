using Wolfgang.Etl.Abstractions.Tests.Unit.ETL;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit
{
    public class CompatibilityTests
    {
        [Fact]
        public async Task Ensure_basic_interfaces_work_with_specified_dotnet_versions()
        {

            var extractor = new FibonacciExtractor();
            var transformer = new IntToStringTransformer();
            var loader = new ConsoleLoader();

            await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync()));
        }



        [Fact]
        public async Task Ensure_interfaces_with_CancellationToken_work_with_specified_dotnet_versions()
        {

            var extractor = new FibonacciWithCancellationExtractor();
            var transformer = new IntToStringTransformerWithCancellation();
            var loader = new ConsoleLoaderWithCancellation();

            var token = new CancellationTokenSource().Token;

            await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync(token), token), token);
        }



        [Fact]
        public async Task Ensure_interfaces_with_Progress_work_with_specified_dotnet_versions()
        {
            var extractor = new FibonacciExtractorWithProgress();
            var transformer = new IntToStringTransformerWithProgress();
            var loader = new ConsoleLoaderWithProgress();

            var extractorProgress = new Progress<EtlProgress>(p => Console.WriteLine($"Progress: {p.CurrentCount} items processed."));
            var transformerProgress = new Progress<EtlProgress>(p => Console.WriteLine($"Progress: {p.CurrentCount} items transformed."));
            var loaderProgress = new Progress<EtlProgress>(p => Console.WriteLine($"Progress: {p.CurrentCount} items loaded."));

            var items = extractor.ExtractAsync(extractorProgress);
            var transformedItems = transformer.TransformAsync(items, transformerProgress);
            await loader.LoadAsync(transformedItems, loaderProgress);

        }




    }
}