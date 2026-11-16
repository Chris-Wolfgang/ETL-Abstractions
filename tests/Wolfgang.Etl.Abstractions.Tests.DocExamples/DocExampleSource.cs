using System.Net;
using Xunit.Abstractions;

namespace Wolfgang.Etl.Abstractions.Tests.DocExamples;

/// <summary>
/// A single <c>&lt;example&gt;&lt;code&gt;</c> block extracted from the library's
/// XML-doc comments, together with the source location it came from so a failure
/// can point back at the exact doc comment to fix.
/// </summary>
public sealed class DocExample : IXunitSerializable
{
    // Parameterless ctor + settable members are required by IXunitSerializable so
    // xunit can round-trip the case through the test runner.
    public DocExample()
    {
    }


    public DocExample(string file, int line, string code)
    {
        File = file;
        Line = line;
        Code = code;
    }


    /// <summary>Repository-relative path (forward-slashed) of the file the example lives in.</summary>
    public string File { get; set; } = string.Empty;


    /// <summary>1-based line number of the first line of the <c>&lt;code&gt;</c> block.</summary>
    public int Line { get; set; }


    /// <summary>The decoded snippet text (XML entities resolved, <c>///</c> prefixes stripped).</summary>
    public string Code { get; set; } = string.Empty;


    public void Deserialize(IXunitSerializationInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);
        File = info.GetValue<string>("file");
        Line = info.GetValue<int>("line");
        Code = info.GetValue<string>("code");
    }


    public void Serialize(IXunitSerializationInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);
        info.AddValue("file", File);
        info.AddValue("line", Line);
        info.AddValue("code", Code);
    }


    // Drives the theory's per-case display name.
    public override string ToString() => $"{File}:{Line}";
}


/// <summary>
/// What a scan of the library source found: the extracted code blocks, plus two counts that
/// let a test prove no <c>&lt;example&gt;</c> was skipped.
/// </summary>
public sealed class DocExampleScan
{
    /// <summary>Every <c>&lt;code&gt;</c> block found inside an <c>&lt;example&gt;</c>.</summary>
    public List<DocExample> Examples { get; } = new();


    /// <summary>
    /// The number of doc-comment lines that open an <c>&lt;example</c> element, counted from the
    /// raw text and so independent of which tag layouts the extractor recognises.
    /// </summary>
    public int ExampleTagCount { get; set; }


    /// <summary>The number of <c>&lt;example&gt;</c> elements the extractor took code from.</summary>
    public int ExamplesWithCodeCount { get; set; }
}


/// <summary>
/// Locates the library source tree and scans it for <c>&lt;example&gt;&lt;code&gt;</c>
/// blocks in <c>///</c> documentation comments.
/// </summary>
public static class DocExampleSource
{
    // The packages whose doc examples this project compiles. The TestKit packages have their
    // own DocExamples project.
    private static readonly string[] ScannedPackages =
    [
        "Wolfgang.Etl.Abstractions",
        "Wolfgang.Etl.ErrorPolicies",
    ];

    private static readonly string[] BlockTags = ["<example>", "</example>", "<code>", "</code>"];


    /// <summary>
    /// Finds every <c>&lt;example&gt;&lt;code&gt;</c> block in the scanned packages' source files.
    /// </summary>
    public static IReadOnlyList<DocExample> DiscoverAll() => Scan().Examples;


    /// <summary>
    /// Scans the source files of the packages this project covers.
    /// </summary>
    public static DocExampleScan Scan()
    {
        var sourceRoot = Path.GetDirectoryName(LocateSourceDirectory(AppContext.BaseDirectory))!;
        var scan = new DocExampleScan();

        foreach (var package in ScannedPackages)
        {
            var packageDirectory = Path.Combine(sourceRoot, package);
            foreach (var file in Directory.EnumerateFiles(packageDirectory, "*.cs", SearchOption.AllDirectories))
            {
                if (IsGeneratedPath(file))
                {
                    continue;
                }

                var relative = Path.GetRelativePath(sourceRoot, file).Replace('\\', '/');
                ScanLines(File.ReadAllLines(file), relative, scan);
            }
        }

        return scan;
    }


