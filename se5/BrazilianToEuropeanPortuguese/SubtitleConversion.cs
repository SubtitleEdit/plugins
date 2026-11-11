using System.Text.RegularExpressions;
using SubtitleEdit.Plugins.Shared;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

/// <summary>Shared by preview and Batch convert; originals and timings are never mutated here.</summary>
public static class SubtitleConversion
{
    public static IReadOnlyList<CueConversion> Convert(IReadOnlyList<SrtBlock> blocks, ConversionOptions options,
        PortugueseLocalWordList? words = null, IReadOnlyList<string?>? english = null, ISet<int>? selected = null,
        IReadOnlyList<string?>? speakers = null, ISet<string>? unresolvedNameArticles = null)
    {
        var converter = new PortugueseConverter(options, words, unresolvedNameArticles);
        var result = new CueConversion[blocks.Count];
        for (var i = 0; i < blocks.Count; i++)
        {
            var hint = Hint(blocks, i, english, options.Address, speakers);
            if (i > 0 && Continues(blocks[i - 1], blocks[i]) &&
                converter.HasInformalTagEvidence(result[i - 1].Text) &&
                (speakers == null || string.IsNullOrWhiteSpace(speakers[i - 1]) || string.IsNullOrWhiteSpace(speakers[i]) ||
                 speakers[i - 1] == speakers[i]) &&
                (selected is not { Count: > 0 } || selected.Contains(i - 1) && selected.Contains(i)) &&
                result[i - 1].Hint is not { FormalAddress: true } && hint is not { FormalAddress: true })
                hint = (hint ?? new ReferenceHint(false, false, false, false)) with { InformalTagContinuation = true };
            if (i > 0 && RelativeAndModalGrammar.HasTopicContinuation(blocks[i - 1].Text, blocks[i].Text) &&
                Continues(blocks[i - 1], blocks[i]) &&
                (speakers == null || string.IsNullOrWhiteSpace(speakers[i - 1]) || string.IsNullOrWhiteSpace(speakers[i]) ||
                 speakers[i - 1] == speakers[i]) &&
                (selected is not { Count: > 0 } || selected.Contains(i - 1) && selected.Contains(i)) &&
                (result[i - 1].Hint?.FormalAddress == true) == (hint?.FormalAddress == true))
                hint = (hint ?? new ReferenceHint(false, false, false, false)) with { TopicRelativeContinuation = true };
            var rules = new List<RuleChange>();
            var conflict = options.Address == AddressMode.Automatic && DialogueRegister.ConflictingAddress(blocks[i].Text);
            var cueConverter = conflict ? new PortugueseConverter(options with { YouToTu = false }, words, unresolvedNameArticles) : converter;
            var text = selected is { Count: > 0 } && !selected.Contains(i)
                ? blocks[i].Text
                : cueConverter.Convert(blocks[i].Text, Previous(blocks, i), hint, rules);
            var note = conflict ? "Address review: formal and informal evidence conflict; automatic address conversion was withheld." :
                english?[i] == null && rules.Any(r => r.Id is "address" or "progressive-command") &&
                !Regex.IsMatch(blocks[i].Text, @"(?<!\p{L})(?:você|tu|senhor|senhora)\b", RegexOptions.IgnoreCase)
                    ? "Address review: subject/register inferred from context without an aligned English cue; verify the addressee." : null;
            if (rules.Any(r => r.Id == "arrival-group-apology"))
                note = string.Join("\n", new[] { note,
                    "Address review: plural apology inferred from a named arrival and a nearby waiting group; verify the recipients." }
                    .Where(n => n != null));
            result[i] = new CueConversion(text, hint, rules.Distinct().ToArray(), -1, note);
        }

        if (!options.SentenceContext || !options.RuleEnabled("sentence-context")) return result;
        for (var first = 0; first < blocks.Count; first++)
        {
            var last = first;
            while (last + 1 < blocks.Count && last - first < 2 && Continues(blocks[last], blocks[last + 1]) &&
                   (speakers == null || string.IsNullOrWhiteSpace(speakers[last]) || string.IsNullOrWhiteSpace(speakers[last + 1]) ||
                    speakers[last] == speakers[last + 1]) &&
                   (selected is not { Count: > 0 } || selected.Contains(last) && selected.Contains(last + 1)) &&
                   (result[last].Hint?.FormalAddress == true) == (result[last + 1].Hint?.FormalAddress == true))
                last++;
            if (last == first) continue;
            if (Enumerable.Range(first, last - first + 1).Any(i => options.Address == AddressMode.Automatic &&
                DialogueRegister.ConflictingAddress(blocks[i].Text))) continue;
            var originals = blocks.Skip(first).Take(last - first + 1).Select(b => b.Text).ToArray();
            var baselines = result.Skip(first).Take(last - first + 1).Select(r => r.Text).ToArray();
            var joined = string.Join(" ", baselines.Select(text => Regex.Replace(text, @"\s+", " ")));
            if (joined.Length > 1200)
            {
                for (var index = first; index <= last; index++)
                    result[index] = result[index] with { ContextNote = "Sentence context skipped: continuation exceeds 1200 characters." };
                first = last;
                continue;
            }
            var reference = english == null ? null : string.Join(" ",
                english.Skip(first).Take(last - first + 1).Where(t => t != null).Distinct());
            var meaningSentence = new SentenceMeaning(first, last, string.Join(" ", originals),
                english == null || english.Skip(first).Take(last - first + 1).Any(string.IsNullOrWhiteSpace)
                    ? null : reference);
            for (var index = first; index <= last; index++)
                result[index] = result[index] with { MeaningSentence = meaningSentence };
            var combinedHint = reference == null ? result[first].Hint : ReferenceHint.FromText(reference);
            if (combinedHint != null)
                combinedHint = combinedHint with
                {
                    FormalAddress = result[first].Hint?.FormalAddress == true,
                    ForceInformal = options.Address == AddressMode.Informal,
                    IgnorePreviousAddress = result[first].Hint?.IgnorePreviousAddress == true,
                };
            var rules = new List<RuleChange>();
            var converted = converter.ConvertContinuation(joined, Previous(blocks, first), combinedHint, rules);
            var sentenceReview = GrammarReview.Note(converted);
            if (converted == joined)
            {
                if (sentenceReview != null)
                    for (var index = first; index <= last; index++)
                        result[index] = result[index] with
                        {
                            ContextNote = string.Join("\n", new[] { result[index].ContextNote,
                                $"Sentence review across cues {first + 1}-{last + 1}: {sentenceReview}" }.Where(n => n != null)),
                        };
                first = last;
                continue;
            }
            var mapped = MapWords(baselines, converted);
            if (mapped == null)
            {
                for (var index = first; index <= last; index++)
                    result[index] = result[index] with { ContextNote = "Sentence context skipped: a safe non-empty cue mapping was not possible." };
                first = last;
                continue;
            }
            var mappedTexts = mapped.Select((text, index) => PortugueseConverter.RestoreLineBreaks(originals[index], text)).ToArray();
            var changedSentence = mappedTexts.Where((text, index) => text != result[first + index].Text).Any();
            var anyChange = mappedTexts.Where((value, offset) => value != originals[offset]).Any();
            for (var index = first; index <= last; index++)
            {
                var text = mappedTexts[index - first];
                var note = anyChange ? $"Sentence context: cues {first + 1}-{last + 1}; accept/reject these changes together." : null;
                if (sentenceReview != null) note = note == null ? sentenceReview : note + "\n" + sentenceReview;
                result[index] = new CueConversion(text, result[index].Hint,
                    result[index].Rules.Concat(changedSentence
                        ? rules.Append(new RuleChange("sentence-context", "Complete sentence across adjacent cues", "Context-sensitive")) : rules)
                    .Distinct().ToArray(), anyChange ? first : -1, note) { MeaningSentence = meaningSentence };
            }
            first = last;
        }
        return result;
    }

