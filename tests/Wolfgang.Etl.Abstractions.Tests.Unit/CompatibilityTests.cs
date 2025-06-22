using Wolfgang.Etl.Abstractions.Tests.Unit.ETL;

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
            var transformer = new IntToStringWithCancellationTransformer();
            var loader = new ConsoleWithCancellationLoader();

            var token = new CancellationTokenSource().Token;

            await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync(token), token), token);
        }
    }
}