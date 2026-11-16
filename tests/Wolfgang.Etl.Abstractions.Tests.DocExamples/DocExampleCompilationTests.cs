using System.Diagnostics.CodeAnalysis;
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


    [Fact]
    public void Source_scan_takes_code_from_every_example_element()
    {
        var scan = DocExampleSource.Scan();

        // An <example> written in a layout the extractor does not recognise would otherwise be
        // skipped silently and never compiled.
        Assert.Equal(scan.ExampleTagCount, scan.ExamplesWithCodeCount);
    }


    [Fact]
    public void Source_scan_covers_the_ErrorPolicies_package()
    {
        var examples = DocExampleSource.DiscoverAll();

        Assert.Contains(examples, example => example.File.StartsWith("Wolfgang.Etl.ErrorPolicies/", StringComparison.Ordinal));
        Assert.Contains(examples, example => example.File.StartsWith("Wolfgang.Etl.Abstractions/", StringComparison.Ordinal));
    }


    [Fact]
    public void ScanLines_reads_tags_that_share_a_line()
    {
        var scan = new DocExampleScan();
        string[] lines =
        [
            "/// <example><code>",
            "/// var x = 1;",
            "///",
            "/// </code></example>",
        ];

        DocExampleSource.ScanLines(lines, "File.cs", scan);

        var example = Assert.Single(scan.Examples);
        Assert.Equal("var x = 1;\n", example.Code);
        Assert.Equal(2, example.Line);
        Assert.Equal(1, scan.ExampleTagCount);
        Assert.Equal(1, scan.ExamplesWithCodeCount);
    }


    [Fact]
    public void ScanLines_counts_an_example_without_code_as_not_extracted()
    {
        var scan = new DocExampleScan();
        string[] lines =
        [
            "/// <example>",
            "/// Prose only.",
            "/// </example>",
        ];

        DocExampleSource.ScanLines(lines, "File.cs", scan);

        Assert.Empty(scan.Examples);
        Assert.Equal(1, scan.ExampleTagCount);
        Assert.Equal(0, scan.ExamplesWithCodeCount);
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


    // ---- helper coverage: the branches the doc-example corpus never exercises ----

    [Theory]
    [InlineData("var x = myawait;", "await", false)]           // only inside a longer identifier
    [InlineData("myawait; await y;", "await", true)]           // first hit is embedded, a later one is whole
    [InlineData("await", "await", true)]                       // whole input
    [InlineData("awaitable", "await", false)]                  // prefix of a longer identifier
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "InlineData supplies non-null literals; the analyzer follows the call into ContainsWord.")]
    public void ContainsWord_matches_whole_identifiers_only(string code, string word, bool expected)
    {
        Assert.Equal(expected, DocExampleCompiler.ContainsWord(code, word));
    }


    [Fact]
    public void AddIfUnseen_adds_a_path_once_and_skips_empty_and_repeated_paths()
    {
        var references = new List<MetadataReference>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var path = typeof(EtlPipeline).Assembly.Location;

        DocExampleCompiler.AddIfUnseen(references, seen, null);
        DocExampleCompiler.AddIfUnseen(references, seen, string.Empty);
        DocExampleCompiler.AddIfUnseen(references, seen, path);
        DocExampleCompiler.AddIfUnseen(references, seen, path);

        Assert.Single(references);
        Assert.Contains(path, seen);
    }


    [Fact]
    public void LocateSourceDirectory_throws_when_no_source_tree_is_above_the_start_directory()
    {
        var start = Path.GetTempPath();

        var exception = Assert.Throws<DirectoryNotFoundException>(() => DocExampleSource.LocateSourceDirectory(start));

        Assert.Contains(start, exception.Message, StringComparison.Ordinal);
    }
}