    internal static IReadOnlyList<string?>? Speakers(PluginRequest request, IReadOnlyList<SrtBlock> blocks, out string? note)
    {
        note = null;
        var paragraphs = request.Subtitle.Paragraphs;
        if (paragraphs == null || !paragraphs.Any(p => !string.IsNullOrWhiteSpace(p.Actor))) return null;
        if (paragraphs.Count != blocks.Count || paragraphs.Where((p, i) =>
            Math.Abs(p.StartMs - blocks[i].StartMs) > 1 || Math.Abs(p.EndMs - blocks[i].EndMs) > 1).Any())
        {
            note = "Speaker metadata ignored: paragraphs do not match the SubRip cue count/timings.";
            return null;
        }
        return paragraphs.Select(p => string.IsNullOrWhiteSpace(p.Actor) ? null : p.Actor.Trim()).ToArray();
    }

    private static ReferenceHint? Hint(IReadOnlyList<SrtBlock> blocks, int index, IReadOnlyList<string?>? english, AddressMode mode,
        IReadOnlyList<string?>? speakers)
    {
        var hint = ReferenceHint.FromContext(blocks, index, english, speakers);
        if (NamedArrivalToWaitingGroup(blocks, index, english))
            hint = (hint ?? new ReferenceHint(false, false, false, false)) with { GroupArrivalApology = true };
        if (index > 0 && blocks[index].StartMs >= blocks[index - 1].StartMs &&
            blocks[index].StartMs - blocks[index - 1].EndMs <= 1500 &&
            !DialogueRegister.DifferentSpeaker(blocks[index - 1].Text, blocks[index].Text) &&
            (speakers == null || string.IsNullOrWhiteSpace(speakers[index - 1]) || string.IsNullOrWhiteSpace(speakers[index]) ||
             speakers[index - 1] == speakers[index]) &&
            !Regex.IsMatch(blocks[index - 1].Text + blocks[index].Text, @"(?m)^\s*[-–—]|\p{Lu}{2,}:") &&
            !Regex.IsMatch(english?[index] ?? "", @"\b(?:he|she|they|we|I)\b", RegexOptions.IgnoreCase) &&
            ExplanationConstructions.HasGoverningAddressee(blocks[index - 1].Text, hint))
            hint = (hint ?? new ReferenceHint(false, false, false, false)) with { AddresseeContinuation = true };
        if (index > 0 && (DialogueRegister.DifferentSpeaker(blocks[index - 1].Text, blocks[index].Text) ||
            speakers != null && !string.IsNullOrWhiteSpace(speakers[index - 1]) && !string.IsNullOrWhiteSpace(speakers[index]) &&
            speakers[index - 1] != speakers[index]))
            hint = (hint ?? new ReferenceHint(false, false, false, false)) with { IgnorePreviousAddress = true };
        return mode switch
        {
            AddressMode.Formal => (hint ?? new ReferenceHint(false, false, false, false)) with { FormalAddress = true },
            AddressMode.Informal => (hint ?? new ReferenceHint(false, false, false, false)) with { FormalAddress = false, ForceInformal = true },
            _ => hint,
        };
    }

