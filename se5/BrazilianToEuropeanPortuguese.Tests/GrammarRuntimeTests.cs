using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;
using Xunit.Abstractions;

namespace BrazilianToEuropeanPortuguese.Tests;

[Collection("XdgDataHome")]
public sealed class GrammarRuntimeTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData("linux", Architecture.X64)]
    [InlineData("linux", Architecture.Arm64)]
    [InlineData("mac", Architecture.X64)]
    [InlineData("mac", Architecture.Arm64)]
    [InlineData("windows", Architecture.X64)]
    [InlineData("windows", Architecture.Arm64)]
    public void PortableJavaPackagesArePinnedForSupportedTargets(string os, Architecture architecture)
    {
        var package = GrammarAssets.JavaPackage(os, architecture);
        Assert.Equal(64, package.Hash.Length);
        Assert.Matches("^[a-f0-9]{64}$", package.Hash);
        Assert.InRange(package.Size, 30_000_000, 60_000_000);
        Assert.Contains("21.0.13", package.File);
        Assert.StartsWith("https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.13%2B9/", package.Url);
        if (os == "windows") Assert.Contains("x64_windows", package.File);
    }

    [Fact]
    public void UnsupportedJavaPlatformAndArchitectureFailExplicitly()
    {
        Assert.Throws<PlatformNotSupportedException>(() => GrammarAssets.JavaPackage("linux", Architecture.X86));
        Assert.Throws<PlatformNotSupportedException>(() => GrammarAssets.JavaPackage("other", Architecture.X64));
    }

    [Fact]
    public void GrammarPackageIsFixedAndDefaultsAreManagedButOff()
    {
        Assert.Equal("https://languagetool.org/download/LanguageTool-6.6.zip", GrammarAssets.Tool.Url);
        Assert.Equal("53600506b399bb5ffe1e4c8dec794fd378212f14aaf38ccef9b6f89314d11631", GrammarAssets.Tool.Hash);
        Assert.Equal(251998221, GrammarAssets.Tool.Size);
        Assert.False(ConversionOptions.Default.GrammarCheck);
        Assert.False(ConversionOptions.Default.GrammarExternalServer);
        var external = new ConversionOptions(GrammarCheck: true, GrammarExternalServer: true);
        Assert.Equal(external, ConversionOptions.Load(new PluginRequest
        {
            SettingsVersion = ConversionOptions.Version, Settings = external.ToJson(),
        }));
    }

    [Fact]
    public async Task MissingManagedAssetsExplainTheTwoDownloadButtons()
    {
        var directory = Path.Combine(Path.GetTempPath(), "ptpt-missing-grammar-" + Guid.NewGuid().ToString("N"));
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            GrammarRuntime.StartAsync(new Progress<string>(), CancellationToken.None, directory));
        Assert.Contains("Download LanguageTool pt-PT", exception.Message);
        Assert.Contains("Download portable Java", exception.Message);
        Assert.False(Directory.Exists(directory));
    }

    [Fact]
    public void SafeJavaLicenseAliasesAreCopiedAndEscapesAreRejected()
    {
        var directory = Path.Combine(Path.GetTempPath(), "ptpt-grammar-extract-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var archive = Path.Combine(directory, "java.tar.gz");
        try
        {
            void Archive(string link)
            {
                using var file = File.Create(archive);
                using var gzip = new GZipStream(file, CompressionMode.Compress);
                using var writer = new TarWriter(gzip);
                using var content = new MemoryStream(Encoding.UTF8.GetBytes("license text"));
                writer.WriteEntry(new PaxTarEntry(TarEntryType.RegularFile, "java/legal/java.base/LICENSE") { DataStream = content });
                writer.WriteEntry(new PaxTarEntry(TarEntryType.SymbolicLink, "java/legal/java.naming/LICENSE") { LinkName = link });
            }
            Archive("../java.base/LICENSE");
            var safe = Path.Combine(directory, "safe");
            EmbeddingAssets.ExtractRuntime(archive, safe, CancellationToken.None, allowInternalLinks: true);
            Assert.Equal("license text", File.ReadAllText(Path.Combine(safe, "java/legal/java.naming/LICENSE")));
            Assert.Null(new FileInfo(Path.Combine(safe, "java/legal/java.naming/LICENSE")).LinkTarget);
            Archive("../../../../outside");
            Assert.Throws<InvalidDataException>(() =>
                EmbeddingAssets.ExtractRuntime(archive, Path.Combine(directory, "unsafe"), CancellationToken.None, allowInternalLinks: true));
        }
        finally { Directory.Delete(directory, true); }
    }

    [OptInFact("PTPT_TEST_GRAMMAR", "downloads and runs the actual managed LanguageTool and portable Java")]
    public async Task RealManagedPtPtCheckerStartsChecksAndStopsWithoutExternalServer()
    {
        var directory = Environment.GetEnvironmentVariable("PTPT_GRAMMAR_TEST_ROOT") ??
            Path.Combine(Path.GetTempPath(), "ptpt-real-grammar-" + Guid.NewGuid().ToString("N"));
        var keep = Environment.GetEnvironmentVariable("PTPT_GRAMMAR_TEST_ROOT") != null;
        using var limit = new CancellationTokenSource(TimeSpan.FromMinutes(8));
        var progress = new Progress<string>(output.WriteLine);
        try
        {
            await GrammarAssets.DownloadToolAsync(progress, limit.Token, directory);
            await GrammarAssets.DownloadJavaAsync(progress, limit.Token, directory);
            Assert.True(GrammarAssets.IsInstalled(directory));
            int pid;
            var clock = Stopwatch.StartNew();
            await using (var runtime = await GrammarRuntime.StartAsync(progress, limit.Token, directory))
            {
                pid = runtime.ProcessId;
                output.WriteLine($"Ready in {clock.Elapsed.TotalSeconds:0.00}s; PID {pid}; {runtime.Server}");
                using var client = new LanguageToolClient(runtime.Server);
                var examples = new[]
                {
                    "Eles tem razão.", "Eles têm razão.", "Elas vem aqui.", "Elas vêm aqui.",
                    "Ela falarei nisto.", "Não para polícias.", "As operações policiais começaram.",
                };
                clock.Restart();
                foreach (var example in examples)
                {
                    var matches = await client.CheckAsync(example, limit.Token);
                    output.WriteLine(example + " => " + string.Join("; ", matches.Select(m =>
                        $"{m.Rule}: {string.Join("/", m.Replacements.Take(3))}")));
                }
                output.WriteLine($"Checked {examples.Length} examples in {clock.Elapsed.TotalSeconds:0.00}s.");
                var wrong = await client.CheckAsync(examples[0], limit.Token);
                Assert.Contains(wrong, m => m.Replacements.Contains("Eles têm", StringComparer.Ordinal));
                var correct = await client.CheckAsync(examples[1], limit.Token);
                Assert.DoesNotContain(correct, m => m.Replacements.Contains("tem", StringComparer.Ordinal));
                var lines = new[] { new CheckLine(0, examples[0], examples[0], null, "They are right.") };
                var texts = new Dictionary<int, string> { [0] = examples[0] };
                var notes = new List<string>();
                var summary = await GrammarChecks.RunAsync(lines, new ConversionOptions(GrammarCheck: true, GrammarAutoFix: true),
                    null, texts, (_, note) => notes.Add(note), client.CheckAsync,
                    (_, _) => Task.FromResult(new[] { 1f, 0f }), progress, limit.Token);
                Assert.Equal(examples[1], texts[0]);
                Assert.Contains(notes, n => n.Contains("corrected"));
                output.WriteLine(summary);
            }
            try
            {
                using var process = Process.GetProcessById(pid);
                Assert.True(process.HasExited);
            }
            catch (ArgumentException) { /* Process has been reaped. */ }
            Assert.Empty(Directory.GetFiles(directory, "server-*.properties"));

            var cancelled = new CancellationTokenSource();
            cancelled.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                GrammarRuntime.StartAsync(progress, cancelled.Token, directory));
            cancelled.Dispose();
            using var interrupted = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                GrammarRuntime.StartAsync(progress, interrupted.Token, directory));
            Assert.Empty(Directory.GetFiles(directory, "server-*.properties"));
        }
        finally { if (!keep && Directory.Exists(directory)) Directory.Delete(directory, true); }
    }
}
