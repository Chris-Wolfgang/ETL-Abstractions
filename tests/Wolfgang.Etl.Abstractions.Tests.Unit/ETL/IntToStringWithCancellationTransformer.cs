using System.Runtime.CompilerServices;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.ETL
{
    internal class IntToStringWithCancellationTransformer : ITransformWithCancellationAsync<int, string>
    {
        public async IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<int> source, [EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var item in source.WithCancellation(token))
            {
                yield return item.ToString();
            }
        }
    }
}
