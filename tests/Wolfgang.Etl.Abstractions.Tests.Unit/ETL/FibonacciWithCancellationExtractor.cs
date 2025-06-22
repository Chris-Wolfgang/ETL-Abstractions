using System.Runtime.CompilerServices;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.ETL
{
    internal class FibonacciWithCancellationExtractor : IExtractWithCancellationAsync<int>
    {
        public async IAsyncEnumerable<int> ExtractAsync([EnumeratorCancellation] CancellationToken token)
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
