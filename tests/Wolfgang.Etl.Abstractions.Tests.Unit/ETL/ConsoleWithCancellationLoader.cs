namespace Wolfgang.Etl.Abstractions.Tests.Unit.ETL
{
    internal class ConsoleWithCancellationLoader : ILoadWithCancellationAsync<string>
    {
        public async Task LoadAsync(IAsyncEnumerable<string> items, CancellationToken token)
        {
            await foreach (var item in items.WithCancellation(token))
            {
                Console.WriteLine(item);
            }
        }
    }
}
