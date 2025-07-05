using System.Diagnostics.CodeAnalysis;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.InterfaceTests
{
    // ReSharper disable once InconsistentNaming
    public class ILoadWithProgressAsyncTest
    {

        [Fact]
        public async Task ILoadWithProgressAsync_works_with_specified_versions_of_dotnet()
        {

            var items = new List<string>
            {
                "Item1",
                "Item2",
                "Item3"
            }.ToAsyncEnumerable();


            var progress = new Progress<EtlProgress>(p => 
                Console.WriteLine($"Progress: {p.CurrentCount} items processed.")
                );
            
            var sut = new ConsoleLoader();
            
            await sut.LoadAsync(items, progress);
        }



        [ExcludeFromCodeCoverage]
        internal class ConsoleLoader : ILoadWithProgressAsync<string, EtlProgress>
        {
            public Task LoadAsync(IAsyncEnumerable<string> items)
            {
                throw new NotImplementedException();
            }


            public async Task LoadAsync(IAsyncEnumerable<string> items, IProgress<EtlProgress> progress)
            {
                var count = 0;
                await foreach (var item in items)
                {
                    Console.WriteLine(item);
                    progress.Report(new EtlProgress(++count)); // Simulate progress reporting
                }
            }
        }
    }
}
