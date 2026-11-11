using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal static class RelativeAndModalGrammar
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    private const string TopicAntecedent =
        @"algo|nada|(?:alguma|uma|esta|essa|aquela)\s+coisa|" +
        @"(?:o|um|este|esse|aquele)\s+(?:assunto|tema|problema)|" +
        @"(?:a|uma|esta|essa|aquela)\s+(?:questão|situação|ideia|proposta)";
    private static readonly Regex PreviousTopic = new(
        @"(?<![\p{L}\p{M}-])(?<ante>" + TopicAntecedent + @")\s*$", Ci);
    private static readonly Regex TopicRelative = new(
        @"(?<![\p{L}\p{M}-])(?<ante>" + TopicAntecedent + @")(?<space>\s+)" +
        @"(?<relative>sobre\s+o\s+que)(?<tail>\s+(?:(?:eu|tu|ele|ela|nós|eles|elas|você|vocês|" +
        @"não|nunca|já|ainda|também|me|te|lhe|lhes|nos|vos|se|de|" +
        @"queria|querias|queríamos|queriam|quero|queres|quer|queremos|querem|" +
        @"esperava|esperavas|esperávamos|esperavam|espero|esperas|espera|esperamos|esperam|" +
        @"gostaria|gostarias|gostaríamos|gostariam|podia|podias|podíamos|podiam|" +
        @"posso|podes|pode|podemos|podem|possa|possas|possamos|possam|" +
        @"tenho|tens|tem|temos|têm|tinha|tinhas|tínhamos|tinham|ia|ias|íamos|iam)\s+){0,6}" +
        @"(?<verb>fal\p{L}+))(?![\p{L}\p{M}\d_-])", Ci);
    private static readonly HashSet<string> FalarForms = BuildFalarForms();
    private static readonly Regex SubjectVerb = new(
        @"(?<pre>^|[.!?;]\s*|\b(?:que|mas)\s+)[\s""“«'\uE000-\uF8FF–—-]*(?:(?:e|então)\s+)?" +
        @"(?<subject>eu|tu|você|vocês|ele|ela|nós|eles|elas)(?<space>\s+)" +
        @"(?<adverbs>(?:(?:não|nunca|jamais|já|ainda|sempre|também|só|apenas|provavelmente|" +
        @"me|te|se|nos|vos|lhe|lhes|o|a|os|as)\s+){0,3})(?<verb>\p{L}+)(?![\p{L}\p{M}\d_-])", Ci);
    private static readonly Regex EnglishConditional = new(
        @"(?:^|[.!?;]\s*)(?:(?:yes|yeah|no|well|and|but|so|then)[,.]?\s+){0,3}" +
        @"(?<subject>I|you|he|she|it|we|they)(?:\s+would(?<negative>n't)?|(?<contracted>['’]d))" +
        @"\s+(?:(?<negative>not|never)\s+)?(?:(?:always|still|really|also|just|probably)\s+){0,2}" +
        @"(?<verb>[a-z]+)\b", Ci);
    private static readonly Regex EnglishModal = new(
        @"\b(?:would|wouldn't|will|won't|could|couldn't|should|shouldn't|might|must|can|can't)\b|['’](?:d|ll)\b", Ci);
    private static readonly HashSet<string> EnglishBaseVerbs = new(StringComparer.OrdinalIgnoreCase)
    {
        "be", "have", "do", "go", "come", "bring", "take", "give", "make", "get", "keep",
        "talk", "speak", "say", "tell", "mention", "ask", "answer", "call", "help", "work",
        "leave", "stay", "wait", "try", "need", "want", "like", "love", "hate", "prefer",
        "know", "think", "believe", "feel", "see", "hear", "find", "look", "meet", "remember",
        "forget", "understand", "buy", "sell", "pay", "eat", "drink", "sleep", "drive", "write",
        "send", "open", "close", "start", "stop", "continue", "agree", "accept", "refuse", "return",
    };

    private static HashSet<string> BuildFalarForms()
    {
        if (!VerbConjugator.TryConjugate("falar", out var forms))
            throw new InvalidOperationException("Cannot conjugate falar.");
        return new HashSet<string>(forms, StringComparer.OrdinalIgnoreCase);
    }

    internal static string TopicRelatives(string text, string? previous = null)
    {
        text = TopicRelative.Replace(text, m => FalarForms.Contains(m.Groups["verb"].Value)
            ? m.Groups["ante"].Value + m.Groups["space"].Value +
              CaseHelper.MatchCase(m.Groups["relative"].Value, "de que") + m.Groups["tail"].Value
            : m.Value);
        if (previous == null || !Regex.IsMatch(text, @"^\s*sobre\s+o\s+que\b", Ci)) return text;
        var antecedent = PreviousTopic.Match(previous);
        if (!antecedent.Success) return text;
        var prefix = antecedent.Groups["ante"].Value + " ";
        return TopicRelatives(prefix + text)[prefix.Length..];
    }

    internal static bool HasTopicContinuation(string previous, string text) =>
        PreviousTopic.IsMatch(previous) && Regex.IsMatch(text, @"^\s*sobre\s+o\s+que\b", Ci);

    private static string EnglishSubject(string subject) => subject.ToLowerInvariant() switch
    {
        "eu" => "i", "tu" or "você" or "vocês" => "you", "ele" => "he", "ela" => "she",
        "nós" => "we", _ => "they",
    };

    private static string ConditionalEnding(string subject) => subject.ToLowerInvariant() switch
    {
        "tu" => "ias", "nós" => "íamos", "vocês" or "eles" or "elas" => "iam", _ => "ia",
    };

    internal static string Conditionals(string text, string? english)
    {
        if (string.IsNullOrWhiteSpace(english) || EnglishModal.Matches(english).Count != 1) return text;
        var predicates = EnglishConditional.Matches(english);
        if (predicates.Count != 1 || !EnglishBaseVerbs.Contains(predicates[0].Groups["verb"].Value)) return text;
        if (predicates[0].Groups["contracted"].Success &&
            predicates[0].Groups["verb"].Value.Equals("come", StringComparison.OrdinalIgnoreCase)) return text;
        var candidates = SubjectVerb.Matches(text).Where(m =>
            PortugueseVerbs.TrySplitFuture(m.Groups["verb"].Value, out _, out _, requireTransitive: false)).ToArray();
        if (candidates.Length != 1) return text;
        var candidate = candidates[0];
        var predicate = predicates[0];
        if (!EnglishSubject(candidate.Groups["subject"].Value).Equals(predicate.Groups["subject"].Value,
                StringComparison.OrdinalIgnoreCase) ||
            Regex.IsMatch(candidate.Groups["adverbs"].Value, @"\b(?:não|nunca|jamais)\b", Ci) !=
            predicate.Groups["negative"].Success)
            return text;
        var verb = candidate.Groups["verb"];
        PortugueseVerbs.TrySplitFuture(verb.Value, out var stem, out _, requireTransitive: false);
        var replacement = CaseHelper.MatchCase(verb.Value, stem + ConditionalEnding(candidate.Groups["subject"].Value));
        return text[..verb.Index] + replacement + text[(verb.Index + verb.Length)..];
    }

    internal static IEnumerable<string> Review(string text)
    {
        foreach (Match m in TopicRelative.Matches(text))
            if (FalarForms.Contains(m.Groups["verb"].Value))
                yield return $"Review: explicit antecedent “{m.Groups["ante"].Value}” before “sobre o que”; " +
                    "check the relative complement of falar (for example, de que).";
        foreach (Match m in SubjectVerb.Matches(text))
        {
            if (!PortugueseVerbs.TrySplitFuture(m.Groups["verb"].Value, out _, out var ending, requireTransitive: false))
                continue;
            var subject = m.Groups["subject"].Value.ToLowerInvariant();
            var expected = subject switch
            {
                "eu" => new[] { "ei", "ia" }, "tu" => new[] { "ás", "ias" },
                "nós" => new[] { "emos", "íamos" }, "vocês" or "eles" or "elas" => new[] { "ão", "iam" },
                _ => new[] { "á", "ia" },
            };
            if (!expected.Contains(ending, StringComparer.OrdinalIgnoreCase))
                yield return $"Review: explicit subject “{m.Groups["subject"].Value}” disagrees with " +
                    $"future/conditional “{m.Groups["verb"].Value}”; verify person and tense against the context.";
        }
    }
}
