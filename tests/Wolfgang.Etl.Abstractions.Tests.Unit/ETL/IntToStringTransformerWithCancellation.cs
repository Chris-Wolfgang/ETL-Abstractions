using System.Runtime.CompilerServices;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.ETL
{

    internal class IntToStringTransformerWithCancellation : ITransformWithCancellationAsync<int, string>
    {
        public async IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<int> items)
        {
            await foreach (var item in items)
            {
                yield return item.ToString();
            }
        }



        public async IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<int> items, [EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var item in items.WithCancellation(token))
            {
                yield return item.ToString();
            }
        }
    }
}
