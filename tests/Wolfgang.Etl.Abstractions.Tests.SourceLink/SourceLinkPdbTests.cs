// SourceLink PDB gates — the mechanical preconditions for F11-into-source.
//
// Interactive-debugger step-into is not automatable in an ordinary CI job, but
// the things that make it work ARE: the assembly's PDB must be portable (not
// full-format), it must carry a SourceLink CustomDebugInformation record, that
// record must map this repo's source paths to GitHub raw URLs, and those URLs
// must actually resolve. If all of those hold, F11-into-source works by
// construction; if any breaks, a consumer's debugger silently falls back to
// decompiled placeholders.
//
// This repo ships four packages, so every check runs per-package rather than
// against a single PDB: a symbol defect in any one of them is a broken
// debugging experience for that package's consumers.
//
// Refs #214.

using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Wolfgang.Etl.Abstractions.Tests.SourceLink;

public class SourceLinkPdbTests
{
    private const string RepoSlug = "Chris-Wolfgang/ETL-Abstractions";


    private const string RawHost = "raw.githubusercontent.com";


    private static readonly Guid SourceLinkGuid = new("CC110556-A091-4D38-9FEC-25AB9A351A6A");


    /// <summary>
    /// Every package this repo ships. Each is referenced by the test project, so
    /// each one's PDB is copied into the output directory.
    /// </summary>
    public static TheoryData<string> ShippedPackages() => new()
    {
        "Wolfgang.Etl.Abstractions",
        "Wolfgang.Etl.ErrorPolicies",
        "Wolfgang.Etl.TestKit",
        "Wolfgang.Etl.TestKit.Xunit"
    };



    /// <summary>
    /// Portable PDBs start with the four bytes 'B','S','J','B' — the ECMA-335
    /// metadata-blob magic. Full-format Windows PDBs start with
    /// "Microsoft C/C++ MSF 7.00". SourceLink is portable-PDB only, so
    /// full-format is an immediate fail.
    /// </summary>
    [Theory]
    [MemberData(nameof(ShippedPackages))]
    public void Runtime_pdb_is_portable_format(string package)
    {
        var pdbPath = LocateRuntimePdb(package);
        Assert.True(File.Exists(pdbPath), $"Runtime PDB not found at {pdbPath}");

        Span<byte> magic = stackalloc byte[4];
        using (var fs = File.OpenRead(pdbPath))
        {
            Assert.Equal(4, fs.Read(magic));
        }

        Assert.Equal((byte)'B', magic[0]);
        Assert.Equal((byte)'S', magic[1]);
        Assert.Equal((byte)'J', magic[2]);
        Assert.Equal((byte)'B', magic[3]);
    }



    /// <summary>
    /// The runtime PDB must carry a SourceLink record whose JSON maps this
    /// repo's source paths to GitHub raw URLs. Third-party packages contribute
    /// their own mappings, so only entries pointing at this repo are asserted.
    /// </summary>
    [Theory]
    [MemberData(nameof(ShippedPackages))]
    public void Runtime_pdb_has_sourcelink_pointing_at_github_raw(string package)
    {
        var mappings = ReadOurSourceLinkMappings(package);

        Assert.NotEmpty(mappings);

        foreach (var (_, url) in mappings)
        {
            AssertIsOurRawGitHubUrl(url);
        }
    }



