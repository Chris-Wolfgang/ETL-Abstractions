using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.DocExamples;

/// <summary>
/// Compiles every <c>&lt;example&gt;&lt;code&gt;</c> block found in the two shipped
/// projects' XML documentation against the current public API, so a snippet that
/// outlives the API it demonstrates (a renamed or removed member) fails the build.
/// </summary>
/// <remarks>
/// TestKit's examples come in three shapes: statement snippets (constructing a
/// <c>TestExtractor</c>), full type declarations (a consumer's contract-test
/// subclass), and bare member snippets (a single <c>override</c>). A single
/// wrapping cannot host all three, so each snippet is compiled in three contexts
/// — top level, as a class member, and as an async-method body — and is accepted
/// if <em>any</em> context compiles cleanly. Errors that merely reflect an
/// undefined illustrative placeholder (<c>MyExtractor</c>, <c>db</c>, <c>source</c>)
/// or the structural artifacts of extracting a member out of its base type are
/// tolerated. What is NOT tolerated is an error against a symbol the library
/// actually owns — the documentation rot this test exists to catch.
/// </remarks>
public sealed class DocExampleCompilationTests
{
    // Errors that indicate the snippet references a symbol which does not exist
    // in the illustrative context (an undefined placeholder), as opposed to a
    // broken reference to the library's own API.
    private static readonly HashSet<string> PlaceholderErrorCodes = new(StringComparer.Ordinal)
    {
        "CS0246", // type or namespace could not be found (e.g. `MyExtractor`, `MyRecord`)
        "CS0103", // name does not exist in the current context (e.g. `db`, `sourceData`)
        "CS0234", // type/namespace does not exist in a placeholder namespace
        "CS0305", // wrong number of type arguments on an undefined generic placeholder
    };

    // Errors that cascade from an undefined placeholder: once a lambda parameter
    // or argument has an error type (because its type is a placeholder), overload
    // resolution and type inference can no longer succeed. Tolerated ONLY when the
    // snippet also produced a hard placeholder error above.
    private static readonly HashSet<string> PlaceholderCascadeCodes = new(StringComparer.Ordinal)
    {
        "CS0121", // ambiguous call (overload can't be picked without the lambda's real return type)
        "CS0411", // type arguments cannot be inferred
        "CS1503", // argument type conversion
        "CS1660", // cannot convert lambda to the expected type
        "CS1661", // lambda parameter count/signature mismatch
        "CS1662", // cannot convert lambda (return type)
        "CS1929", // receiver has no matching (extension) method
        "CS0119", // placeholder name used as a value where a type is expected
    };

    // Structural artifacts of extracting a snippet out of its surrounding context,
    // which are not documentation rot: an `override` member lifted away from its
    // base type has nothing to override (CS0115), and a snippet that shows two
    // alternatives back-to-back reuses a variable name (CS0128). These are
    // tolerated in whichever wrapping is otherwise cleanest.
    private static readonly HashSet<string> StructuralArtifactCodes = new(StringComparer.Ordinal)
    {
        "CS0115", // no suitable method found to override
        "CS0128", // a local variable is already defined (two-alternatives snippet)
        "CS0106", // modifier not valid for this item (e.g. `protected` at an unexpected level)
    };

    private static readonly string[] HarnessUsings =
    {
        "System",
        "System.Collections.Generic",
        "System.Linq",
        "System.Threading",
        "System.Threading.Tasks",
        "Wolfgang.Etl.Abstractions",
        "Wolfgang.Etl.TestKit",
        "Wolfgang.Etl.TestKit.Xunit",
        "Xunit",
    };

    [Fact]
    public async Task Xml_doc_examples_compile_against_the_current_public_api()
    {
        var sourceRoot = FindSourceRoot();
        var failures = new List<string>();
        var examplesChecked = 0;

        foreach (var file in Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories))
        {
            if (IsGeneratedOrBuildOutput(file))
            {
                continue;
            }

            var source = await File.ReadAllTextAsync(file).ConfigureAwait(false);

            foreach (var (line, code) in ExtractExampleCode(source))
            {
                if (ContainsElision(code))
                {
                    continue;
                }

                examplesChecked++;

                var rot = BestEffortRot(code);
                if (rot.Count > 0)
                {
                    failures.Add(FormatFailure(file, line, code, rot));
                }
            }
        }

        Assert.True
        (
            examplesChecked > 0,
            $"No <example><code> blocks were found under '{sourceRoot}'. The extractor is probably broken."
        );

