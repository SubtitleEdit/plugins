using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WeCantSpell.Hunspell;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal sealed class NaturaDictionary
{
    private const string BaseUrl = "https://natura.di.uminho.pt/download/sources/Dictionaries/hunspell/LATEST/";
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(90) };
    private static readonly Dictionary<string, (DateTime Modified, WordList Words)> Cache = new();
    private static readonly Regex Tokens = new(@"[\p{L}\p{M}]+(?:-[\p{L}\p{M}]+)*");
    private readonly WordList _words;

    private NaturaDictionary(WordList words) => _words = words;

    internal static string DirectoryPath => Path.Combine(
        Path.GetDirectoryName(PortugueseLocalWordList.GetDefaultPath())!, "Natura");

    internal static string FileName(bool postAgreement) => postAgreement ? "natura-ao.tar.gz" : "natura-preao.tar.gz";

    internal static NaturaDictionary Load(bool postAgreement, string? directory = null)
    {
        var file = Path.Combine(directory ?? DirectoryPath, FileName(postAgreement));
        var modified = File.Exists(file) ? File.GetLastWriteTimeUtc(file) : DateTime.MinValue;
        lock (Cache)
        {
            if (!Cache.TryGetValue(file, out var cached) || cached.Modified != modified)
            {
                using var stream = File.Exists(file)
                    ? File.OpenRead(file)
                    : typeof(NaturaDictionary).Assembly.GetManifestResourceStream(
                        $"BrazilianToEuropeanPortuguese.Dictionaries.{FileName(postAgreement)}")
                      ?? throw new InvalidDataException("Bundled Natura dictionary is missing.");
                cached = (modified, ReadArchive(stream).Words);
                Cache[file] = cached;
            }
            return new NaturaDictionary(cached.Words);
        }
    }

    internal bool Check(string word) => _words.Check(word);

    internal string[] UnknownWords(IEnumerable<string> texts) => texts
        .Select(t => Regex.Replace(t, @"<[^<>]*>|\{[^{}]*\}", " "))
        .SelectMany(t => Tokens.Matches(t).Select(m => m.Value))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Where(word => !Check(word))
        .OrderBy(word => word, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    internal static (WordList Words, string Version) ReadArchive(Stream archive)
    {
        using var gzip = new GZipStream(archive, CompressionMode.Decompress, leaveOpen: true);
        using var reader = new TarReader(gzip);
        using var dic = new MemoryStream();
        using var aff = new MemoryStream();
        string? version = null;
        bool hasLicense = false;
        int entries = 0;
        TarEntry? entry;
        while ((entry = reader.GetNextEntry()) != null)
        {
            if (++entries > 30 || entry.Length > 8 * 1024 * 1024)
                throw new InvalidDataException("Natura dictionary archive exceeds the permitted size.");
            if (entry.EntryType != TarEntryType.RegularFile && entry.EntryType != TarEntryType.V7RegularFile)
                continue;
            var name = Path.GetFileName(entry.Name);
            if (name == "COPYING") hasLicense = true;
            if (name is not ("pt_PT.dic" or "pt_PT.aff" or "pt_PT-preao.dic" or "pt_PT-preao.aff")) continue;
            version = entry.Name.Split('/')[0];
            entry.DataStream!.CopyTo(name.EndsWith(".dic", StringComparison.Ordinal) ? dic : aff);
        }
        if (dic.Length == 0 || aff.Length == 0 || !hasLicense || version == null)
            throw new InvalidDataException("Natura archive lacks its dictionary, affixes or licence.");
        dic.Position = aff.Position = 0;
        var words = WordList.CreateFromStreams(dic, aff);
        if (!words.Check("casa") || !words.Check("português"))
            throw new InvalidDataException("Natura archive is not a usable Portuguese dictionary.");
        return (words, version);
    }

    internal static async Task<string> UpdateAsync(bool postAgreement, bool download,
        HttpClient? client = null, string? directory = null)
    {
        directory ??= DirectoryPath;
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        var name = FileName(postAgreement);
        var urlName = postAgreement ? "hunspell-pt_PT-latest.tar.gz" : "hunspell-pt_PT-preao-latest.tar.gz";
        using var response = await (client ?? Client).GetAsync(BaseUrl + urlName,
            HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        using var stream = await response.Content.ReadAsStreamAsync(timeout.Token).ConfigureAwait(false);
        using var bytes = new MemoryStream();
        var buffer = new byte[8192];
        int read;
        while ((read = await stream.ReadAsync(buffer.AsMemory(), timeout.Token).ConfigureAwait(false)) > 0)
        {
            if (bytes.Length + read > 2 * 1024 * 1024)
                throw new InvalidDataException("Natura download exceeds the permitted size.");
            bytes.Write(buffer, 0, read);
        }
        bytes.Position = 0;
        var parsed = ReadArchive(bytes);
        var path = Path.Combine(directory, name);
        using var current = File.Exists(path) ? File.OpenRead(path)
            : typeof(NaturaDictionary).Assembly.GetManifestResourceStream($"BrazilianToEuropeanPortuguese.Dictionaries.{name}")
              ?? throw new InvalidDataException("Bundled Natura dictionary is missing.");
        bool same = SHA256.HashData(current).SequenceEqual(SHA256.HashData(bytes.ToArray()));
        current.Dispose();
        if (!download) return same ? $"Dictionary is up to date ({parsed.Version})." : $"Update available: {parsed.Version}. Click Re-download.";
        Directory.CreateDirectory(directory);
        var temporary = Path.Combine(directory, Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            await File.WriteAllBytesAsync(temporary, bytes.ToArray(), timeout.Token).ConfigureAwait(false);
            File.Move(temporary, path, overwrite: true);
            lock (Cache) Cache.Remove(path);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
        return $"Dictionary downloaded and validated ({parsed.Version}).";
    }
}