    private static bool NamedArrivalToWaitingGroup(IReadOnlyList<SrtBlock> blocks, int index, IReadOnlyList<string?>? english)
    {
        const RegexOptions ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
        if (english == null || index < 2) return false;
        var arrival = Regex.Match(english[index] ?? "",
            @"\b(?<name>(?-i:[A-Z][A-Z'-]+)):\s*(?:hey|hi|hello)?[!. ,]*sorry[!. ]*$", ci);
        if (!arrival.Success) return false;
        var name = Regex.Escape(arrival.Groups["name"].Value);
        var previous = Regex.Replace(blocks[index - 1].Text, @"<[^>]*>", "");
        if (!Regex.IsMatch(previous, @"\balguém\s+(?:sabe|soube|ouviu|falou|teve)\b", ci)) return false;
        for (var earlier = index - 2; earlier >= Math.Max(0, index - 3); earlier--)
        {
            if (blocks[index].StartMs < blocks[earlier].EndMs ||
                blocks[index].StartMs - blocks[earlier].EndMs > 15000) continue;
            var waiting = Regex.Replace(blocks[earlier].Text, @"<[^>]*>", "");
            var collective = Regex.Match(waiting,
                @"\b(?:(?:temos|tínhamos|vamos|devemos|precisamos)\s+(?:(?:de|que)\s+)?esperar|esperamos|esperávamos|esperaremos)" +
                @"\s+(?:(?:por|pelo|pela|o|a)\s+)?" + name + @"\b", ci);
            if (collective.Success &&
                !Regex.IsMatch(waiting[..collective.Index], @"\b(?:não|nunca|jamais|nem)\b[^.!?]*$", ci) &&
                Regex.IsMatch(english[earlier] ?? "", @"\bwe\s+(?:(?:have|need)\s+to\s+|(?:are|were)\s+(?:still\s+)?|" +
                    @"(?:will|must|should)\s+)?wait(?:ing)?\b[^.!?]{0,30}\b" + name + @"\b", ci))
                return true;
        }
        return false;
    }

