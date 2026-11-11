using SubtitleEdit.Plugins.Shared;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

public sealed record EmbeddingSettings(string RuntimePath = "", string ModelPath = "", bool UseEmbeddings = false,
    bool Vulkan = false, string Device = "Vulkan0", bool CheckMeaning = true, bool UseJudge = false, string JudgeModelPath = "")
{
    // Paths always come from the plugin's own download folder, so they are not stored.
    [System.Text.Json.Serialization.JsonIgnore] public string RuntimePath { get; init; } = RuntimePath;
    [System.Text.Json.Serialization.JsonIgnore] public string ModelPath { get; init; } = ModelPath;
    [System.Text.Json.Serialization.JsonIgnore] public string JudgeModelPath { get; init; } = JudgeModelPath;
}

/// <summary>Pinned llama.cpp runtime and multilingual MiniLM model, downloaded only when the user clicks.</summary>
internal static class EmbeddingAssets
{
    internal const string Release = "b11707";
    internal const string ModelFile = "paraphrase-multilingual-MiniLM-L12-118M-v2-F16.gguf";
    internal const string ModelHash = "fa47907f8732d52ba31796d74bc87c4642b76da465d7d1f74df04ca89f3a9aab";
    internal const long ModelSize = 242358176;
    private const string ModelBase =
        "https://huggingface.co/mykor/paraphrase-multilingual-MiniLM-L12-v2.gguf/resolve/a964c09982b6e28b13646d5f9860b8a04f4182c4/";
    private const string ModelCardHash = "9e29bbfefed005bab2be743f75f210f682ac261479d26dccb03f2536f63acbb8";
    private static readonly HttpClient Client = new() { Timeout = Timeout.InfiniteTimeSpan };

    internal static string DirectoryPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SubtitleEdit", "BrazilianToEuropeanPortuguese", "ReferenceAlignment");

    private static string SettingsPath => Path.Combine(DirectoryPath, "settings.json");

    internal static EmbeddingSettings Load()
    {
        var settings = new EmbeddingSettings();
        try
        {
            if (File.Exists(SettingsPath))
            {
                settings = JsonSerializer.Deserialize<EmbeddingSettings>(File.ReadAllText(SettingsPath)) ?? new();
            }
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            // Unreadable settings fall back to defaults.
        }

        return WithInstalledFiles(settings);
    }

    /// <summary>The plugin only uses its own downloaded, hash-verified runtime and model.</summary>
    internal static EmbeddingSettings WithInstalledFiles(EmbeddingSettings settings)
    {
        var model = Path.Combine(DirectoryPath, ModelFile);
        var judge = Path.Combine(DirectoryPath, JudgeAssets.ModelFile);
        return settings with
        {
            RuntimePath = InstalledRuntime() ?? string.Empty,
            ModelPath = File.Exists(model) ? model : string.Empty,
            JudgeModelPath = File.Exists(judge) ? judge : string.Empty,
        };
    }

    internal static bool IsInstalled(EmbeddingSettings settings) =>
        File.Exists(settings.RuntimePath) && File.Exists(settings.ModelPath);