    // Internal so a test can feed it tag layouts the library source does not currently use.
    internal static void ScanLines(string[] lines, string relative, DocExampleScan scan)
    {
        var inExample = false;
        var inCode = false;
        var exampleHasCode = false;
        var codeStartLine = 0;
        var buffer = new List<string>();

        for (var i = 0; i < lines.Length; i++)
        {
            var content = StripDocPrefix(lines[i]);
            if (content is null)
            {
                // A non-doc line cannot appear inside a well-formed doc block; reset defensively.
                inExample = false;
                inCode = false;
                continue;
            }

            if (content.Contains("<example", StringComparison.Ordinal))
            {
                scan.ExampleTagCount++;
            }

            // A line may hold one tag or several (`<example><code>`, `</code></example>`).
            foreach (var tag in SplitTags(content.Trim()))
            {
                switch (tag)
                {
                    case "<example>":
                        inExample = true;
                        exampleHasCode = false;
                        break;

                    case "</example>":
                        inExample = false;
                        inCode = false;
                        break;

                    case "<code>" when inExample:
                        inCode = true;
                        buffer.Clear();
                        codeStartLine = i + 2; // first code line, 1-based
                        break;

                    case "</code>" when inCode:
                        inCode = false;
                        scan.Examples.Add(new DocExample(relative, codeStartLine, Decode(string.Join("\n", buffer))));
                        if (!exampleHasCode)
                        {
                            exampleHasCode = true;
                            scan.ExamplesWithCodeCount++;
                        }

                        break;

                    default:
                        if (inCode)
                        {
                            buffer.Add(content);
                        }

                        break;
                }
            }
        }
    }


    // Returns the block tags a line is made of when it holds nothing else, and otherwise the
    // line itself, so that a code line is never mistaken for markup.
    private static IReadOnlyList<string> SplitTags(string line)
    {
        var tags = new List<string>();
        var rest = line;
        while (rest.Length > 0)
        {
            var tag = Array.Find(BlockTags, candidate => rest.StartsWith(candidate, StringComparison.Ordinal));
            if (tag is null)
            {
                return [line];
            }

            tags.Add(tag);
            rest = rest.Substring(tag.Length).TrimStart();
        }

        return tags.Count == 0 ? [line] : tags;
    }


    // Strips a leading `///` (and the single conventional space after it) while preserving
    // the snippet's own indentation. Returns null for lines that are not doc comments.
    private static string? StripDocPrefix(string line)
    {
        var trimmed = line.TrimStart();
        if (!trimmed.StartsWith("///", StringComparison.Ordinal))
        {
            return null;
        }

        var rest = trimmed.Substring(3);
        if (rest.StartsWith(" ", StringComparison.Ordinal))
        {
            rest = rest.Substring(1);
        }

        return rest;
    }


    // XML doc comments escape `<`, `>` and `&` as entities; decode them back to real C#.
    private static string Decode(string code) => WebUtility.HtmlDecode(code);


    private static bool IsGeneratedPath(string file)
    {
        var sep = Path.DirectorySeparatorChar;
        return file.Contains($"{sep}bin{sep}", StringComparison.Ordinal)
            || file.Contains($"{sep}obj{sep}", StringComparison.Ordinal);
    }


    // Walks up from the test assembly's base directory to the checked-out tree. Deliberately
    // avoids [CallerFilePath], which bakes in the build-machine path and resolves to a
    // non-existent '/_/...' location under CI's deterministic-build settings.
    internal static string LocateSourceDirectory(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src", "Wolfgang.Etl.Abstractions");
            if (Directory.Exists(candidate)
                && File.Exists(Path.Combine(candidate, "Wolfgang.Etl.Abstractions.csproj")))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate 'src/Wolfgang.Etl.Abstractions' above '{startDirectory}'.");
    }
}
