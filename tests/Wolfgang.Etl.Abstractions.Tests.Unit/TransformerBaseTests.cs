using System.Runtime.CompilerServices;
using Wolfgang.Etl.Abstractions.Tests.Unit.Models;

namespace Wolfgang.Etl.Abstractions.Tests.Unit
{
    public class TransformerBaseTests
    {

        [Fact]
        public async Task TransformerBase_works_with_specified_versions_of_dotnet()
        {
            var numbers = new[] { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55 };
            var sut = new IntToStringTransformerFromTransformerBase();

            var actual = await sut.TransformAsync(numbers.ToAsyncEnumerable()).ToListAsync();
            var expected = numbers.Select(n => n.ToString()).ToList();

            Assert.Equal(expected, actual);
        }

    }



    public class IntToStringTransformerFromTransformerBase : TransformerBase<int, string, EtlProgress>
    {
        protected override async IAsyncEnumerable<string> TransformWorkerAsync
        (
            IAsyncEnumerable<int> items, 
            [EnumeratorCancellation] CancellationToken token
        )
        {
            await foreach (var item in items.WithCancellation(token))
            {
                yield return item.ToString();
                ++CurrentItemCount;
            }
        }

        protected override EtlProgress CreateProgressReport()
        {
            return new EtlProgress(CurrentItemCount);
        }
    }
}
