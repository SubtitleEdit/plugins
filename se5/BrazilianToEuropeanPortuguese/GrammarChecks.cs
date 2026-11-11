using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal static class GrammarChecks
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    private static readonly Regex Markup = new(@"<[^>]*>|\{[^}]*\}|[\uE000-\uF8FF]", RegexOptions.Compiled);
    private static readonly Regex UnsafeAddress = new(@"[""'‘’“”«»]|^\s*[-–—]|\n\s*[-–—]", RegexOptions.Compiled);
    private static readonly Regex SubjectAgreement = new(
        @"(?:^|[.!?;]\s*)(?<subject>ele|ela|eles|elas)(?<gap>\s+(?:(?:não|nunca|já|ainda|também)\s+){0,2})(?<verb>tem|têm|vem|vêm)\b", Ci);

    internal static async Task<string> RunAsync(IReadOnlyList<CheckLine> lines, ConversionOptions options,
        PortugueseLocalWordList? words, Dictionary<int, string> texts, Action<int, string> note,
        Func<string, CancellationToken, Task<IReadOnlyList<GrammarMatch>>> check,
        Func<string, CancellationToken, Task<float[]>>? embed, IProgress<string> progress, CancellationToken token)
    {
        if (!options.GrammarCheck || !options.RuleEnabled("languagetool")) return string.Empty;
        var groups = Groups(lines);
        var changed = 0;
        var flagged = 0;
        for (var n = 0; n < groups.Count; n++)
        {
            token.ThrowIfCancellationRequested();
            if (n % 10 == 0) progress.Report($"LanguageTool: {n + 1}/{groups.Count} sentence group(s)...");
            var group = groups[n];
            var parts = group.Select(l => texts[l.Index]).ToArray();
            var baseline = string.Join("\n", parts);
            if (baseline.Length > 1200)
            {
                foreach (var line in group) note(line.Index, "LanguageTool: skipped context over 1200 characters.");
                continue;
            }
            var masked = Mask(baseline, words);
            var matches = (await check(masked, token)).Where(m => Visible(masked, baseline, m)).ToArray();
            if (matches.Length == 0) continue;
            flagged += group.Count;
            var english = group.Any(l => string.IsNullOrWhiteSpace(l.Reference))
                ? null : string.Join(" ", group.Select(l => l.Reference));
            var allowed = matches.SelectMany(m => m.Replacements.Take(5).Select(r =>
                (Match: m, Text: Suggestion(baseline, m, r))))
                .Where(c => masked == baseline && Eligible(baseline, c.Text, english, options) &&
                    !matches.Any(m => m != c.Match && Overlaps(m, c.Match)) &&
                    ChangedWithinCue(parts, baseline, c.Text)).Distinct().ToArray();
            string? accepted = null;
            GrammarMatch? acceptedMatch = null;
            var reason = !options.GrammarAutoFix ? "review only" :
                allowed.Length != 1 ? "not a unique allowlisted correction" :
                embed == null ? "automatic correction requires enabled, installed MiniLM" : "";
            if (options.GrammarAutoFix && allowed.Length == 1 && embed != null)
            {
                var candidate = allowed[0];
                if (!await MeaningSafeAsync(baseline, candidate.Text, english!, embed, token))
                    reason = "MiniLM meaning safeguard rejected the candidate";
                else
                {
                    var rechecked = (await check(Mask(candidate.Text, words), token)).ToArray();
                    if (!ResolvedWithoutNewDiagnostics(baseline, candidate.Text, matches, rechecked, candidate.Match.Rule))
                        reason = "grammar recheck did not resolve the diagnostic without new findings";
                    else
                    {
                        accepted = candidate.Text;
                        acceptedMatch = candidate.Match;
                        reason = "not automatically changed";
                        changed++;
                    }
                }
            }
            if (accepted != null)
            {
                if (accepted.Length != baseline.Length)
                    throw new InvalidDataException("LanguageTool: correction could not preserve cue and line boundaries.");
                // Each cue may itself contain newlines; the allowed edits preserve all whitespace.
                var position = 0;
                for (var i = 0; i < group.Count; i++)
                {
                    texts[group[i].Index] = accepted.Substring(position, parts[i].Length);
                    position += parts[i].Length + 1;
                }
            }
            foreach (var match in matches)
            {
                var suggestions = match.Replacements.Length == 0 ? "no replacement supplied" :
                    string.Join(" / ", match.Replacements.Take(5).Select(r => r.Replace("\r", " ").Replace("\n", " ")));
                var status = ReferenceEquals(acceptedMatch, match) ? "corrected (allowlisted agreement; English + MiniLM + recheck)" :
                    $"review ({reason})";
                var description = $"LanguageTool [{match.Rule}]: {match.Message} Suggested: {suggestions}; {status}.";
                foreach (var line in group) note(line.Index, description);
            }
            progress.Report($"LanguageTool: {n + 1}/{groups.Count} sentence group(s)...");
        }
        return $"LanguageTool pt-PT: {flagged} cue(s) with findings, {changed} allowlisted correction(s).";
    }

    private static List<List<CheckLine>> Groups(IReadOnlyList<CheckLine> lines)
    {
        var byIndex = lines.ToDictionary(l => l.Index);
        var used = new HashSet<int>();
        var groups = new List<List<CheckLine>>();
        foreach (var line in lines.OrderBy(l => l.Index))
        {
            if (!used.Add(line.Index)) continue;
            var group = new List<CheckLine> { line };
            if (line.MeaningSentence is { } sentence && sentence.First == line.Index &&
                sentence.Last - sentence.First is > 0 and < 3 &&
                Enumerable.Range(sentence.First, sentence.Last - sentence.First + 1)
                    .All(i => byIndex.TryGetValue(i, out var peer) && peer.MeaningSentence == sentence))
                for (var i = line.Index + 1; i <= sentence.Last; i++)
                {
                    used.Add(i);
                    group.Add(byIndex[i]);
                }
            groups.Add(group);
        }
        return groups;
    }

    internal static string Mask(string text, PortugueseLocalWordList? words)
    {
        var masked = Markup.Replace(text, m => new string(' ', m.Length));
        foreach (var phrase in (words?.Ignored ?? Enumerable.Empty<string>())
            .Concat(words?.Words.Select(w => w.Pt) ?? Enumerable.Empty<string>()).Where(p => p.Length > 0))
            masked = Regex.Replace(masked, @"(?<![\p{L}\p{M}])" + Regex.Escape(phrase) + @"(?![\p{L}\p{M}])",
                m => new string(' ', m.Length), Ci);
        return masked;
    }

    private static bool Visible(string masked, string original, GrammarMatch match)
    {
        if (match.Length < 0 || match.Offset < 0 || match.Offset > original.Length - match.Length) return false;
        if (match.Length > 0)
            return original.AsSpan(match.Offset, match.Length).SequenceEqual(masked.AsSpan(match.Offset, match.Length));
        return (match.Offset == 0 || original[match.Offset - 1] == masked[match.Offset - 1]) &&
            (match.Offset == original.Length || original[match.Offset] == masked[match.Offset]);
    }

    private static bool Overlaps(GrammarMatch a, GrammarMatch b) =>
        a.Offset < b.Offset + b.Length && b.Offset < a.Offset + a.Length;

    internal static string Suggestion(string baseline, GrammarMatch match, string replacement)
    {
        var original = baseline.Substring(match.Offset, match.Length);
        var words = Regex.Matches(replacement, @"\S+").Select(m => m.Value).ToArray();
        if (words.Length == Regex.Matches(original, @"\S+").Count)
        {
            var index = 0;
            replacement = Regex.Replace(original, @"\S+", _ => words[index++]);
        }
        return baseline[..match.Offset] + replacement + baseline[(match.Offset + match.Length)..];
    }

    private static bool ChangedWithinCue(string[] parts, string before, string after)
    {
        if (before.Length != after.Length) return false;
        var changed = Enumerable.Range(0, before.Length).Where(i => before[i] != after[i]).ToArray();
        if (changed.Length == 0) return false;
        var start = 0;
        foreach (var part in parts)
        {
            if (changed[0] >= start && changed[^1] < start + part.Length) return true;
            start += part.Length + 1;
        }
        return false;
    }

    internal static bool Eligible(string before, string after, string? english, ConversionOptions options)
    {
        if (!options.YouToTu || !options.RuleEnabled("address") ||
            string.IsNullOrWhiteSpace(english) || !LocalChecks.IsComparable(before, english) ||
            before.Length != after.Length || Markup.IsMatch(before) || UnsafeAddress.IsMatch(before) ||
            DialogueRegister.ConflictingAddress(before)) return false;
        foreach (Match match in SubjectAgreement.Matches(before))
        {
            var subject = match.Groups["subject"].Value.ToLowerInvariant();
            var verb = match.Groups["verb"];
            var plural = subject is "eles" or "elas";
            var replacement = verb.Value.ToLowerInvariant() switch
            {
                "tem" when plural => "têm", "vem" when plural => "vêm",
                "têm" when !plural => "tem", "vêm" when !plural => "vem", _ => null,
            };
            if (replacement == null) continue;
            var expected = before[..verb.Index] + CaseHelper.MatchCase(verb.Value, replacement) + before[(verb.Index + verb.Length)..];
            var englishSubject = subject switch { "ele" => "he", "ela" => "she", _ => "they" };
            if (after == expected && Regex.IsMatch(english, @"^\s*" + englishSubject + @"\b", Ci))
                return true;
        }
        return false;
    }

    internal static async Task<bool> MeaningSafeAsync(string before, string after, string english,
        Func<string, CancellationToken, Task<float[]>> embed, CancellationToken token)
    {
        var source = await embed(LocalChecks.ForScoring(before), token);
        var candidate = await embed(LocalChecks.ForScoring(after), token);
        var reference = await embed(LocalChecks.ForScoring(english), token);
        var similarity = ReferenceAligner.Dot(source, candidate);
        var originalEnglish = ReferenceAligner.Dot(source, reference);
        var finalEnglish = ReferenceAligner.Dot(candidate, reference);
        return double.IsFinite(similarity) && double.IsFinite(originalEnglish) && double.IsFinite(finalEnglish) &&
            similarity >= LocalChecks.MeaningThreshold &&
            originalEnglish >= LocalChecks.TranslationThreshold && finalEnglish >= LocalChecks.TranslationThreshold &&
            originalEnglish - finalEnglish < (originalEnglish >= LocalChecks.FaithfulBaseline
                ? LocalChecks.SuspiciousDrop : LocalChecks.ReferenceDrop);
    }

    private static bool ResolvedWithoutNewDiagnostics(string original, string candidate,
        IReadOnlyList<GrammarMatch> before, IReadOnlyList<GrammarMatch> after, string rule)
    {
        static (string Rule, string Message, string Text) Key(GrammarMatch m, string text) =>
            (m.Rule, m.Message, text.Substring(m.Offset, m.Length));
        var counts = before.GroupBy(m => Key(m, original)).ToDictionary(g => g.Key, g => g.Count());
        return after.Count(m => m.Rule == rule) < before.Count(m => m.Rule == rule) &&
            after.GroupBy(m => Key(m, candidate)).All(g => counts.TryGetValue(g.Key, out var count) && g.Count() <= count);
    }
}
