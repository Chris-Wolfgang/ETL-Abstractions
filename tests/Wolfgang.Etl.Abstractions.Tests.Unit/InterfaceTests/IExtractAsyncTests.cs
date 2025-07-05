using System.Diagnostics.CodeAnalysis;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.InterfaceTests
{
    // ReSharper disable once InconsistentNaming
    public class IExtractAsyncTests
    {

        [Fact]
        public async Task IExtractAsync_works_with_specified_versions_of_dotnet()
        {
            var expected = new[] { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55 };

            var sut = new FibonacciExtractor();

            var actual = await sut.ExtractAsync().ToListAsync();

            Assert.Equal(expected, actual);
        }


        [ExcludeFromCodeCoverage]
        internal class FibonacciExtractor : IExtractAsync<int>
        {
            public async IAsyncEnumerable<int> ExtractAsync()
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
        }
    }
}