        Assert.True
        (
            failures.Count == 0,
            $"{failures.Count} XML-doc example(s) no longer compile against the current public API:"
            + Environment.NewLine + Environment.NewLine
            + string.Join(Environment.NewLine + Environment.NewLine, failures)
        );
    }

    // Compile the snippet in three contexts and return the smallest set of
    // real-rot diagnostics across them — a snippet is only rot if it fails to
    // compile cleanly in EVERY context it could plausibly belong to.
    private static IReadOnlyList<Diagnostic> BestEffortRot(string snippet)
    {
        var usings = string.Concat(HarnessUsings.Select(u => $"using {u};" + Environment.NewLine));

        var candidates = new[]
        {
            // Top level: hosts type declarations and (as top-level statements) plain
            // statement snippets, including `await` / `await foreach`.
            usings + snippet + Environment.NewLine,

            // Class member: hosts a bare member snippet (e.g. a single `override`).
            usings + "internal class DocExampleHarness" + Environment.NewLine
                   + "{" + Environment.NewLine + snippet + Environment.NewLine + "}" + Environment.NewLine,

            // Async method body: hosts statement snippets that need a method scope.
            usings + "internal class DocExampleHarness" + Environment.NewLine + "{" + Environment.NewLine
                   + "    private async global::System.Threading.Tasks.Task RunAsync()" + Environment.NewLine
                   + "    {" + Environment.NewLine + snippet + Environment.NewLine + "    }" + Environment.NewLine
                   + "}" + Environment.NewLine,
        };

        IReadOnlyList<Diagnostic>? best = null;
        foreach (var program in candidates)
        {
            var rot = RotDiagnostics(program);
            if (rot.Count == 0)
            {
                return rot;
            }

            if (best is null || rot.Count < best.Count)
            {
                best = rot;
            }
        }

        return best ?? Array.Empty<Diagnostic>();
    }

    private static IReadOnlyList<Diagnostic> RotDiagnostics(string program)
    {
        var tree = CSharpSyntaxTree.ParseText
        (
            program,
            new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.None)
        );

        var compilation = CSharpCompilation.Create
        (
            "DocExamples",
            new[] { tree },
            ReferenceAssemblies(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var errors = compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        var hasPlaceholder = errors.Any(d => PlaceholderErrorCodes.Contains(d.Id));

        return errors
            .Where(d => !PlaceholderErrorCodes.Contains(d.Id)
                     && !StructuralArtifactCodes.Contains(d.Id)
                     && !(hasPlaceholder && PlaceholderCascadeCodes.Contains(d.Id)))
            .ToList();
    }

    private static IReadOnlyList<MetadataReference> ReferenceAssemblies()
    {
        // The runtime's trusted-platform-assemblies list includes the framework
        // plus every assembly this test resolves — which, via the two project
        // references, includes both TestKit assemblies and their dependencies.
        var platformAssemblies =
            (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

        return platformAssemblies
            .Where(p => p.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))
            .ToList();
    }

    private static IEnumerable<(int Line, string Code)> ExtractExampleCode(string source)
    {
        var tree = CSharpSyntaxTree.ParseText
        (
            source,
            new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Parse)
        );

        var docComments = tree.GetRoot()
            .DescendantTrivia(descendIntoTrivia: true)
            .Where(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
            .Select(t => t.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>();

        foreach (var doc in docComments)
        {
            var codeElements = doc.DescendantNodes()
                .OfType<XmlElementSyntax>()
                .Where(e => string.Equals(e.StartTag.Name.LocalName.Text, "example", StringComparison.Ordinal))
                .SelectMany(e => e.DescendantNodes().OfType<XmlElementSyntax>())
                .Where(e => string.Equals(e.StartTag.Name.LocalName.Text, "code", StringComparison.Ordinal));

            foreach (var code in codeElements)
            {
                var cleaned = CleanDocText(code.Content.ToFullString());
                if (!string.IsNullOrWhiteSpace(cleaned))
                {
                    var line = code.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    yield return (line, cleaned);
                }
            }
        }
    }

    private static string CleanDocText(string raw)
    {
        var builder = new StringBuilder();

        foreach (var rawLine in raw.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            var line = rawLine.TrimStart();

            if (line.StartsWith("///", StringComparison.Ordinal))
            {
                line = line.Substring(3);
            }

            // Drop exactly one leading space (the `/// ` separator) but keep the
            // snippet's own indentation intact.
            if (line.StartsWith(" ", StringComparison.Ordinal))
            {
                line = line.Substring(1);
            }

            builder.Append(line).Append('\n');
        }

        return WebUtility.HtmlDecode(builder.ToString()).Trim('\n');
    }

    // Doc examples elide bodies and continuations with `...`, which is not valid
    // C#. Those snippets can't be compiled meaningfully, so they are skipped.
    private static bool ContainsElision(string code) =>
        code.Contains("...", StringComparison.Ordinal);

    private static bool IsGeneratedOrBuildOutput(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase);
    }

    private static string FindSourceRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src");
            if (Directory.Exists(Path.Combine(candidate, "Wolfgang.Etl.TestKit")))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException
        (
            "Could not locate the 'src' root by walking up from " + AppContext.BaseDirectory
        );
    }

    private static string FormatFailure(string file, int line, string code, IEnumerable<Diagnostic> diagnostics)
    {
        var indentedCode = string.Join
        (
            Environment.NewLine,
            code.Replace("\r\n", "\n", StringComparison.Ordinal)
                .Split('\n')
                .Select(l => "      " + l)
        );

        var diagnosticText = string.Join
        (
            Environment.NewLine,
            diagnostics.Select(d => "    -> " + d.Id + ": " + d.GetMessage())
        );

        return Path.GetFileName(file) + " (example at line " + line + "):" + Environment.NewLine
            + indentedCode + Environment.NewLine
            + diagnosticText;
    }
}
