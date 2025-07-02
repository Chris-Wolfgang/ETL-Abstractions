using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.BaseClassTests
{
    public class ExtractorBaseTests
    {


        [Fact]
        public async Task ExtractorBase_works_with_specified_versions_of_dotnet()
        {
            var sut = new FibonacciExtractorFromExtractorBase();

            Assert.Equal(10, await sut.ExtractAsync().Take(10).CountAsync());
        }
    }




    public class FibonacciExtractorFromExtractorBase : ExtractorBase<int, EtlProgress>
    {
        protected override async IAsyncEnumerable<int> ExtractWorkerAsync
        (
            [EnumeratorCancellation] CancellationToken token
        )
        {
            var current = 1;
            var previous = 0;
            for (var x = 0; x < 10; ++x)
            {
                await Task.Yield(); // Simulate asynchronous operation
                if (token.IsCancellationRequested)
                {
                    yield break; // Exit if cancellation is requested
                }
                yield return current;
                ++CurrentItemCount;

                var temp = current;
                current += previous;
                previous = temp;
            }
        }

        protected override EtlProgress CreateProgressReport()
        {
            return new EtlProgress(CurrentItemCount);
        }
    }
}
