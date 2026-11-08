using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal enum ConflictSense { Unknown, Combat, Argument, Brawl, Dispute, Conversation, Noise }

internal static class ConflictVocabulary
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    private static readonly Regex Boundaries = new(@"[.!?…;]|(?:^|\n)\s*[-–—]", RegexOptions.Compiled);
    private static readonly Regex ConflictWords = new(
        @"(?<![\p{L}\p{M}-])(?:brigas?|bate-bocas?)(?![\p{L}\p{M}-])", Ci);
    private static readonly Regex Combat = new(
        @"\b(?:combat|battles?|battling|struggles?|struggling)\b|" +
        @"\b(?:street|fist|physical|knife|gun)\s+fights?\b|" +
        @"\b(?:fights?|fighting|fought)\b[^.!?;]{0,60}\b(?:fists|punches|punching|kicks|kicking|knives|stabbing)\b|" +
        @"\b(?:fights?|fighting)\s+(?:broke|breaks?|breaking)\s+out\b|" +
        @"\b(?:fights?|fighting|fought)\s+(?:for|against|back)\b", Ci);
    private static readonly Regex Argument = new(
        @"\b(?:argue|argues|argued|arguing|arguments?|quarrels?|quarrelled|quarreling|quarrelling|" +
        @"verbal\s+fight|fight(?:s|ing)?\s+about|battle\s+of\s+words|domestic\s+dispute|" +
        @"fight(?:s|ing)?\s+like\s+(?:an?\s+)?(?:old\s+)?(?:married\s+)?couple)\b", Ci);
    private static readonly Regex Brawl = new(
        @"\b(?:brawls?|brawling|fisticuffs)\b|" +
        @"\b(?:violent|physical|street)\s+scuffles?\b|" +
        @"\b(?:scuffles?|scuffling)\b[^.!?;]{0,40}\b(?:fists|punches|kicks|knives|stabbing)\b", Ci);
    private static readonly Regex Dispute = new(@"\b(?:disputes?|disputed|disputing|contests?|contested|contesting)\b", Ci);
    private static readonly Regex Conversation = new(@"\b(?:chat|chats|chatting|chatted|chitchat|small\s+talk)\b", Ci);
    private static readonly Regex Noise = new(@"\b(?:hubbub|uproar|clamour|clamor)\b", Ci);
    private static readonly Regex VerbalFight = new(
        @"\b(?:verbal\s+fights?|fight(?:s|ing)?\s+about|battle\s+of\s+words|" +
        @"fight(?:s|ing)?\s+like\s+(?:an?\s+)?(?:old\s+)?(?:married\s+)?couple)\b", Ci);
    private static readonly Regex PhysicalComplement = new(
        @"^\s+(?:de\s+(?:rua|socos|punhos|facas?)|a\s+socos|corpo\s+a\s+corpo|com\s+(?:socos|pontapés))\b", Ci);
    private static readonly Regex PurposeComplement = new(
        @"^\s+(?:pelo|pela|por|contra)\s+(?:(?:o|a|os|as|um|uma|seus|suas|teus|tuas)\s+)?" +
        @"(?:título|sobrevivência|vida|liberdade|direitos|justiça|igualdade|cancro|câncer|doença)\b", Ci);
    private static readonly Regex VerbalComplement = new(
        @"^\s+(?:verbal|verbais|de\s+palavras|por\s+(?:dinheiro|ciúmes)|sobre\b)", Ci);

    private static List<(int Start, int End)> Clauses(string text)
    {
        var clauses = new List<(int Start, int End)>();
        var start = 0;
        foreach (Match boundary in Boundaries.Matches(text))
        {
            if (!string.IsNullOrWhiteSpace(text[start..boundary.Index])) clauses.Add((start, boundary.Index));
            start = boundary.Index + boundary.Length;
        }
        if (!string.IsNullOrWhiteSpace(text[start..])) clauses.Add((start, text.Length));
        return clauses;
    }

    private static ConflictSense EnglishSense(string text, bool noun)
    {
        var argument = Argument.IsMatch(text);
        var brawl = Brawl.IsMatch(text);
        var dispute = Dispute.IsMatch(Regex.Replace(text, @"\bdomestic\s+dispute\b", "", Ci));
        var conversation = noun && Conversation.IsMatch(text);
        var noise = noun && Noise.IsMatch(text);
        var nonverbal = VerbalFight.Replace(text, "");
        var independentCombat = Combat.IsMatch(nonverbal);
        var independentFight = Regex.IsMatch(nonverbal, @"\b(?:fights?|fighting|fought)\b", Ci);
        // "fight about" is itself verbal evidence; mixed independent senses are not resolved.
        if (new[] { argument, brawl, dispute, conversation, noise }.Count(value => value) > 1)
            return ConflictSense.Unknown;
        if (independentCombat && (argument || brawl || dispute || conversation || noise))
            return ConflictSense.Unknown;
        if (independentFight && (argument || dispute || conversation || noise))
            return ConflictSense.Unknown;
        if (argument) return ConflictSense.Argument;
        if (brawl) return ConflictSense.Brawl;
        if (dispute) return ConflictSense.Dispute;
        if (conversation) return ConflictSense.Conversation;
        if (noise) return ConflictSense.Noise;
        return Combat.IsMatch(text) ? ConflictSense.Combat : ConflictSense.Unknown;
    }

    internal static bool IsNominalVerbMatch(string text, Group word)
    {
        if (!word.Value.Equals("briga", StringComparison.OrdinalIgnoreCase) &&
            !word.Value.Equals("brigas", StringComparison.OrdinalIgnoreCase)) return false;
        var clauses = Clauses(text);
        var clause = clauses.First(c => c.Start <= word.Index && word.Index < c.End);
        var prefix = text[clause.Start..word.Index];
        if (Regex.IsMatch(prefix, @"\p{L}", Ci)) return false;
        var following = Regex.Match(text[(word.Index + word.Length)..clause.End], @"^\s+(?<word>\p{L}+)");
        return following.Success && (PortugueseVerbs.IsKnownVerbForm(following.Groups["word"].Value) ||
            Regex.IsMatch(following.Groups["word"].Value,
                @"^(?:são|eram|foram|serão|estão|estavam|violentas?|feias?|constantes?|verbais|verbal)$", Ci));
    }

    internal static ConflictSense Sense(string text, int offset, int length, ReferenceHint? reference, bool noun)
    {
        var sourceClauses = Clauses(text);
        var index = sourceClauses.FindIndex(c => c.Start <= offset && offset < c.End);
        if (index < 0) return ConflictSense.Unknown;
        var clause = sourceClauses[index];
        var following = text[clause.Start..clause.End];
        var after = text[(offset + length)..clause.End];
        var local = PhysicalComplement.IsMatch(after) || PurposeComplement.IsMatch(after) ? ConflictSense.Combat :
            VerbalComplement.IsMatch(after) ? ConflictSense.Argument : ConflictSense.Unknown;
        var english = reference?.EnglishText;
        if (string.IsNullOrWhiteSpace(english)) return local;
        var englishClauses = Clauses(english);
        if (sourceClauses.Count == englishClauses.Count)
        {
            var aligned = englishClauses[index];
            english = english[aligned.Start..aligned.End];
        }
        var sense = EnglishSense(english, noun);
        if (local != ConflictSense.Unknown && sense != ConflictSense.Unknown && local != sense)
            return ConflictSense.Unknown;
        // Two bare occurrences may have different senses despite cue-wide reference evidence.
        if (local == ConflictSense.Unknown && noun && ConflictWords.Matches(following).Count > 1)
            return ConflictSense.Unknown;
        return local != ConflictSense.Unknown ? local : sense;
    }

    internal static string NounTarget(string text, Group word, ReferenceHint? reference, string fallback)
    {
        var sense = Sense(text, word.Index, word.Length, reference, noun: true);
        var verbalNoun = word.Value.StartsWith("bate", StringComparison.OrdinalIgnoreCase);
        if (verbalNoun && sense is ConflictSense.Combat or ConflictSense.Brawl or ConflictSense.Dispute)
            return fallback;
        var target = sense switch
        {
            ConflictSense.Combat => "luta",
            ConflictSense.Argument => "discussão",
            ConflictSense.Brawl => "rixa",
            ConflictSense.Dispute => "disputa",
            ConflictSense.Conversation => verbalNoun ? "conversa" : fallback,
            ConflictSense.Noise => verbalNoun ? "algazarra" : fallback,
            _ => fallback,
        };
        if (!word.Value.EndsWith('s') || target == fallback) return target;
        return target == "discussão" ? "discussões" : target + "s";
    }

    internal static string VerbInfinitive(string text, Match match, ReferenceHint? reference, string fallback) =>
        Sense(text, match.Index, match.Length, reference, noun: false) switch
        {
            ConflictSense.Combat or ConflictSense.Brawl => "lutar",
            _ => fallback,
        };
}
