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
    /// what this test does. The SHA is substituted on every build, local ones
    /// included, so a URL can always be built; only a local, unpushed commit is
    /// allowed to 404.
    /// </remarks>
    [Theory]
    [MemberData(nameof(ShippedPackages))]
    public async Task Sourcelink_github_raw_url_resolves_for_a_real_source_file(string package)
    {
        var mappings = ReadOurSourceLinkMappings(package);
        Assert.NotEmpty(mappings);

        var probeUrl = BuildProbeUrl(package, mappings);
        Assert.NotNull(probeUrl);
        Assert.DoesNotContain("*", probeUrl, StringComparison.Ordinal);

        // raw.githubusercontent.com does not serve a commit the instant it is pushed.
        // Measured propagation here was under two minutes, so a single 404 does not
        // prove the SHA is unresolvable. Retry briefly before concluding anything.
        // Only CI waits. Locally a 404 cannot fail the test, so retrying just adds
        // ten seconds per package to every run on an unpushed commit.
        var (outcome, status) = await ProbeAsync
        (
            () => GetStatusCodeAsync(probeUrl),
            RunningInCi ? 3 : 1,
            TimeSpan.FromSeconds(5)
        );

        // In CI the commit under test is always pushed, so a 404 is a real defect.
        // Locally it usually just means this commit has not been pushed yet.
        Assert.True
        (
            IsAcceptable(outcome, RunningInCi),
            $"SourceLink URL probe ended {outcome} (HTTP {status}): {probeUrl}"
        );
    }



    [Theory]
    [InlineData(200, ProbeOutcome.Resolved)]
    [InlineData(403, ProbeOutcome.Unavailable)]
    [InlineData(429, ProbeOutcome.Unavailable)]
    [InlineData(503, ProbeOutcome.Unavailable)]
    [InlineData(400, ProbeOutcome.BadStatus)]
    public async Task ProbeAsync_when_first_answer_is_final_returns_its_outcome_without_retrying(int status, ProbeOutcome expected)
    {
        var calls = 0;

        var (outcome, reported) = await ProbeAsync
        (
            () =>
            {
                calls++;
                return Task.FromResult(status);
            },
            3,
            TimeSpan.Zero
        );

        Assert.Equal(expected, outcome);
        Assert.Equal(status, reported);
        Assert.Equal(1, calls);
    }



    [Fact]
    public async Task ProbeAsync_when_404_persists_retries_every_attempt_then_reports_NotFound()
    {
        var calls = 0;

        var (outcome, status) = await ProbeAsync
        (
            () =>
            {
                calls++;
                return Task.FromResult(404);
            },
            3,
            TimeSpan.Zero
        );

        Assert.Equal(ProbeOutcome.NotFound, outcome);
        Assert.Equal(404, status);
        Assert.Equal(3, calls);
    }



    [Fact]
    public async Task ProbeAsync_when_404_clears_on_a_retry_reports_Resolved()
    {
        var answers = new Queue<int>(new[] { 404, 200 });

        var (outcome, status) = await ProbeAsync
        (
            () => Task.FromResult(answers.Dequeue()),
            3,
            TimeSpan.Zero
        );

        Assert.Equal(ProbeOutcome.Resolved, outcome);
        Assert.Equal(200, status);
        Assert.Empty(answers);
    }



    [Fact]
    public async Task ProbeAsync_when_the_request_fails_reports_Unavailable()
    {
        var (outcome, _) = await ProbeAsync
        (
            () => throw new HttpRequestException("network down"),
            3,
            TimeSpan.Zero
        );

        Assert.Equal(ProbeOutcome.Unavailable, outcome);
    }



    [Fact]
    public async Task ProbeAsync_when_the_request_times_out_reports_Unavailable()
    {
        var (outcome, _) = await ProbeAsync
        (
            () => throw new TaskCanceledException("timeout"),
            3,
            TimeSpan.Zero
        );

        Assert.Equal(ProbeOutcome.Unavailable, outcome);
    }



    [Theory]
    [InlineData(ProbeOutcome.Resolved, true, true)]
    [InlineData(ProbeOutcome.Unavailable, true, true)]
    [InlineData(ProbeOutcome.NotFound, false, true)]
    [InlineData(ProbeOutcome.NotFound, true, false)]
    [InlineData(ProbeOutcome.BadStatus, false, false)]
    public void IsAcceptable_fails_only_a_bad_status_or_a_404_in_CI(ProbeOutcome outcome, bool runningInCi, bool expected)
    {
        Assert.Equal(expected, IsAcceptable(outcome, runningInCi));
    }



    // ------------------------------------------------------------------


    /// <summary>How a SourceLink URL probe ended.</summary>
    public enum ProbeOutcome
    {
        /// <summary>GitHub served the file.</summary>
        Resolved,

        /// <summary>
        /// Rate-limited (403/429), a server fault (5xx), or no network. Infra rather than
        /// a SourceLink defect; the deterministic checks above still carry the gate.
        /// </summary>
        Unavailable,

        /// <summary>Still 404 after every attempt: the commit SHA does not resolve.</summary>
        NotFound,

        /// <summary>
        /// Any other status: the URL itself is wrong — malformed, or naming a
        /// repository the runner cannot read. There is nothing to wait for.
        /// </summary>
        BadStatus,
    }



    /// <summary>
    /// Asks <paramref name="getStatus"/> for the URL's HTTP status up to
    /// <paramref name="attempts"/> times, pausing <paramref name="pause"/> between
    /// attempts, and retries only while the answer is 404.
    /// </summary>
    private static async Task<(ProbeOutcome Outcome, int Status)> ProbeAsync(Func<Task<int>> getStatus, int attempts, TimeSpan pause)
    {
        var status = 0;
        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                status = await getStatus();
            }
            catch (HttpRequestException)
            {
                return (ProbeOutcome.Unavailable, status);
            }
            catch (TaskCanceledException)
            {
                return (ProbeOutcome.Unavailable, status);
            }

            var outcome = Classify(status);
            if (outcome != ProbeOutcome.NotFound)
            {
                return (outcome, status);
            }

            if (attempt < attempts)
            {
                await Task.Delay(pause);
            }
        }

        return (ProbeOutcome.NotFound, status);
    }



    private static ProbeOutcome Classify(int status) =>
        status switch
        {
            >= 200 and < 300 => ProbeOutcome.Resolved,
            403 or 429 or >= 500 => ProbeOutcome.Unavailable,
            404 => ProbeOutcome.NotFound,
            _ => ProbeOutcome.BadStatus,
        };



    private static bool IsAcceptable(ProbeOutcome outcome, bool runningInCi) =>
        outcome is ProbeOutcome.Resolved or ProbeOutcome.Unavailable
        || (outcome is ProbeOutcome.NotFound && !runningInCi);



    private static async Task<int> GetStatusCodeAsync(string url)
    {
        using var response = await Http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        return (int)response.StatusCode;
    }



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
    /// URL that names an actual file. Returns <c>null</c> when nothing matches.
    /// </summary>
    private static string? BuildProbeUrl(string package, List<(string LocalPrefix, string UrlPrefix)> mappings)
    {
        var pdbPath = LocateRuntimePdb(package);
        using var stream = File.OpenRead(pdbPath);
        using var provider = MetadataReaderProvider.FromPortablePdbStream(stream);
        var reader = provider.GetMetadataReader();

        return reader.Documents
            .Select(handle => reader.GetString(reader.GetDocument(handle).Name))
            .Where(name => name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            .SelectMany(name => mappings
                .Where(m => m.LocalPrefix.Length > 0 && name.StartsWith(m.LocalPrefix, StringComparison.OrdinalIgnoreCase))
                .Select(m => m.UrlPrefix + name.Substring(m.LocalPrefix.Length).Replace('\\', '/')))
            .FirstOrDefault();
    }



    private static string ReadSourceLinkPayload(MetadataReader reader) =>
        reader.CustomDebugInformation
            .Select(reader.GetCustomDebugInformation)
            .Where(cdi => reader.GetGuid(cdi.Kind) == SourceLinkGuid)
            .Select(cdi => Encoding.UTF8.GetString(reader.GetBlobBytes(cdi.Value)))
            .FirstOrDefault() ?? string.Empty;
}
