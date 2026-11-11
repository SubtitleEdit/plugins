using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal sealed record GrammarPackage(string File, string Url, string Hash, long Size);

internal static class GrammarAssets
{
    internal const string ToolVersion = "6.6";
    internal const string JavaVersion = "21.0.13+9";
    internal static readonly GrammarPackage Tool = new("LanguageTool-6.6.zip",
        "https://languagetool.org/download/LanguageTool-6.6.zip",
        "53600506b399bb5ffe1e4c8dec794fd378212f14aaf38ccef9b6f89314d11631", 251998221);
    internal static string DirectoryPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SubtitleEdit", "BrazilianToEuropeanPortuguese", "GrammarChecker");

    internal static GrammarPackage JavaPackage(string os, Architecture architecture)
    {
        // Windows on ARM runs the current x64 Temurin JRE through OS emulation.
        var arch = architecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 when os == "windows" => "x64",
            Architecture.Arm64 => "aarch64",
            _ => throw new PlatformNotSupportedException("Portable Java downloads support x64 and ARM64."),
        };
        var (hash, size) = (os, arch) switch
        {
            ("linux", "x64") => ("a535a1df9c6284d9618872984e026ae0352d1f7e9e0605425c0e6114f47351dd", 52181297L),
            ("linux", "aarch64") => ("b1734db7ade49c3f6acc6ae06233b8c2dd90e3e2f00f67000ae1c972aaa64d8c", 51266660L),
            ("mac", "x64") => ("c909674b1c10c261b9a3fb7e9c0efebd303e33f88a4fe84be7135b5e2d711f89", 41902098L),
            ("mac", "aarch64") => ("225bab0a09491ef06835488cd942f5d2befa9a0410a2484f0c8bbcf7506ec09a", 47935740L),
            ("windows", "x64") => ("2332c0b4cff6bed722da3567c91d5e47568bcba863fa4945d0a8d6fc76158628", 49106397L),
            _ => throw new PlatformNotSupportedException("Portable Java downloads support Windows, Linux and macOS."),
        };
        var name = $"OpenJDK21U-jre_{arch}_{os}_hotspot_21.0.13_9." + (os == "windows" ? "zip" : "tar.gz");
        return new GrammarPackage(name,
            "https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.13%2B9/" + name, hash, size);
    }

    internal static GrammarPackage JavaPackage() => JavaPackage(
        OperatingSystem.IsWindows() ? "windows" : OperatingSystem.IsLinux() ? "linux" :
        OperatingSystem.IsMacOS() ? "mac" : throw new PlatformNotSupportedException("Unsupported Java platform."),
        RuntimeInformation.ProcessArchitecture);

    internal static string ToolJar(string? root = null) =>
        Path.Combine(root ?? DirectoryPath, "tool-" + ToolVersion, "LanguageTool-" + ToolVersion, "languagetool-server.jar");

    internal static string JavaExecutable(string? root = null)
    {
        var home = Path.Combine(root ?? DirectoryPath, "java-" + JavaVersion, "jdk-" + JavaVersion + "-jre");
        if (OperatingSystem.IsMacOS()) home = Path.Combine(home, "Contents", "Home");
        return Path.Combine(home, "bin", OperatingSystem.IsWindows() ? "java.exe" : "java");
    }

    internal static bool IsInstalled(string? root = null) => File.Exists(ToolJar(root)) && File.Exists(JavaExecutable(root));

    internal static Task DownloadToolAsync(IProgress<string> progress, CancellationToken token, string? root = null) =>
        InstallAsync(Tool, "tool-" + ToolVersion, ToolJar(root), progress, token, root);

    internal static Task DownloadJavaAsync(IProgress<string> progress, CancellationToken token, string? root = null) =>
        InstallAsync(JavaPackage(), "java-" + JavaVersion, JavaExecutable(root), progress, token, root);

    private static async Task InstallAsync(GrammarPackage package, string folder, string expected,
        IProgress<string> progress, CancellationToken token, string? directory)
    {
        var root = directory ?? DirectoryPath;
        Directory.CreateDirectory(root);
        FileStream installLock;
        try
        {
            installLock = new FileStream(Path.Combine(root, "install.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }
        catch (IOException ex)
        {
            throw new IOException("Grammar checker installation is already running in another plugin instance.", ex);
        }
        await using (installLock)
        {
            if (File.Exists(expected))
            {
                progress.Report(package.File + " is already installed.");
                return;
            }
            var archive = Path.Combine(root, package.File);
            if (!File.Exists(archive) || !await EmbeddingAssets.HasHashAsync(archive, package.Hash, token))
                await EmbeddingAssets.DownloadAsync(new Uri(package.Url), archive, package.Hash, package.Size, progress, token);
            var target = Path.Combine(root, folder);
            if (Directory.Exists(target))
                throw new IOException("Incomplete grammar checker installation at " + target + ". Remove that version's folder and download again.");
            var staging = Path.Combine(root, "install-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(staging);
            try
            {
                progress.Report("Extracting " + package.File + "...");
                await Task.Run(() => EmbeddingAssets.ExtractRuntime(archive, staging, token, allowInternalLinks: true), token);
                var executable = Path.Combine(staging, Path.GetRelativePath(target, expected));
                if (!File.Exists(executable)) throw new InvalidDataException("Grammar package is missing its expected executable or server JAR.");
                if (!OperatingSystem.IsWindows() && expected == JavaExecutable(root))
                    File.SetUnixFileMode(executable, File.GetUnixFileMode(executable) | UnixFileMode.UserExecute);
                token.ThrowIfCancellationRequested();
                Directory.Move(staging, target);
                File.Delete(archive);
            }
            finally
            {
                if (Directory.Exists(staging)) Directory.Delete(staging, true);
            }
        }
    }
}

/// <summary>Owned loopback-only LanguageTool server, started for a check and stopped afterwards.</summary>
internal sealed class GrammarRuntime : IAsyncDisposable
{
    private static readonly ConcurrentDictionary<Process, byte> Running = new();
    private readonly Process _process;
    private readonly StringBuilder _log = new();
    private readonly Task _stdout;
    private readonly Task _stderr;
    private readonly string _configuration;
    internal string Server { get; }
    internal int ProcessId => _process.Id;

    static GrammarRuntime() => AppDomain.CurrentDomain.ProcessExit += (_, _) => KillAll();

    private GrammarRuntime(Process process, string server, string configuration)
    {
        _process = process;
        Server = server;
        _configuration = configuration;
        Running.TryAdd(process, 0);
        _stdout = ReadLogAsync(process.StandardOutput);
        _stderr = ReadLogAsync(process.StandardError);
    }

    private async Task ReadLogAsync(StreamReader reader)
    {
        while (await reader.ReadLineAsync() is { } line)
            lock (_log)
            {
                if (_log.Length > 16000) _log.Remove(0, _log.Length - 8000);
                _log.AppendLine(line);
            }
    }

    private string Tail() { lock (_log) return _log.ToString(); }

    internal static async Task<GrammarRuntime> StartAsync(IProgress<string> progress, CancellationToken token, string? root = null)
    {
        token.ThrowIfCancellationRequested();
        if (!GrammarAssets.IsInstalled(root))
            throw new InvalidOperationException("Local grammar checker is not ready. Click Download LanguageTool pt-PT and Download portable Java first.");
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        var configuration = Path.Combine(root ?? GrammarAssets.DirectoryPath, "server-" + Guid.NewGuid().ToString("N") + ".properties");
        var start = new ProcessStartInfo(GrammarAssets.JavaExecutable(root))
        {
            WorkingDirectory = Path.GetDirectoryName(GrammarAssets.ToolJar(root))!,
            UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true,
        };
        foreach (var argument in new[]
        {
            "-Xms64m", "-Xmx768m", "-cp", GrammarAssets.ToolJar(root),
            "org.languagetool.server.HTTPServer", "--config", configuration, "--port", port.ToString(System.Globalization.CultureInfo.InvariantCulture),
        }) start.ArgumentList.Add(argument);
        // Do not inherit machine-wide options capable of changing the downloaded runtime's command.
        foreach (var variable in new[] { "JAVA_TOOL_OPTIONS", "_JAVA_OPTIONS", "JDK_JAVA_OPTIONS", "CLASSPATH" })
            start.Environment.Remove(variable);
        Process process;
        try
        {
            await File.WriteAllTextAsync(configuration,
                "maxTextLength=20000\nmaxCheckTimeMillis=10000\nmaxCheckThreads=2\ncacheSize=500\npipelineCaching=true\nmaxPipelinePoolSize=2\n", token);
            token.ThrowIfCancellationRequested();
            process = Process.Start(start) ?? throw new IOException("Could not start the downloaded portable Java.");
        }
        catch
        {
            File.Delete(configuration);
            throw;
        }
        var runtime = new GrammarRuntime(process, $"http://127.0.0.1:{port}/", configuration);
        try
        {
            progress.Report("Starting local LanguageTool pt-PT on CPU...");
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
            timeout.CancelAfter(TimeSpan.FromSeconds(90));
            using var http = new HttpClient(new HttpClientHandler { UseProxy = false, AllowAutoRedirect = false })
            { Timeout = TimeSpan.FromSeconds(2) };
            while (true)
            {
                timeout.Token.ThrowIfCancellationRequested();
                if (process.HasExited)
                {
                    await Task.WhenAll(runtime._stdout, runtime._stderr);
                    throw new IOException("Local LanguageTool exited during startup:\n" + runtime.Tail());
                }
                try
                {
                    using var response = await http.GetAsync(runtime.Server + "v2/languages", timeout.Token);
                    response.EnsureSuccessStatusCode();
                    break;
                }
                catch (HttpRequestException) { }
                catch (OperationCanceledException) when (!timeout.IsCancellationRequested) { }
                await Task.Delay(200, timeout.Token);
            }
            using var client = new LanguageToolClient(runtime.Server);
            var findings = await client.CheckAsync("Eles tem razão.", timeout.Token);
            if (!findings.Any(m => m.Replacements.Any(r => r.Contains("têm", StringComparison.Ordinal))))
                throw new InvalidDataException("Local LanguageTool did not pass its European Portuguese agreement self-test.");
            progress.Report("Local LanguageTool pt-PT ready (CPU, no generative model).");
            return runtime;
        }
        catch (OperationCanceledException ex) when (!token.IsCancellationRequested)
        {
            await runtime.DisposeAsync();
            throw new IOException("Local LanguageTool startup timed out after 90 seconds.", ex);
        }
        catch
        {
            await runtime.DisposeAsync();
            throw;
        }
    }

    internal static void KillAll()
    {
        foreach (var process in Running.Keys)
            Stop(process);
    }

    private static void Stop(Process process)
    {
        try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
        catch (InvalidOperationException) { /* Already stopped. */ }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            Stop(_process);
            await _process.WaitForExitAsync();
            await Task.WhenAll(_stdout, _stderr);
        }
        finally
        {
            Running.TryRemove(_process, out _);
            _process.Dispose();
            File.Delete(_configuration);
        }
    }
}
