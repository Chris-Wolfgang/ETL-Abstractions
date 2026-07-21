using Microsoft.CodeAnalysis;

namespace Wolfgang.Etl.Abstractions.Tests.DocExamples;

/// <summary>
/// Guards against XML-doc example rot (#211): every <c>&lt;example&gt;&lt;code&gt;</c>
/// block in the library's documentation comments must still compile against the
/// current public API. A snippet that references a renamed or removed member becomes
/// a failing test instead of silently outliving the API it documents.
/// </summary>
public sealed class DocExampleCompilationTests
{
    public static IEnumerable<object[]> Examples()
        => DocExampleSource.DiscoverAll().Select(example => new object[] { example });


    [Fact]
    public void Source_scan_finds_the_documented_examples()
    {
        var examples = DocExampleSource.DiscoverAll();

        // Floor guard: if extraction (or source-tree location) ever silently breaks, the
        // theory below would pass vacuously with zero cases. This fails loudly instead.
        Assert.True(
            examples.Count >= 5,
            $"Expected to find the documented <example><code> blocks in the library source, "
            + $"but found {examples.Count}. Doc-example extraction or source-tree discovery is broken.");
    }


    [Theory]
    [MemberData(nameof(Examples))]
    public void Documented_example_still_compiles(DocExample example)
    {
        ArgumentNullException.ThrowIfNull(example);
        var errors = DocExampleCompiler.Compile(example);

        Assert.True(
            errors.Count == 0,
            BuildFailureMessage(example, errors));
    }


    private static string BuildFailureMessage(DocExample example, IReadOnlyList<Diagnostic> errors)
    {
        var rendered = string.Join(Environment.NewLine, errors.Select(e => "    " + e));

        return $"The XML-doc <example> at {example.File}:{example.Line} no longer compiles "
            + $"against the current public API:{Environment.NewLine}{rendered}"
            + $"{Environment.NewLine}--- snippet ---{Environment.NewLine}{example.Code}";
    }
}
