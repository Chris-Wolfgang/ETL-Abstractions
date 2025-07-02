namespace Wolfgang.Etl.Abstractions.Tests.Unit.InterfaceTests
{
    // ReSharper disable once InconsistentNaming
    public class ILoadAsyncTest
    {

        [Fact]
        public async Task ILoadAsync_works_with_specified_versions_of_dotnet()
        {

            var items = new List<string>
            {
                "Item1",
                "Item2",
                "Item3"
            }.ToAsyncEnumerable();


            var sut = new ConsoleLoader();
            
            await sut.LoadAsync(items);
        }



        public class ConsoleLoader : ILoadAsync<string>
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
}
