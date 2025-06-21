namespace Wolfgang.Etl.Abstractions.Tests.Unit.ETL
{
    internal class ConsoleLoader : ILoadAsync<string>
    {
        public async Task LoadAsync(IAsyncEnumerable<string> source)
        {
            await foreach (var item in source)
            {
                Console.WriteLine(item);
            }
        }
    }
}
