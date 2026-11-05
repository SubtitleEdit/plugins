using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

/// <summary>Gervásio 8B PT-PT (University of Lisbon), pinned and SHA-256 verified, downloaded only when the user clicks.</summary>
internal static class JudgeAssets
{
    internal const string ModelFile = "gervasio-8b-portuguese-ptpt-decoder.Q4_K_M.gguf";
    internal const string ModelHash = "092660e6b66ccec643f4ec03d1d06f77b04cca7e5772e77678df213abe4bce37";
    internal const long ModelSize = 4920735264;
    private const string ModelBase =
        "https://huggingface.co/mradermacher/gervasio-8b-portuguese-ptpt-decoder-GGUF/resolve/6bd048b4e3e14e3946894d5d4e094c69ab73f4ff/";
    private const string ModelCardHash = "44c5363bb09678ce934e0b07f79c84cad451f87a3cc040c81182d014e796c9bb";

    internal static async Task<string> DownloadModelAsync(IProgress<string> progress, CancellationToken token)
    {
        Directory.CreateDirectory(EmbeddingAssets.DirectoryPath);
        var path = Path.Combine(EmbeddingAssets.DirectoryPath, ModelFile);
        await EmbeddingAssets.DownloadAsync(new Uri(ModelBase + "README.md"), path + ".MODEL-CARD.md", ModelCardHash, 65536, progress, token);
        if (!File.Exists(path) || new FileInfo(path).Length != ModelSize || !await EmbeddingAssets.HasHashAsync(path, ModelHash, token))
        {
            await EmbeddingAssets.DownloadAsync(new Uri(ModelBase + ModelFile), path, ModelHash, ModelSize, progress, token);
        }
        return path;
    }
}

/// <summary>
/// The only choices Gervásio may make: each candidate swaps one word of the converted line for a fixed alternative.
/// The model never writes text, so it cannot invent words or change the meaning.
/// </summary>
internal static class JudgeAlternatives
{
    private sealed record Swap(string From, string To, string? SourcePattern = null, bool NeedsPlural = false);

    private static readonly Swap[] Swaps = BuildSwaps();

    private static Swap[] BuildSwaps()
    {
        var swaps = new List<Swap>();
        void Both(string a, string b, string? source = null)
        {
            // Singular → plural only with a plural-looking word before it: "Qual é, tem de nos dar" stays singular.
            swaps.Add(new Swap(a, b, source, b.EndsWith("êm", StringComparison.Ordinal)));
            swaps.Add(new Swap(b, a, source));
        }

        // Plural agreement of ter/vir and their compounds.
        Both("tem", "têm");
        Both("vem", "vêm");
        foreach (var stem in new[] { "man", "con", "ob", "de", "re", "abs", "en", "sus" })
        {
            Both(stem + "tém", stem + "têm");
        }
        foreach (var stem in new[] { "con", "pro", "inter", "pre", "sobre", "pro" }.Distinct())
        {
            Both(stem + "vém", stem + "vêm");
        }
        // Gender agreement of mau/má.
        Both("mau", "má");
        Both("maus", "más");
        // "tira" left unchanged by the rules: the noun (cop) → polícia; the verb tirar stays.
        swaps.Add(new Swap("tira", "polícia"));
        swaps.Add(new Swap("tiras", "polícias"));
        // garoto → miúdo by default; an adult "boy" (your boy is lead detective) is "rapaz".
        swaps.Add(new Swap("miúdo", "rapaz", "garot"));
        swaps.Add(new Swap("miúda", "rapariga", "garot"));
        swaps.Add(new Swap("miúdos", "rapazes", "garot"));
        swaps.Add(new Swap("miúdas", "raparigas", "garot"));
        return swaps.ToArray();
    }