    /// <summary>Deletes earlier runtime downloads once a new one is in place.</summary>
    internal static void RemoveOtherRuntimes(string keep)
    {
        var root = Path.GetFullPath(DirectoryPath) + Path.DirectorySeparatorChar;
        foreach (var directory in Directory.GetDirectories(DirectoryPath, "runtime-*"))
        {
            var full = Path.GetFullPath(directory) + Path.DirectorySeparatorChar;
            if (!full.StartsWith(root, StringComparison.Ordinal) || Path.GetFullPath(keep).StartsWith(full, StringComparison.Ordinal)) continue;
            try { Directory.Delete(directory, true); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        }
    }

    internal static string? InstalledRuntime()
    {
        if (!Directory.Exists(DirectoryPath)) return null;
        return Directory.GetDirectories(DirectoryPath, "runtime-" + Release + "-*")
            .SelectMany(d => Directory.GetFiles(d, OperatingSystem.IsWindows() ? "llama-server.exe" : "llama-server", SearchOption.AllDirectories))
            .OrderBy(p => p, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    internal static void Save(EmbeddingSettings settings)
    {
        Directory.CreateDirectory(DirectoryPath);
        var temp = SettingsPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temp, JsonSerializer.Serialize(settings));
            File.Move(temp, SettingsPath, true);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }

    internal static (string File, string Hash) RuntimePackage()
    {
        var arch = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException("Downloads support x64 and ARM64 only; choose an installed llama-server."),
        };
        if (OperatingSystem.IsLinux())
            return ($"ubuntu-vulkan-{arch}.tar.gz", arch == "x64"
                ? "5c7726d41d34e4bde75d9b36b4b366a95c6ca64f5713ec787f6b973db1719504"
                : "acd15202fdc21d5a599da77a350d8033ece7ed8de206f22f9baa2fb37949f7aa");
        if (OperatingSystem.IsWindows())
            return ($"win-vulkan-{arch}.zip", arch == "x64"
                ? "9c7d17c12053bf95b03a179f9bcdf15fdeafe680a4f2845d23bc951bc18c0fa5"
                : "5903c75ebc433500d6e1d896562a75decf750bda35d7bab60fc4c53041196313");
        throw new PlatformNotSupportedException("On this platform choose an installed llama-server.");
    }

    internal static async Task<string> DownloadModelAsync(IProgress<string> progress, CancellationToken token)
    {
        Directory.CreateDirectory(DirectoryPath);
        var path = Path.Combine(DirectoryPath, ModelFile);
        await DownloadAsync(new Uri(ModelBase + "README.md"), path + ".MODEL-CARD.md", ModelCardHash, 65536, progress, token);
        if (!File.Exists(path) || !await HasHashAsync(path, ModelHash, token))
        {
            await DownloadAsync(new Uri(ModelBase + ModelFile), path, ModelHash, ModelSize, progress, token);
        }
        return path;
    }

    internal static async Task<bool> HasHashAsync(string path, string hash, CancellationToken token)
    {
        await using var stream = File.OpenRead(path);
        var digest = await SHA256.HashDataAsync(stream, token);
        return Convert.ToHexString(digest).Equals(hash, StringComparison.OrdinalIgnoreCase);
    }

    internal static async Task<string> DownloadRuntimeAsync(IProgress<string> progress, CancellationToken token)
    {
        var package = RuntimePackage();
        Directory.CreateDirectory(DirectoryPath);
        var archive = Path.Combine(DirectoryPath, $"llama-{Release}-{package.File}");
        await DownloadAsync(new Uri($"https://github.com/ggml-org/llama.cpp/releases/download/{Release}/llama-{Release}-bin-{package.File}"),
            archive, package.Hash, 512L * 1024 * 1024, progress, token);
        var install = Path.Combine(DirectoryPath, "runtime-" + Release + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(install);
        try
        {
            progress.Report("Extracting runtime...");
            await Task.Run(() => ExtractRuntime(archive, install, token), token);
            var executable = Directory.GetFiles(install, OperatingSystem.IsWindows() ? "llama-server.exe" : "llama-server",
                SearchOption.AllDirectories).Single();
            if (!OperatingSystem.IsWindows())
                File.SetUnixFileMode(executable, File.GetUnixFileMode(executable) | UnixFileMode.UserExecute);
            return executable;
        }
        catch
        {
            Directory.Delete(install, true);
            throw;
        }
        finally { File.Delete(archive); }
    }

    internal static string SafePath(string directory, string entry)
    {
        if (entry.Contains('\\') || entry.Contains(':') || Path.IsPathRooted(entry))
            throw new InvalidDataException("Unsafe runtime archive path.");
        var root = Path.GetFullPath(directory) + Path.DirectorySeparatorChar;
        var path = Path.GetFullPath(Path.Combine(directory, entry));
        if (!path.StartsWith(root, StringComparison.Ordinal))
            throw new InvalidDataException("Runtime archive path escapes its installation directory.");
        return path;
    }

    internal static void ExtractRuntime(string archive, string directory, CancellationToken token, bool allowInternalLinks = false)
    {
        long total = 0;
        int count = 0;
        var regular = new HashSet<string>(StringComparer.Ordinal);
        var links = new Dictionary<string, string>(StringComparer.Ordinal);
        void Copy(string name, long size, Stream input)
        {
            token.ThrowIfCancellationRequested();
            if (++count > 3000 || size < 0 || (total += size) > 2L * 1024 * 1024 * 1024)
                throw new InvalidDataException("Runtime archive exceeds extraction limits.");
            var path = SafePath(directory, name);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var output = new FileStream(path, FileMode.CreateNew);
            var buffer = new byte[65536];
            int read;
            while ((read = input.Read(buffer)) > 0)
            {
                token.ThrowIfCancellationRequested();
                output.Write(buffer, 0, read);
            }
            regular.Add(path);
        }
        if (archive.EndsWith(".zip", StringComparison.Ordinal))
        {
            using var zip = ZipFile.OpenRead(archive);
            foreach (var entry in zip.Entries)
            {
                SafePath(directory, entry.FullName);
                if (entry.FullName.EndsWith('/')) continue;
                var kind = (entry.ExternalAttributes >> 16) & 0xF000;
                if (kind == 0xA000) throw new InvalidDataException("Archive links are not allowed.");
                using var input = entry.Open();
                Copy(entry.FullName, entry.Length, input);
            }
        }
        else
        {
            using var input = File.OpenRead(archive);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var tar = new TarReader(gzip);
            TarEntry? entry;
            while ((entry = tar.GetNextEntry()) != null)
            {
                if (entry.EntryType == TarEntryType.Directory) continue;
                if (entry.EntryType == TarEntryType.SymbolicLink)
                {
                    if (++count > 3000) throw new InvalidDataException("Too many runtime entries.");
                    var path = SafePath(directory, entry.Name);
                    if (entry.LinkName.Contains('\\') || entry.LinkName.Contains(':') || Path.IsPathRooted(entry.LinkName) ||
                        entry.LinkName is "." or ".." ||
                        !allowInternalLinks && entry.LinkName != Path.GetFileName(entry.LinkName))
                        throw new InvalidDataException("Only safe internal archive aliases are allowed.");
                    links.Add(path, SafePath(directory, Path.Combine(Path.GetDirectoryName(entry.Name)!, entry.LinkName)));
                    continue;
                }
                if (entry.EntryType is not (TarEntryType.RegularFile or TarEntryType.V7RegularFile))
                    throw new InvalidDataException("Archive links and special entries are not allowed.");
                Copy(entry.Name, entry.Length, entry.DataStream!);
            }
        }
        // Aliases become regular copies; their final target must be an extracted regular file.
        foreach (var link in links)
        {
            var target = link.Value;
            var visited = new HashSet<string> { link.Key };
            while (links.TryGetValue(target, out var next))
            {
                if (!visited.Add(target)) throw new InvalidDataException("Circular runtime library alias.");
                target = next;
            }
            if (!regular.Contains(target)) throw new InvalidDataException("Runtime alias has no regular-file target.");
            using var input = File.OpenRead(target);
            Copy(Path.GetRelativePath(directory, link.Key), input.Length, input);
        }
    }

    internal static async Task DownloadAsync(Uri uri, string destination, string hash, long maximum,
        IProgress<string> progress, CancellationToken token, HttpClient? client = null)
    {
        var temporary = destination + "." + Guid.NewGuid().ToString("N") + ".part";
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromHours(12));
        try
        {
            using var response = await (client ?? Client).GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            response.EnsureSuccessStatusCode();
            await using var input = await response.Content.ReadAsStreamAsync(timeout.Token);
            using var digest = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            long received = 0, reported = -1;
            await using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 65536, true))
            {
                var buffer = new byte[65536];
                int read;
                while ((read = await input.ReadAsync(buffer, timeout.Token)) > 0)
                {
                    if ((received += read) > maximum) throw new InvalidDataException("Download exceeds its permitted size.");
                    digest.AppendData(buffer, 0, read);
                    await output.WriteAsync(buffer.AsMemory(0, read), timeout.Token);
                    var megabytes = received / (1024 * 1024);
                    if (megabytes / 10 != reported)
                    {
                        reported = megabytes / 10;
                        progress.Report($"Downloading {Path.GetFileName(destination)}: {megabytes} MiB");
                    }
                }
            }
            if (!Convert.ToHexString(digest.GetHashAndReset()).Equals(hash, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("SHA-256 mismatch; download was not installed.");
            File.Move(temporary, destination, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}

/// <summary>An owned local llama-server in embedding mode, on CPU or a Vulkan GPU. Nothing leaves the computer.</summary>
internal sealed class EmbeddingRuntime : IAsyncDisposable
{
    private readonly Process _process;
    private readonly HttpClient _client;
    private readonly StringBuilder _log = new();
    private readonly Task _stdout;
    private readonly Task _stderr;
    private int _dimensions;
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Process, byte> Running = new();

    static EmbeddingRuntime() => AppDomain.CurrentDomain.ProcessExit += (_, _) => KillAll();

    /// <summary>Stops every owned server at once, e.g. when the window closes while MiniLM is running.</summary>
    internal static void KillAll()
    {
        foreach (var process in Running.Keys)
        {
            try
            {
                if (!process.HasExited) process.Kill(entireProcessTree: true);
            }
            catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
            {
                // Already gone.
            }
        }
    }

    public bool UsesGpu { get; }
    public string Layers { get; private set; } = string.Empty;

    private EmbeddingRuntime(Process process, HttpClient client, bool gpu)
    {
        _process = process;
        _client = client;
        UsesGpu = gpu;
        Running.TryAdd(process, 0);
        _stdout = ReadLog(process.StandardOutput);
        _stderr = ReadLog(process.StandardError);
    }

    private async Task ReadLog(StreamReader reader)
    {
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
            lock (_log)
            {
                if (_log.Length > 32000) _log.Remove(0, _log.Length - 16000);
                _log.AppendLine(line);
            }
    }

    private string Log { get { lock (_log) return _log.ToString(); } }

    private static ProcessStartInfo StartInfo(string executable)
    {
        var full = Path.GetFullPath(executable);
        var folder = Path.GetDirectoryName(full)!;
        var start = new ProcessStartInfo(full)
        {
            UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true,
            WorkingDirectory = folder,
        };
        foreach (var key in start.Environment.Keys.Where(k => k.StartsWith("LLAMA_ARG_", StringComparison.Ordinal)).ToList())
            start.Environment.Remove(key);
        if (OperatingSystem.IsLinux())
        {
            start.Environment.TryGetValue("LD_LIBRARY_PATH", out var existing);
            start.Environment["LD_LIBRARY_PATH"] = string.IsNullOrEmpty(existing) ? folder : folder + Path.PathSeparator + existing;
        }
        return start;
    }

    internal static IReadOnlyList<(string Id, string Name)> ParseDevices(string output) =>
        Regex.Matches(output, @"(?m)^\s*(Vulkan\d+)\s*:\s*(.+?)\s*$")
            .Select(m => (m.Groups[1].Value, m.Groups[2].Value))
            .Where(d => !Regex.IsMatch(d.Item2, "llvmpipe|lavapipe|swiftshader", RegexOptions.IgnoreCase))
            .ToList();

    internal static async Task<IReadOnlyList<(string Id, string Name)>> ListDevicesAsync(string executable, CancellationToken token)
    {
        if (!File.Exists(executable)) throw new FileNotFoundException("Download or choose llama-server first.");
        var start = StartInfo(executable);
        start.ArgumentList.Add("--list-devices");
        using var process = Process.Start(start) ?? throw new IOException("Could not inspect runtime devices.");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromSeconds(60));
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        try
        {
            await process.WaitForExitAsync(timeout.Token);
            var text = await output + "\n" + await error;
            if (process.ExitCode != 0) throw new IOException("Device discovery failed: " + text.Trim());
            return ParseDevices(text);
        }
        finally
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync(CancellationToken.None);
        }
    }

    internal static Task<EmbeddingRuntime> StartAsync(EmbeddingSettings settings, IProgress<string> progress, CancellationToken token)
    {
        if (!File.Exists(settings.ModelPath)) throw new FileNotFoundException("Download or choose the MiniLM GGUF model first.");
        return StartServerAsync(settings, settings.ModelPath, "MiniLM", new[]
        {
            "--embedding", "--pooling", "mean", "--embd-normalize", "2",
            "--ctx-size", "512", "--batch-size", "512", "--ubatch-size", "512", "--parallel", "1",
        }, true, TimeSpan.FromMinutes(3), progress, token);
    }

    /// <summary>Gervásio PT-PT in scoring mode: it never writes text, it only rates candidate lines.</summary>
    internal static Task<EmbeddingRuntime> StartJudgeAsync(EmbeddingSettings settings, IProgress<string> progress, CancellationToken token)
    {
        if (!File.Exists(settings.JudgeModelPath)) throw new FileNotFoundException("Download the Gervásio PT-PT model first.");
        // Vulkan: layers that do not fit in the GPU's memory stay on the CPU (llama.cpp --fit, -ngl auto).
        return StartServerAsync(settings, settings.JudgeModelPath, "Gervásio", new[]
        {
            "--ctx-size", (JudgeSlots * 512).ToString(), "--parallel", JudgeSlots.ToString(), "--batch-size", "512",
        }, false, TimeSpan.FromMinutes(10), progress, token);
    }

    internal const int JudgeSlots = 4;

    private static async Task<EmbeddingRuntime> StartServerAsync(EmbeddingSettings settings, string modelPath, string label,
        IEnumerable<string> modeArguments, bool embedding, TimeSpan startupLimit, IProgress<string> progress, CancellationToken token)
    {
        if (!File.Exists(settings.RuntimePath)) throw new FileNotFoundException("Download or choose llama-server first.");
        await using (var model = File.OpenRead(modelPath))
        {
            var magic = new byte[4];
            await model.ReadExactlyAsync(magic, token);
            if (!magic.SequenceEqual("GGUF"u8.ToArray())) throw new InvalidDataException("The chosen model is not a GGUF file.");
        }
        if (settings.Vulkan && !Regex.IsMatch(settings.Device, @"^Vulkan\d+$"))
            throw new InvalidDataException("Enter a Vulkan device such as Vulkan0 (use List devices).");

        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        var key = Guid.NewGuid().ToString("N");
        var start = StartInfo(settings.RuntimePath);
        foreach (var argument in new[]
        {
            "-m", Path.GetFullPath(modelPath),
            "--host", "127.0.0.1", "--port", port.ToString(), "--api-key", key,
            "--no-warmup", "--no-webui", "--offline", "-lv", "4",
        }.Concat(modeArguments)) start.ArgumentList.Add(argument);
        if (settings.Vulkan)
        {
            start.ArgumentList.Add("--device");
            start.ArgumentList.Add(settings.Device);
            if (embedding)
            {
                start.ArgumentList.Add("-ngl");
                start.ArgumentList.Add("99");
            }
        }
        else
        {
            start.ArgumentList.Add("--device");
            start.ArgumentList.Add("none");
            start.ArgumentList.Add("-ngl");
            start.ArgumentList.Add("0");
        }

        var process = Process.Start(start) ?? throw new IOException("Could not start llama-server.");
        var client = new HttpClient(new HttpClientHandler { UseProxy = false })
        {
            BaseAddress = new Uri($"http://127.0.0.1:{port}/"), Timeout = TimeSpan.FromMinutes(2),
        };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
        var runtime = new EmbeddingRuntime(process, client, settings.Vulkan);
        try
        {
            progress.Report(settings.Vulkan ? $"Starting {label} on {settings.Device}..." : $"Starting {label} on CPU...");
            using var startup = CancellationTokenSource.CreateLinkedTokenSource(token);
            startup.CancelAfter(startupLimit);
            while (true)
            {
                startup.Token.ThrowIfCancellationRequested();
                if (process.HasExited) throw new IOException("llama-server exited during startup:\n" + runtime.Tail());
                try
                {
                    using var response = await client.GetAsync("health", startup.Token);
                    if (response.IsSuccessStatusCode) break;
                    if (response.StatusCode != HttpStatusCode.ServiceUnavailable)
                        throw new IOException("Unexpected health response: " + response.StatusCode);
                }
                catch (HttpRequestException)
                {
                    // Connection refused until the owned server binds its port.
                }
                await Task.Delay(200, startup.Token);
            }

            // Gervásio may get no layers when the GPU is busy or small; it then runs on the CPU, which is only slower.
            var offload = Regex.Match(runtime.Log, @"offloaded (\d+)/(\d+) layers to GPU");
            if (settings.Vulkan && (!offload.Success || (embedding && offload.Groups[1].Value == "0")))
                throw new IOException("Vulkan offloading could not be verified; untick Vulkan GPU to use the CPU.\n" + runtime.Tail());
            if (embedding) await runtime.SelfTestAsync(token);
            else await runtime.JudgeSelfTestAsync(token);
            runtime.Layers = offload.Success ? $"{offload.Groups[1].Value}/{offload.Groups[2].Value} layers on the GPU" : string.Empty;
            progress.Report(settings.Vulkan ? $"{label} ready on {settings.Device} ({runtime.Layers})." : $"{label} ready on CPU.");
            return runtime;
        }
        catch
        {
            await runtime.DisposeAsync();
            throw;
        }
    }

    private string Tail()
    {
        var lines = Log.Split('\n');
        return string.Join("\n", lines.Skip(Math.Max(0, lines.Length - 15)));
    }

    private async Task SelfTestAsync(CancellationToken token)
    {
        var hello = await EmbedAsync("Hello, how are you?", token);
        var ola = await EmbedAsync("Olá, como estás?", token);
        var train = await EmbedAsync("The train leaves tomorrow.", token);
        var related = ReferenceAligner.Dot(hello, ola);
        if (related < 0.7 || related <= ReferenceAligner.Dot(hello, train) + 0.1)
            throw new InvalidDataException($"The model failed the translation self-test (similarity {related:0.00}); use the MiniLM model.");
    }

    private async Task JudgeSelfTestAsync(CancellationToken token)
    {
        const string context = "Legenda em português de Portugal:\n";
        var right = await ScoreAsync(context, "Eles têm razão.", token);
        var wrong = await ScoreAsync(context, "Eles tem razão.", token);
        if (right <= wrong)
            throw new InvalidDataException("The model failed the Portuguese self-test; download the Gervásio PT-PT model again.");
    }

    internal static string TokenGrammar(IEnumerable<int> ids) => "root ::= " + string.Join(" ", ids.Select(id => $"<[{id}]>"));


    /// <summary>
    /// Log-probability of <paramref name="text"/> after <paramref name="context"/>. The grammar forces the exact text,
    /// and llama-server reports each token's probability from the raw model output (before the grammar), so this is
    /// the model's own opinion of the line. Higher is more natural.
    /// </summary>
    public async Task<double> ScoreAsync(string context, string text, CancellationToken token)
    {
        if (_process.HasExited) throw new IOException("llama-server stopped:\n" + Tail());
        // Force the model's own tokenisation by token id; a text grammar lets greedy decoding split words oddly.
        using var tokenize = await _client.PostAsJsonAsync("tokenize", new { content = text, add_special = false, parse_special = false }, token);
        tokenize.EnsureSuccessStatusCode();
        using var tokenJson = JsonDocument.Parse(await tokenize.Content.ReadAsStringAsync(token));
        var ids = tokenJson.RootElement.GetProperty("tokens").EnumerateArray().Select(t => t.GetInt32()).ToList();
        if (ids.Count == 0) return 0;
        using var response = await _client.PostAsJsonAsync("completion", new
        {
            prompt = context, grammar = TokenGrammar(ids), n_predict = ids.Count, n_probs = 1,
            temperature = 0.0, cache_prompt = true, stream = false,
        }, token);
        if (!response.IsSuccessStatusCode)
            throw new IOException($"Scoring request failed ({(int)response.StatusCode}): {await response.Content.ReadAsStringAsync(token)}");
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(token));
        var content = json.RootElement.GetProperty("content").GetString();
        var probabilities = json.RootElement.GetProperty("completion_probabilities");
        if (content != text || probabilities.GetArrayLength() < ids.Count)
            throw new InvalidDataException("The scoring model did not follow the line it was asked to rate.");
        double total = 0;
        foreach (var item in probabilities.EnumerateArray().Take(ids.Count))
        {
            var logprob = item.GetProperty("logprob").GetDouble();
            if (!double.IsFinite(logprob)) logprob = -30;
            total += Math.Max(logprob, -30);
        }
        return total;
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken token)
    {
        if (_process.HasExited) throw new IOException("llama-server stopped:\n" + Tail());
        using var tokenize = await _client.PostAsJsonAsync("tokenize",
            new { content = text, add_special = true, parse_special = false }, token);
        tokenize.EnsureSuccessStatusCode();
        using var tokenJson = JsonDocument.Parse(await tokenize.Content.ReadAsStringAsync(token));
        var tokens = tokenJson.RootElement.GetProperty("tokens").EnumerateArray().Select(t => t.GetInt32()).ToList();
        // MiniLM was trained on 128 tokens; keep the start and the closing special token.
        if (tokens.Count > 128) tokens = tokens.Take(127).Append(tokens[^1]).ToList();

        using var response = await _client.PostAsJsonAsync("v1/embeddings", new { input = tokens, encoding_format = "float" }, token);
        if (!response.IsSuccessStatusCode)
            throw new IOException($"Embedding request failed ({(int)response.StatusCode}): {await response.Content.ReadAsStringAsync(token)}");
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(token));
        var vector = json.RootElement.GetProperty("data")[0].GetProperty("embedding").EnumerateArray().Select(v => v.GetSingle()).ToArray();
        double norm = 0;
        foreach (var value in vector)
        {
            if (!float.IsFinite(value)) throw new InvalidDataException("The model returned a non-finite embedding.");
            norm += value * value;
        }
        if (vector.Length == 0 || norm < 0.0001) throw new InvalidDataException("The model returned an empty embedding.");
        if (_dimensions == 0) _dimensions = vector.Length;
        else if (vector.Length != _dimensions) throw new InvalidDataException("The model returned embeddings of different sizes.");
        var scale = (float)(1 / Math.Sqrt(norm));
        for (var i = 0; i < vector.Length; i++) vector[i] *= scale;
        return vector;
    }

