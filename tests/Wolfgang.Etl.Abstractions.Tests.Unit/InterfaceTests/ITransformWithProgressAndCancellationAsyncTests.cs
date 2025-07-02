using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit.InterfaceTests
{
    // ReSharper disable once InconsistentNaming
    public class ITransformWithProgressAndCancellationAsyncTests
    {

        [Fact]
        public async Task ITransformWithProgressAndCancellationAsync_works_with_specified_versions_of_dotnet()
        {
            var items = new List<int> { 1, 2, 3, 4, 5 };
            var progress = new Progress<EtlProgress>(p => Console.WriteLine($"Progress: {p.CurrentCount} items processed."));

            var sut = new IntToStringTransformer();

            var actual = await sut.TransformAsync(items.ToAsyncEnumerable(), progress, CancellationToken.None).ToListAsync();

            Assert.Equal(["1", "2", "3", "4", "5"], actual);

        }


        public class IntToStringTransformer 
            : ITransformWithProgressAndCancellationAsync<int, string, EtlProgress>
        {
            public IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<int> items)
            {
                throw new NotImplementedException();
            }

            public IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<int> items, CancellationToken token)
            {
                throw new NotImplementedException();
            }

            public IAsyncEnumerable<string> TransformAsync(IAsyncEnumerable<int> items, IProgress<EtlProgress> progress)
            {
                throw new NotImplementedException();
            }



            public async IAsyncEnumerable<string> TransformAsync
            (
                IAsyncEnumerable<int> items, 
                IProgress<EtlProgress> progress, 
                [EnumeratorCancellation] CancellationToken token
            )
            {
                await foreach (var item in items.WithCancellation(token))
                {
                    yield return item.ToString();
                }
            }
        }
    }
}
