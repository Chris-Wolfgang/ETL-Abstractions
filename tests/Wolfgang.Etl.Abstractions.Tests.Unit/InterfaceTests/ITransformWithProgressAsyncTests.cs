using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.InterfaceTests
{
    // ReSharper disable once InconsistentNaming
    public class ITransformWithProgressAsyncTests
    {

        [Fact]
        public async Task ITransformWithProgressAsync_works_with_specified_versions_of_dotnet()
        {
            var items = new List<int> { 1, 2, 3, 4, 5 };
            var progress = new Progress<EtlProgress>(p => Console.WriteLine($"Progress: {p.CurrentCount} items processed."));

            var sut = new IntToStringTransformer();

            var actual = await sut.TransformAsync(items.ToAsyncEnumerable(), progress).ToListAsync();

            Assert.Equal(["1", "2", "3", "4", "5"], actual);

        }


        public class IntToStringTransformer 
            : ITransformWithProgressAsync<int, string, EtlProgress>
        {
            public IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<int> items)
            {
                throw new NotImplementedException();
            }


            public IAsyncEnumerable<string> TransformAsync
            (
                IAsyncEnumerable<int> items, 
                IProgress<EtlProgress> progress
            )
            {
                return items.Select(item => item.ToString());
            }
        }
    }
}