    private static readonly Dictionary<string, Swap[]> ByWord = Swaps
        .GroupBy(s => s.From, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(g => g.Key, g => g.ToArray(), StringComparer.OrdinalIgnoreCase);

    private static readonly Regex Word = new(@"(?<![\p{L}\p{M}\d_-])[\p{L}\p{M}]+(?![\p{L}\p{M}\d_])", RegexOptions.CultureInvariant);

    private static readonly Regex PluralWord = new(@"(?<![\p{L}\p{M}])(?:\p{L}{2,}s|eles|elas|vocês|ambos)(?![\p{L}\p{M}])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly HashSet<string> NotPlural = new(StringComparer.OrdinalIgnoreCase)
    {
        "mas", "mais", "menos", "depois", "pois", "nós", "vós", "às", "atrás", "através", "lápis", "talvez", "simples", "após", "antes", "apenas", "quais", "jamais", "demais",
    };

    /// <summary>A word ending in -s among the five words before <paramref name="index"/>, in the same sentence.</summary>
    private static bool PluralBefore(string text, int index)
    {
        var before = text.Substring(0, index);
        var sentence = before.LastIndexOfAny(new[] { '.', '!', '?', '…' });
        var words = Word.Matches(before.Substring(sentence + 1)).Select(m => m.Value).TakeLast(5);
        return words.Any(w => PluralWord.IsMatch(w) && !NotPlural.Contains(w));
    }

    internal static IReadOnlyList<(string Text, string Change)> Generate(string original, string converted)
    {
        var result = new List<(string, string)>();
        foreach (Match match in Word.Matches(converted))
        {
            if (!ByWord.TryGetValue(match.Value, out var swaps)) continue;
            foreach (var swap in swaps)
            {
                if (swap.SourcePattern != null && original.IndexOf(swap.SourcePattern, StringComparison.OrdinalIgnoreCase) < 0) continue;
                if (swap.NeedsPlural && !PluralBefore(converted, match.Index)) continue;
                var replacement = CaseHelper.MatchCase(match.Value, swap.To);
                var text = converted.Substring(0, match.Index) + replacement + converted.Substring(match.Index + match.Length);
                result.Add((text, $"{match.Value} → {replacement}"));
            }
        }
        return result;
    }
}

internal sealed record CheckLine(int Index, string Original, string Converted, string? Previous, string? Reference);

internal sealed record CheckResult(string Text, string? Note);

/// <summary>Optional checks with local models: Gervásio PT-PT choices after the rules; MiniLM meaning flags before and after the rules.</summary>
internal static class LocalChecks
{
    /// <summary>Gervásio must prefer a candidate by this many nats (about 7×) before it replaces the rules' choice.</summary>
    internal const double ChoiceMargin = 2.0;
    /// <summary>MiniLM similarity between the original and the converted line below which the meaning may have changed.</summary>
    internal const double MeaningThreshold = 0.70;
    /// <summary>With an English reference: flag when the converted line is this much further from the English than the original.</summary>
    internal const double ReferenceDrop = 0.10;
    /// <summary>
    /// The first review (before the rules) is the baseline: when the Brazilian line already matched the English at least this well,
    /// a much smaller drop after the rules is suspicious ("Tem uma ligação" → "Há uma ligação" for "You have a call").
    /// </summary>
    internal const double FaithfulBaseline = 0.60;
    internal const double SuspiciousDrop = 0.04;
    /// <summary>
    /// Every line with an English cue, changed or not: below this MiniLM similarity to the English the Brazilian translation itself
    /// may be wrong (an idiom or slang word translated literally). About 2% of the lines of a typical episode.
    /// </summary>
    internal const double TranslationThreshold = 0.45;

    private const string JudgeContext = "Legendas de uma série, em português europeu (de Portugal):\n";

    private static readonly Regex Words = new(@"[\p{L}\p{M}]+", RegexOptions.Compiled);

    /// <summary>
    /// A line worth comparing with its English cue: at least 3 words, and the cue is not much longer or shorter
    /// (then the cues were merged or split and a low score says nothing about the translation).
    /// </summary>
    internal static bool IsComparable(string text, string? english)
    {
        if (string.IsNullOrWhiteSpace(english)) return false;
        var pt = Words.Matches(ForScoring(text)).Count;
        var en = Words.Matches(ForScoring(english)).Count;
        return pt >= 3 && en >= pt * 0.5 && en <= pt * 2.0;
    }

    internal static string ForScoring(string text) =>
        Regex.Replace(Regex.Replace(text, @"</?[ibu]>|\{\\[^}]*\}", string.Empty), @"\s*\r?\n\s*", " ").Trim();

    internal static bool JudgeReady(EmbeddingSettings settings) =>
        settings.UseJudge && File.Exists(settings.RuntimePath) && File.Exists(settings.JudgeModelPath);

    internal static bool MeaningReady(EmbeddingSettings settings) =>
        settings.CheckMeaning && EmbeddingAssets.IsInstalled(settings);

    internal static async Task<string?> IdiomMeaningNoteAsync(CheckLine line, string converted,
        Func<string?, Task<float[]?>> embed)
    {
        var reading = IdiomHints.MeaningReference(line.Reference);
        if (reading == null) return null;
        var reference = await embed(reading);
        var original = await embed(line.Original);
        var final = converted == line.Original ? original : await embed(converted);
        if (reference == null || original == null || final == null) return null;
        var before = ReferenceAligner.Dot(original, reference);
        var after = ReferenceAligner.Dot(final, reference);
        if (before >= TranslationThreshold && after >= TranslationThreshold &&
            before - after < (before >= FaithfulBaseline ? SuspiciousDrop : ReferenceDrop))
            return null;
        return $"MiniLM (possible English idiom): similarity {before:0.00} before the rules, {after:0.00} after " +
            $"to the possible reading “{reading}” - check the context; literal use is also possible.";
    }

    internal static async Task<(Dictionary<int, CheckResult> Results, string Summary)> RunAsync(IReadOnlyList<CheckLine> lines,
        EmbeddingSettings settings, IProgress<string> progress, CancellationToken token)
    {
        settings = EmbeddingAssets.WithInstalledFiles(settings);
        var texts = lines.ToDictionary(l => l.Index, l => l.Converted);
        var notes = new Dictionary<int, List<string>>();
        void Note(int index, string note)
        {
            if (!notes.TryGetValue(index, out var list)) notes[index] = list = new List<string>();
            list.Add(note);
        }

        var summary = new List<string>();
        if (JudgeReady(settings))
        {
            var (chosen, device) = await JudgeAsync(lines, settings, texts, Note, progress, token);
            summary.Add($"Gervásio{device}: {chosen} choice(s).");
        }
        else if (settings.UseJudge)
        {
            summary.Add("Gervásio is not downloaded.");
        }

        if (MeaningReady(settings))
        {
            var flagged = await MeaningAsync(lines, settings, texts, Note, progress, token);
            summary.Add($"MiniLM: {flagged.Changed} line(s) whose meaning may have changed, {flagged.Translation} line(s) that may not match the English.");
        }

        var results = lines.Where(l => texts[l.Index] != l.Converted || notes.ContainsKey(l.Index))
            .ToDictionary(l => l.Index, l => new CheckResult(texts[l.Index],
                notes.TryGetValue(l.Index, out var list) ? string.Join("\n", list) : null));
        return (results, string.Join(" ", summary));
    }

    private static async Task<(int Chosen, string Device)> JudgeAsync(IReadOnlyList<CheckLine> lines, EmbeddingSettings settings,
        Dictionary<int, string> texts, Action<int, string> note, IProgress<string> progress, CancellationToken token)
    {
        var work = lines.Select(l => (Line: l, Candidates: JudgeAlternatives.Generate(l.Original, l.Converted)))
            .Where(w => w.Candidates.Count > 0)
            .ToList();
        if (work.Count == 0) return (0, string.Empty);

        await using var runtime = await EmbeddingRuntime.StartJudgeAsync(settings, progress, token);
        int chosen = 0, done = 0;
        var gate = new object();
        await Parallel.ForEachAsync(work, new ParallelOptions { MaxDegreeOfParallelism = EmbeddingRuntime.JudgeSlots, CancellationToken = token },
            async (item, ct) =>
            {
                var line = item.Line;
                var context = JudgeContext + (string.IsNullOrWhiteSpace(line.Previous) ? string.Empty : ForScoring(line.Previous) + "\n");
                var current = line.Converted;
                var currentScore = await runtime.ScoreAsync(context, ForScoring(current), ct);
                string? change = null;
                double best = currentScore;
                foreach (var (text, description) in item.Candidates)
                {
                    var score = await runtime.ScoreAsync(context, ForScoring(text), ct);
                    if (score > best)
                    {
                        best = score;
                        if (score >= currentScore + ChoiceMargin) (current, change) = (text, $"{description} (+{score - currentScore:0.0})");
                    }
                }

                lock (gate)
                {
                    if (change != null)
                    {
                        texts[line.Index] = current;
                        note(line.Index, $"Gervásio chose {change}.");
                        chosen++;
                    }
                    if (++done % 10 == 0) progress.Report($"Gervásio: {done}/{work.Count} lines...");
                }
            });
        var device = runtime.UsesGpu ? $" ({settings.Device}, {runtime.Layers})" : " (CPU)";
        return (chosen, device);
    }

    private static async Task<(int Changed, int Translation)> MeaningAsync(IReadOnlyList<CheckLine> lines, EmbeddingSettings settings, Dictionary<int, string> texts,
        Action<int, string> note, IProgress<string> progress, CancellationToken token)
    {
        var changed = lines.Where(l => l.Original != texts[l.Index]).ToList();
        // Stage 1 (before the rules): is the Brazilian line itself faithful to the English? Stage 2 (after): did the rules keep it so?
        var referenced = lines.Where(l => IsComparable(l.Original, l.Reference) || IsComparable(texts[l.Index], l.Reference)).ToList();
        var idiomatic = lines.Where(l => IdiomHints.MeaningReference(l.Reference) != null).ToList();
        if (changed.Count == 0 && referenced.Count == 0 && idiomatic.Count == 0) return (0, 0);
        await using var runtime = await EmbeddingRuntime.StartAsync(settings, progress, token);
        var cache = new Dictionary<string, float[]>(StringComparer.Ordinal);
        async Task<float[]?> Embed(string? text)
        {
            var clean = ReferenceAligner.Clean(text ?? string.Empty);
            if (!Regex.IsMatch(clean, @"[\p{L}\p{N}]")) return null;
            if (!cache.TryGetValue(clean, out var vector)) cache[clean] = vector = await runtime.EmbedAsync(clean, token);
            return vector;
        }

        var flagged = 0;
        var flaggedLines = new HashSet<int>();
        for (var i = 0; i < changed.Count; i++)
        {
            var line = changed[i];
            var original = await Embed(line.Original);
            var converted = await Embed(texts[line.Index]);
            if (original == null || converted == null) continue;
            var similarity = ReferenceAligner.Dot(original, converted);
            var english = await Embed(line.Reference);
            var baseline = english == null ? 0 : ReferenceAligner.Dot(original, english);
            var drop = english == null ? 0 : baseline - ReferenceAligner.Dot(converted, english);
            if (similarity < MeaningThreshold || drop >= (baseline >= FaithfulBaseline ? SuspiciousDrop : ReferenceDrop))
            {
                note(line.Index, english == null || similarity < MeaningThreshold
                    ? $"MiniLM: the meaning may have changed (similarity {similarity:0.00}) - check it."
                    : $"MiniLM: further from the English than the original ({baseline:0.00} before the rules, {baseline - drop:0.00} after) - check it.");
                flagged++;
                flaggedLines.Add(line.Index);
            }
            if (i % 50 == 0) progress.Report($"MiniLM: meaning check {i + 1}/{changed.Count}...");
        }

        var translation = 0;
        for (var i = 0; i < referenced.Count; i++)
        {
            var line = referenced[i];
            var english = await Embed(line.Reference);
            if (english == null) continue;
            var original = IsComparable(line.Original, line.Reference) ? await Embed(line.Original) : null;
            var converted = IsComparable(texts[line.Index], line.Reference) ? await Embed(texts[line.Index]) : null;
            var beforeRules = original == null ? (double?)null : ReferenceAligner.Dot(original, english);
            var afterRules = converted == null ? (double?)null : ReferenceAligner.Dot(converted, english);
            if (beforeRules < TranslationThreshold && flaggedLines.Add(line.Index))
            {
                note(line.Index, $"MiniLM (before the rules): the Brazilian line may not say what the English says (similarity {beforeRules:0.00}) - check the translation.");
                translation++;
            }
            else if (afterRules < TranslationThreshold && flaggedLines.Add(line.Index))
            {
                note(line.Index, beforeRules >= TranslationThreshold
                    ? $"MiniLM (after the rules): the original matched the English but this line no longer does (similarity {afterRules:0.00}) - a rule may have damaged it."
                    : $"MiniLM: this line may not say what the English says (similarity {afterRules:0.00}) - check the translation.");
                translation++;
            }
            if (i % 50 == 0) progress.Report($"MiniLM: translation check {i + 1}/{referenced.Count}...");
        }
        for (var i = 0; i < idiomatic.Count; i++)
        {
            token.ThrowIfCancellationRequested();
            var line = idiomatic[i];
            var idiomNote = await IdiomMeaningNoteAsync(line, texts[line.Index], Embed);
            if (idiomNote != null)
            {
                note(line.Index, idiomNote);
                if (flaggedLines.Add(line.Index)) translation++;
            }
            if (i % 50 == 0) progress.Report($"MiniLM: idiom check {i + 1}/{idiomatic.Count}...");
        }
        return (flagged, translation);
    }
}
