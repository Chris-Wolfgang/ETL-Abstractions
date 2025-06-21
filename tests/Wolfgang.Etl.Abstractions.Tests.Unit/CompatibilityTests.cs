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
    }
}