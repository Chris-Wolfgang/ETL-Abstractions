namespace Wolfgang.Etl.Abstractions.Tests.Unit.ETL
{
    internal class ConsoleLoader : ILoadAsync<string>
    {
        public async Task LoadAsync(IAsyncEnumerable<string> items)
        {
            await foreach (var item in items)
            {
                Console.WriteLine(item);
            }
        }
    }
}
