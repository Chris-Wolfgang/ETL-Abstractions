namespace Wolfgang.Etl.Abstractions.Tests.Unit.ETL
{
    internal class ConsoleLoaderWithCancellation : ILoadWithCancellationAsync<string>
    {
        public async Task LoadAsync(IAsyncEnumerable<string> items)
        {
            await foreach (var item in items)
            {
                Console.WriteLine(item);
            }
        }

        public async Task LoadAsync(IAsyncEnumerable<string> items, CancellationToken token)
        {
            await foreach (var item in items.WithCancellation(token))
            {
                Console.WriteLine(item);
            }
        }
    }
}