    private static bool Continues(SrtBlock left, SrtBlock right) =>
        right.StartMs >= left.StartMs && right.StartMs - left.EndMs <= 1500 &&
        !DialogueRegister.DifferentSpeaker(left.Text, right.Text) &&
        !Regex.IsMatch(left.Text + right.Text, @"[<>{}]|(?m)^\s*[-–—]|\p{Lu}{2,}:") &&
        !Regex.IsMatch(left.Text.TrimEnd(), @"[.!?…][""»”']*$") &&
        Regex.IsMatch(right.Text, @"^\s*\p{Ll}");

    private static string? Previous(IReadOnlyList<SrtBlock> blocks, int index) =>
        index > 0 && blocks[index].StartMs >= blocks[index - 1].StartMs &&
        blocks[index].StartMs - blocks[index - 1].EndMs <= 5000
            ? blocks[index - 1].Text : null;

    // Monotonic word alignment retains each cue's place in the sentence, including insertions/deletions.
    private static string[]? MapWords(string[] originals, string converted)
    {
        var lengths = originals.Select(t => Regex.Matches(t, @"\S+").Count).ToArray();
        var source = originals.SelectMany(t => Regex.Matches(t, @"\S+").Select(m => m.Value)).ToArray();
        var target = Regex.Matches(converted, @"\S+").Select(m => m.Value).ToArray();
        if (source.Length > 160 || target.Length > 160 || source.Length == 0 || target.Length == 0) return null;
        var cost = new int[source.Length + 1, target.Length + 1];
        for (var i = 0; i <= source.Length; i++) cost[i, 0] = i;
        for (var j = 0; j <= target.Length; j++) cost[0, j] = j;
        for (var i = 1; i <= source.Length; i++)
            for (var j = 1; j <= target.Length; j++)
                cost[i, j] = Math.Min(cost[i - 1, j - 1] + (source[i - 1] == target[j - 1] ? 0 : 1),
                    Math.Min(cost[i - 1, j] + 1, cost[i, j - 1] + 1));
        var origins = new int[target.Length];
        var row = source.Length;
        var column = target.Length;
        while (row > 0 || column > 0)
        {
            if (row > 0 && column > 0 && cost[row, column] ==
                cost[row - 1, column - 1] + (source[row - 1] == target[column - 1] ? 0 : 1))
            {
                origins[--column] = --row;
            }
            else if (row > 0 && cost[row, column] == cost[row - 1, column] + 1) row--;
            else origins[--column] = Math.Max(0, row - 1);
        }
        var output = new string[originals.Length];
        var boundary = 0;
        var start = 0;
        for (var cue = 0; cue < originals.Length; cue++)
        {
            boundary += lengths[cue];
            var end = start;
            while (end < target.Length && origins[end] < boundary) end++;
            if (end == start) return null;
            output[cue] = string.Join(" ", target[start..end]);
            start = end;
        }
        return start == target.Length ? output : null;
    }
}
