namespace Wolfgang.Etl.Abstractions.Tests.Unit.InterfaceTests
{
    // ReSharper disable once InconsistentNaming
    public class ILoadWithCancellationAsyncTest
    {

        [Fact]
        public async Task ILoadWithCancellationAsync_works_with_specified_versions_of_dotnet()
        {

            var items = new List<string>
            {
                "Item1",
                "Item2",
                "Item3"
            }.ToAsyncEnumerable();


            var sut = new ConsoleLoader();
            
            await sut.LoadAsync(items, CancellationToken.None);
        }



        public class ConsoleLoader : ILoadWithCancellationAsync<string>
        {
            public Task LoadAsync(IAsyncEnumerable<string> items)
            {
                throw new NotImplementedException();
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
}
