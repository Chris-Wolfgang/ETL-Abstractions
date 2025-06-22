namespace Wolfgang.Etl.Abstractions.Tests.Unit.ETL
{
    internal class IntToStringTransformer : ITransformAsync<int, string>
    {
        public async IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<int> source)
        {
            await foreach (var item in source)
            {
                yield return item.ToString();
            }
        }
    }
}