    /// <summary>
    /// Resolves a real source URL out of the SourceLink mapping and checks that
    /// GitHub serves it. This is what catches a force-pushed or deleted commit
    /// that would leave a consumer's debugger with a dead raw URL — the
    /// structural checks above cannot see that.
    /// </summary>
    /// <remarks>
    /// A SourceLink mapping is a prefix pair, e.g.
    /// <c>"/_/*" -&gt; "https://raw.githubusercontent.com/{slug}/{sha}/*"</c>.
    /// Probing the mapping value verbatim is useless: it still contains the
    /// literal <c>*</c> and would 404 for that reason alone. A real URL only
    /// exists once an actual document path is substituted into it, which is
    /// what this test does. Skipped when the SHA has not been substituted
    /// (local dev builds), since only a pushed commit resolves.
    /// </remarks>
    [Theory]
    [MemberData(nameof(ShippedPackages))]
    public async Task Sourcelink_github_raw_url_resolves_for_a_real_source_file(string package)
    {
        var mappings = ReadOurSourceLinkMappings(package);
        if (mappings.Count == 0)
        {
            // The structural test above already failed with a precise
            // diagnostic; nothing further to add here.
            return;
        }

        var probeUrl = BuildProbeUrl(package, mappings);
        if (probeUrl is null)
        {
            // Either no document matched a mapping prefix, or the URL still holds the
            // unresolved "*" SHA placeholder. On a developer machine that is the ordinary
            // case — a local, unpushed build — so the probe is skipped.
            //
            // In CI it is not. The same path is taken by a malformed local-prefix mapping,
            // a document-table mismatch and an unresolved SHA, so returning here would let
            // a broken PDB satisfy the two structural checks and silently skip the
            // resolution check this test exists to perform. CI builds from a pushed commit,
            // so there is no legitimate reason for the URL to be unbuildable.
            if (RunningInCi)
            {
                Assert.Fail
                (
                    "No probe URL could be built from the SourceLink document table. In CI "
                    + "this means the mapping prefix, the document paths or the commit SHA "
                    + "did not line up - not that the build is local and unpushed."
                );
            }

            return;
        }

        Assert.DoesNotContain("*", probeUrl, StringComparison.Ordinal);

        try
        {
            using var response = await Http.GetAsync(probeUrl, HttpCompletionOption.ResponseHeadersRead);

            // 404 means the SHA no longer resolves (force-push, repo rename).
            // 403/429 is GitHub rate-limiting the runner, which is infra noise
            // rather than a SourceLink defect.
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Assert.Fail($"SourceLink URL 404s — the commit SHA no longer resolves: {probeUrl}");
            }
        }
        catch (HttpRequestException)
        {
            // Network unavailable / GitHub outage: the deterministic checks
            // above carry the per-PR gate, so don't fail on infra.
        }
        catch (TaskCanceledException)
        {
            // Timeout — same rationale.
        }
    }



    // ------------------------------------------------------------------


    /// <summary>
    /// Asserts that <paramref name="url"/> is an absolute HTTPS URL served by
    /// GitHub's raw host whose path names this repository.
    /// </summary>
    /// <remarks>
    /// The host is compared for equality rather than with a substring test. A
    /// substring test would accept a look-alike host such as
    /// <c>raw.githubusercontent.com.example</c>, or an unrelated host carrying
    /// that text somewhere in its path, and so would not actually prove the
    /// mapping points where a debugger needs it to.
    /// </remarks>
    private static void AssertIsOurRawGitHubUrl(string url)
    {
        Assert.True
        (
            Uri.TryCreate(url, UriKind.Absolute, out var uri),
            $"SourceLink mapping is not an absolute URI: {url}"
        );

        Assert.Equal(Uri.UriSchemeHttps, uri.Scheme);
        Assert.Equal(RawHost, uri.Host, ignoreCase: true);

        Assert.True
        (
            uri.AbsolutePath.StartsWith($"/{RepoSlug}/", StringComparison.OrdinalIgnoreCase),
            $"SourceLink mapping path does not name {RepoSlug}: {uri.AbsolutePath}"
        );
    }



    private static string LocateRuntimePdb(string package)
    {
        // ProjectReference copies each referenced assembly's PDB into this test
        // project's output directory.
        return Path.Combine(AppContext.BaseDirectory, package + ".pdb");
    }



    /// <summary>
    /// Returns the SourceLink prefix mappings that point at this repository,
    /// as (localPathPrefix, urlPrefix) pairs with the trailing '*' removed.
    /// </summary>
    private static List<(string LocalPrefix, string UrlPrefix)> ReadOurSourceLinkMappings(string package)
    {
        var pdbPath = LocateRuntimePdb(package);
        Assert.True(File.Exists(pdbPath), $"Runtime PDB not found at {pdbPath}");

        using var stream = File.OpenRead(pdbPath);
        using var provider = MetadataReaderProvider.FromPortablePdbStream(stream);
        var reader = provider.GetMetadataReader();

        var payload = ReadSourceLinkPayload(reader);
        Assert.False(string.IsNullOrEmpty(payload), $"{package}: PDB has no SourceLink CustomDebugInformation record.");

        using var doc = JsonDocument.Parse(payload);
        Assert.True
        (
            doc.RootElement.TryGetProperty("documents", out var documents),
            $"SourceLink payload has no 'documents' property: {payload}"
        );

        var result = new List<(string, string)>();
        foreach (var entry in documents.EnumerateObject())
        {
            var url = entry.Value.GetString();

            // Deliberately a loose, slug-only filter. Its job is to separate our
            // mappings from the ones third-party packages contribute, nothing
            // more. Applying the strict host check here instead would mean a
            // mapping with the right repo but a WRONG host got silently filtered
            // out, and the only symptom would be an empty-collection failure.
            // Selecting it loosely and asserting strictly reports the actual
            // defect instead. See AssertIsOurRawGitHubUrl.
            if (url is null || !url.Contains(RepoSlug, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            result.Add((entry.Name.TrimEnd('*'), url.TrimEnd('*')));
        }

        return result;
    }



    /// <summary>
    /// One shared client for the whole suite. A per-call <see cref="HttpClient"/> is
    /// disposed while its socket lingers in TIME_WAIT, so repeated creation exhausts
    /// sockets; the analyser flags it for that reason. A static instance also removes the
    /// object-initialiser-inside-using shape, where a throw during initialisation would
    /// leak the half-built client.
    /// </summary>
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(15) };



    /// <summary>
    /// Whether the suite is running in CI, where an unbuildable probe URL is a defect
    /// rather than the ordinary local-build case. GitHub Actions sets <c>CI</c>, as does
    /// every other common CI.
    /// </summary>
    private static bool RunningInCi =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));



    /// <summary>
    /// Picks a source document from the PDB, matches it against a SourceLink
    /// prefix mapping and substitutes the remainder into the URL, yielding a
    /// URL that names an actual file. Returns <c>null</c> when nothing matches
    /// or the SHA is still the unresolved '*' placeholder.
    /// </summary>
    private static string? BuildProbeUrl(string package, List<(string LocalPrefix, string UrlPrefix)> mappings)
    {
        var pdbPath = LocateRuntimePdb(package);
        using var stream = File.OpenRead(pdbPath);
        using var provider = MetadataReaderProvider.FromPortablePdbStream(stream);
        var reader = provider.GetMetadataReader();

        foreach (var handle in reader.Documents)
        {
            var name = reader.GetString(reader.GetDocument(handle).Name);
            if (string.IsNullOrEmpty(name) || !name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (var (localPrefix, urlPrefix) in mappings)
            {
                if (localPrefix.Length == 0 || !name.StartsWith(localPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // The URL still carrying '*' means Microsoft.SourceLink.GitHub
                // never substituted a commit SHA — an unpushed local build.
                if (urlPrefix.Contains('*', StringComparison.Ordinal))
                {
                    return null;
                }

                var relative = name.Substring(localPrefix.Length).Replace('\\', '/');
                return urlPrefix + relative;
            }
        }

        return null;
    }



    private static string ReadSourceLinkPayload(MetadataReader reader)
    {
        foreach (var handle in reader.CustomDebugInformation)
        {
            var cdi = reader.GetCustomDebugInformation(handle);
            if (reader.GetGuid(cdi.Kind) != SourceLinkGuid)
            {
                continue;
            }

            return Encoding.UTF8.GetString(reader.GetBlobBytes(cdi.Value));
        }

        return string.Empty;
    }
}
