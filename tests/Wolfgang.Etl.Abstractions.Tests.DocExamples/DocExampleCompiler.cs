using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Wolfgang.Etl.Abstractions.Tests.DocExamples;

/// <summary>
/// Compiles an extracted doc <see cref="DocExample"/> against the real
/// <c>Wolfgang.Etl.Abstractions</c> assembly. The snippet is wrapped in a synthetic
/// harness that supplies the imports and the placeholder identifiers the illustrative
/// snippets reference (<c>records</c>, <c>sqlLoader</c>, <c>filePath</c>, …) while the
/// actual API calls (<c>From</c>/<c>Through</c>/<c>To</c>/<c>Extract</c>/<c>Transform</c>/
/// <c>Load</c>/<c>RunAsync</c>/…) bind against the shipped types — so a renamed or removed
/// member turns a stale example into a compile error.
/// </summary>
public static class DocExampleCompiler
{
    /// <summary>
    /// Wraps and compiles <paramref name="example"/>, returning only the
    /// error-severity diagnostics (an empty list means the snippet is valid).
    /// </summary>
    public static IReadOnlyList<Diagnostic> Compile(DocExample example)
    {
        ArgumentNullException.ThrowIfNull(example);
        var source = BuildSource(example);

        var tree = CSharpSyntaxTree.ParseText(
            source,
            new CSharpParseOptions(LanguageVersion.Latest));

        var compilation = CSharpCompilation.Create(
            assemblyName: "DocExampleScratch",
            syntaxTrees: [tree],
            references: ReferenceAssemblies(),
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Disable));

        return compilation
            .GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToArray();
    }


    private static string BuildSource(DocExample example)
    {
        var (signature, closer) = WrapperSignature(example.Code);

        // #line remaps compiler diagnostics onto the real doc-comment location.
        var location = example.File; // repository-relative, already forward-slashed

        return $$"""
            using System;
            using System.IO;
            using System.Linq;
            using System.Threading;
            using System.Threading.Tasks;
            using System.Collections.Generic;
            using Wolfgang.Etl.Abstractions;

            namespace DocExamples.Generated
            {
                // A sample record type the fluent examples project over.
                internal sealed class Order { public decimal Amount { get; set; } }

                // The only stub the examples instantiate directly; the real operator lives in
                // Wolfgang.Etl.Transformers, which Abstractions cannot reference (see ADR-0002).
                internal sealed class WhereTransformer<T> : ITransformAsync<T, T>
                    where T : notnull
                {
                    public WhereTransformer(Func<T, bool> predicate) { _ = predicate; }

                    public IAsyncEnumerable<T> TransformAsync(IAsyncEnumerable<T> items)
                        => throw new NotSupportedException();
                }

                // Supplies the placeholder identifiers the illustrative snippets reference. These
                // are scaffolding, not the API under test: they are never executed (the snippets are
                // compiled, not run), so their values are irrelevant. Their TYPES are chosen so the
                // real API calls in the snippets resolve exactly as a consumer's would.
                internal abstract class DocExampleContext
                {
                    protected int MaximumItemCount;
                    protected int SkipItemCount;
                    protected string filePath;
                    protected IEnumerable<Order> items;
                    protected IAsyncEnumerable<Order> records;
                    protected LoaderBase<Order, Report> sqlLoader;
                    protected IProgress<EtlPipelineProgress> progress;
                    protected CancellationToken token;
                    protected IExtractWithProgressAndCancellationAsync<Order, Report> csvExtractor;
                    protected IProgress<Report> extractProgress;
                    protected ITransformAsync<Order, Order> enrich;
                    protected IProgress<Report> loadProgress;
                }

                internal sealed class Example : DocExampleContext
                {
                    public {{signature}}
                    {
            #line {{example.Line}} "{{location}}"
            {{example.Code}}
            #line default
                    }{{closer}}
                }
            }
            """;
    }


    // Chooses the wrapper method shape that lets the snippet compile:
    //   - a `yield` snippet must sit in an async-iterator method;
    //   - an `await` snippet needs `async Task`;
    //   - anything else (e.g. a plain `foreach`) is a synchronous `void` body,
    //     which avoids a spurious CS1998 "async method lacks await" on those.
    private static (string Signature, string Closer) WrapperSignature(string code)
    {
        if (ContainsWord(code, "yield"))
        {
            return ("async IAsyncEnumerable<string> Run()", string.Empty);
        }

        if (ContainsWord(code, "await"))
        {
            return ("async Task Run()", string.Empty);
        }

        return ("void Run()", string.Empty);
    }


    private static bool ContainsWord(string code, string word)
    {
        var index = code.IndexOf(word, StringComparison.Ordinal);
        while (index >= 0)
        {
            var before = index == 0 || !char.IsLetterOrDigit(code[index - 1]);
            var afterIndex = index + word.Length;
            var after = afterIndex >= code.Length || !char.IsLetterOrDigit(code[afterIndex]);
            if (before && after)
            {
                return true;
            }

            index = code.IndexOf(word, index + 1, StringComparison.Ordinal);
        }

        return false;
    }


    // The compiler needs the full framework reference set plus the library under test. The
    // trusted-platform-assemblies list is the reference closure of the running test host
    // (net10.0), which already includes the project-referenced Abstractions assembly.
    private static IReadOnlyList<MetadataReference> ReferenceAssemblies()
    {
        var references = new List<MetadataReference>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var trusted = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? string.Empty;
        foreach (var path in trusted.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            if (path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) && seen.Add(path))
            {
                references.Add(MetadataReference.CreateFromFile(path));
            }
        }

        // Belt-and-braces: guarantee the library under test is referenced even if it is
        // ever loaded from outside the TPA closure.
        var abstractionsPath = typeof(EtlPipeline).Assembly.Location;
        if (!string.IsNullOrEmpty(abstractionsPath) && seen.Add(abstractionsPath))
        {
            references.Add(MetadataReference.CreateFromFile(abstractionsPath));
        }

        return references;
    }
}