    /// <summary>Embeds every distinct cleaned cue text once.</summary>
    public async Task<float[]?[]> EmbedCuesAsync(IReadOnlyList<string> texts, IProgress<string> progress, string label, CancellationToken token)
    {
        var cache = new Dictionary<string, float[]>(StringComparer.Ordinal);
        var result = new float[]?[texts.Count];
        for (var i = 0; i < texts.Count; i++)
        {
            var clean = ReferenceAligner.Clean(texts[i]);
            if (!Regex.IsMatch(clean, @"[\p{L}\p{N}]")) continue;
            if (!cache.TryGetValue(clean, out var vector))
            {
                vector = await EmbedAsync(clean, token);
                cache[clean] = vector;
            }
            result[i] = vector;
            if (i % 50 == 0) progress.Report($"MiniLM: {label} {i + 1}/{texts.Count}...");
        }
        return result;
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        try
        {
            if (!_process.HasExited) _process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException) when (_process.HasExited)
        {
            // The owned server can exit between HasExited and Kill.
        }
        await _process.WaitForExitAsync(CancellationToken.None);
        await Task.WhenAll(_stdout, _stderr);
        Running.TryRemove(_process, out _);
        _process.Dispose();
    }
}

/// <summary>Aligns an English reference with the subtitle: MiniLM (CPU or Vulkan) when ticked and installed, else timing.</summary>
internal static class ReferenceRunner
{
    internal static async Task<(ReferenceAlignment Alignment, string Note)> AlignAsync(IReadOnlyList<SrtBlock> blocks,
        IReadOnlyList<SrtBlock> reference, EmbeddingSettings settings, IProgress<string> progress, CancellationToken token)
    {
        ReferenceAlignment alignment;
        var note = string.Empty;
        if (!EmbeddingAssets.IsInstalled(settings)) settings = EmbeddingAssets.WithInstalledFiles(settings);
        if (settings.UseEmbeddings && EmbeddingAssets.IsInstalled(settings))
        {
            await using var runtime = await EmbeddingRuntime.StartAsync(settings, progress, token);
            var source = await runtime.EmbedCuesAsync(blocks.Select(b => b.Text).ToList(), progress, "subtitle", token);
            var english = await runtime.EmbedCuesAsync(reference.Select(b => b.Text).ToList(), progress, "reference", token);
            alignment = ReferenceAligner.AlignWithEmbeddings(blocks, reference, source, english);
            note = runtime.UsesGpu ? $" MiniLM ran on {settings.Device}." : " MiniLM ran on the CPU.";
        }
        else
        {
            alignment = ReferenceAligner.AlignByTiming(blocks, reference);
            if (settings.UseEmbeddings)
                note = " MiniLM is not installed, so only timing was used - download the MiniLM model and llama-server.";
            else if (alignment.Coverage < ReferenceAligner.MinimumUsefulCoverage)
                note = " Few cues overlap: the timing probably differs - tick 'Check timing with MiniLM'.";
        }

        if (!ReferenceAligner.LooksEnglish(reference))
            note += " This does not look like English; only English pronouns (his/her/their) are used.";
        return (alignment, note);
    }
}
