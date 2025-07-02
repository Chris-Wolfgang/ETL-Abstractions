using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.InterfaceTests
{
    // ReSharper disable once InconsistentNaming
    public class ILoadWithProgressAndCancellationAsyncTests
    {

        [Fact]
        public async Task IExtractWithProgressAndCancellationAsync_works_with_specified_versions_of_dotnet()
        {
            var expected = new[] { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55 };
            var progress =
                new Progress<EtlProgress>(p => Console.WriteLine($"Progress: {p.CurrentCount} items processed."));

            var sut = new FibonacciExtractor();

            var actual = await sut.ExtractAsync(progress, CancellationToken.None).ToListAsync();

            Assert.Equal(expected, actual);
        }





        internal class FibonacciExtractor
            : IExtractWithProgressAndCancellationAsync<int, EtlProgress>
        {
            public async IAsyncEnumerable<int> ExtractAsync(IProgress<EtlProgress> progress,
                [EnumeratorCancellation] CancellationToken token)
            {
                var current = 1;
                var previous = 0;
                for (var x = 0; x < 10; ++x)
                {
                    yield return current;
                    var temp = current;
                    current += previous;
                    previous = temp;
                    await Task.Yield(); // Simulate asynchronous operation
                }
            }



            public IAsyncEnumerable<int> ExtractAsync()
            {
                throw new NotImplementedException();
            }

            public IAsyncEnumerable<int> ExtractAsync(IProgress<EtlProgress> progress)
            {
                throw new NotImplementedException();
            }

            public IAsyncEnumerable<int> ExtractAsync(CancellationToken token)
            {
                throw new NotImplementedException();
            }
        }
    }
}
