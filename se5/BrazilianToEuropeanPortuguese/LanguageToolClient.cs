using System.Net.Http;
using System.Text.Json;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal sealed record GrammarMatch(int Offset, int Length, string Rule, string Message, string[] Replacements);

internal sealed class LanguageToolClient : IDisposable
{
    private readonly HttpClient _client;
    private readonly Uri _checkUri;
    private readonly Dictionary<string, IReadOnlyList<GrammarMatch>> _cache = new(StringComparer.Ordinal);

    internal LanguageToolClient(string server, HttpMessageHandler? handler = null)
    {
        _checkUri = CheckUri(server);
        _client = new HttpClient(handler ?? new HttpClientHandler { AllowAutoRedirect = false, UseProxy = false })
        {
            Timeout = TimeSpan.FromSeconds(15),
            MaxResponseContentBufferSize = 2 * 1024 * 1024,
        };
    }

    internal static Uri CheckUri(string server)
    {
        if (!Uri.TryCreate(server, UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("http" or "https") || uri.UserInfo.Length != 0 ||
            uri.Query.Length != 0 || uri.Fragment.Length != 0 ||
            uri.Host.Equals("languagetool.org", StringComparison.OrdinalIgnoreCase) ||
            uri.Host.EndsWith(".languagetool.org", StringComparison.OrdinalIgnoreCase) ||
            uri.Host.Equals("languagetoolplus.com", StringComparison.OrdinalIgnoreCase) ||
            uri.Host.EndsWith(".languagetoolplus.com", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("LanguageTool: enter an HTTP(S) URL for your self-hosted server, without credentials, query or fragment. Public LanguageTool cloud endpoints are not supported.");
        var address = uri.AbsoluteUri.TrimEnd('/');
        if (address.EndsWith("/v2/check", StringComparison.Ordinal)) return new Uri(address);
        return new Uri(address + (address.EndsWith("/v2", StringComparison.Ordinal) ? "/check" : "/v2/check"));
    }

    internal async Task<IReadOnlyList<GrammarMatch>> CheckAsync(string text, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (_cache.TryGetValue(text, out var cached)) return cached;
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["language"] = "pt-PT",
            ["text"] = text,
        });
        HttpResponseMessage response;
        try
        {
            response = await _client.PostAsync(_checkUri, content, token);
        }
        catch (OperationCanceledException ex) when (!token.IsCancellationRequested)
        {
            throw new IOException("LanguageTool: the self-hosted server timed out after 15 seconds.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new IOException("LanguageTool: could not contact the configured self-hosted server. " + ex.Message, ex);
        }
        using (response)
        {
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"LanguageTool: self-hosted server returned HTTP {(int)response.StatusCode}.", null, response.StatusCode);
            try
            {
                using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(token));
                var matches = Parse(json.RootElement, text);
                _cache.Add(text, matches);
                return matches;
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException("LanguageTool: invalid JSON response from the self-hosted server.", ex);
            }
        }
    }

    internal static IReadOnlyList<GrammarMatch> Parse(JsonElement root, string text)
    {
        if (root.ValueKind != JsonValueKind.Object ||
            !root.TryGetProperty("language", out var language) || language.ValueKind != JsonValueKind.Object ||
            !language.TryGetProperty("code", out var code) || code.ValueKind != JsonValueKind.String || code.GetString() != "pt-PT" ||
            !root.TryGetProperty("matches", out var matches) || matches.ValueKind != JsonValueKind.Array)
            throw new InvalidDataException("LanguageTool: expected a complete pt-PT check response.");
        if (root.TryGetProperty("warnings", out var warnings))
        {
            if (warnings.ValueKind != JsonValueKind.Object)
                throw new InvalidDataException("LanguageTool: invalid warnings field.");
            if (warnings.TryGetProperty("incompleteResults", out var incomplete))
            {
                if (incomplete.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                    throw new InvalidDataException("LanguageTool: invalid incompleteResults field.");
                if (incomplete.ValueKind == JsonValueKind.True)
                    throw new InvalidDataException("LanguageTool: server returned incomplete results; no corrections were applied.");
            }
        }
        var result = new List<GrammarMatch>();
        foreach (var match in matches.EnumerateArray())
        {
            if (match.ValueKind != JsonValueKind.Object ||
                !match.TryGetProperty("offset", out var offset) || offset.ValueKind != JsonValueKind.Number || !offset.TryGetInt32(out var start) ||
                !match.TryGetProperty("length", out var length) || length.ValueKind != JsonValueKind.Number || !length.TryGetInt32(out var count) ||
                start < 0 || count < 0 || start > text.Length - count ||
                !Boundary(text, start) || !Boundary(text, start + count) ||
                !match.TryGetProperty("message", out var message) || message.ValueKind != JsonValueKind.String ||
                !match.TryGetProperty("rule", out var rule) || rule.ValueKind != JsonValueKind.Object ||
                !rule.TryGetProperty("id", out var id) || id.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(id.GetString()) ||
                !match.TryGetProperty("replacements", out var replacements) || replacements.ValueKind != JsonValueKind.Array)
                throw new InvalidDataException("LanguageTool: invalid match offsets or missing diagnostic fields.");
            var values = new List<string>();
            foreach (var replacement in replacements.EnumerateArray())
            {
                if (replacement.ValueKind != JsonValueKind.Object ||
                    !replacement.TryGetProperty("value", out var value) || value.ValueKind != JsonValueKind.String)
                    throw new InvalidDataException("LanguageTool: invalid replacement.");
                values.Add(value.GetString()!);
            }
            result.Add(new GrammarMatch(start, count, id.GetString()!, message.GetString()!, values.Distinct().ToArray()));
        }
        return result;
    }

    private static bool Boundary(string text, int offset) =>
        offset == 0 || offset == text.Length || !char.IsLowSurrogate(text[offset]) || !char.IsHighSurrogate(text[offset - 1]);

    public void Dispose() => _client.Dispose();
}
