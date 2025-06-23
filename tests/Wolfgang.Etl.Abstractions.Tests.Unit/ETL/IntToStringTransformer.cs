namespace Wolfgang.Etl.Abstractions.Tests.Unit.ETL
{
    internal class IntToStringTransformer : ITransformAsync<int, string>
    {
        public async IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<int> items)
        {
            await foreach (var item in items)
            {
                yield return item.ToString();
            }
        }
    }
}
