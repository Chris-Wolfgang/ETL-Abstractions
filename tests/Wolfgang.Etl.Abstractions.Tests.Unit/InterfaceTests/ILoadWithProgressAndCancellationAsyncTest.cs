using System.Diagnostics.CodeAnalysis;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.InterfaceTests
{
    // ReSharper disable once InconsistentNaming
    public class ILoadWithProgressAndCancellationAsyncTest
    {

        [Fact]
        public async Task ILoadWithProgressAndCancellationAsync_works_with_specified_versions_of_dotnet()
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
            
            await sut.LoadAsync(items, progress, CancellationToken.None);
        }



        [ExcludeFromCodeCoverage]
        internal class ConsoleLoader : ILoadWithProgressAndCancellationAsync<string, EtlProgress>
        {
            public Task LoadAsync(IAsyncEnumerable<string> items)
            {
                throw new NotImplementedException();
            }

            public Task LoadAsync(IAsyncEnumerable<string> items, IProgress<EtlProgress> progress)
            {
                throw new NotImplementedException();
            }

            public Task LoadAsync(IAsyncEnumerable<string> items, CancellationToken token)
            {
                throw new NotImplementedException();
            }
            
            public async Task LoadAsync(IAsyncEnumerable<string> items, IProgress<EtlProgress> progress, CancellationToken token)
            {
                await foreach (var item in items.WithCancellation(token))
                {
                    Console.WriteLine(item);
                }
            }
        }
    }
}
