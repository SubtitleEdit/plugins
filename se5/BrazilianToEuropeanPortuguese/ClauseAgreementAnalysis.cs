using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

[Flags]
internal enum VerbCandidate
{
    None = 0,
    ThirdPerson = 1,
    SecondPerson = 2,
    DictionaryValidated = 4,
    NominalHomograph = 8,
}

internal enum SubjectOwner { Unknown, Addressee, Other, RelativeAntecedent }
internal enum AgreementBlock { None, NotFiniteCandidate, NonFiniteUse, NominalUse, OtherSubject, RelativeSubject, Impersonal }
internal readonly record struct AgreementDecision(bool Allowed, AgreementBlock Block, SubjectOwner Owner, VerbCandidate Candidate);

/// <summary>Bounded token/clause evidence for omitted-subject agreement, not a full syntactic parser.</summary>
internal sealed class ClauseAgreementAnalysis
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    private static readonly Regex Tokens = new(
        @"[\p{L}\p{M}]+(?:-[\p{L}\p{M}]+)*|[.!?…;:,]|[-–—](?=\s*\p{L})", RegexOptions.Compiled);
    private static readonly HashSet<string> ClauseLinks = new(StringComparer.OrdinalIgnoreCase)
        { "que", "se", "quando", "porque", "embora", "enquanto", "caso", "mas" };
    private static readonly HashSet<string> Determiners = new(StringComparer.OrdinalIgnoreCase)
        { "o", "a", "os", "as", "um", "uma", "uns", "umas", "este", "esta", "aquele", "aquela" };
    private static readonly HashSet<string> Adverbs = new(StringComparer.OrdinalIgnoreCase)
        { "não", "nunca", "já", "ainda", "sempre", "só", "apenas", "também", "talvez", "bem", "então", "pois",
          "provavelmente", "certamente", "possivelmente", "realmente", "sim", "ora", "bom", "agora", "claro",
          "e", "ou" };
    private static readonly Regex AddresseeAdverb = new(
        @"^(?:" + PortugueseConverter.YouToTuRules.Adverbs + @")$", Ci);
    private static readonly HashSet<string> NominalHomographs = new(StringComparer.OrdinalIgnoreCase)
        { "ajuda", "conta", "guarda", "casa", "prova", "falta", "entrada", "saída", "certeza", "ordem", "trabalho",
          "comando", "cansaço", "resposta", "pergunta", "espera", "chatice", "miúda", "garota", "droga" };
    private static readonly HashSet<string> NominalFollowers = new(StringComparer.OrdinalIgnoreCase)
        { "financeira", "financeiro", "médica", "médico", "urgente", "extra", "disponível", "necessária",
          "necessário", "gratuita", "gratuito", "própria", "próprio", "esperta", "esperto" };
    private static readonly HashSet<string> ImpersonalComplements = new(StringComparer.OrdinalIgnoreCase)
        { "preciso", "necessário", "necessária", "possível", "impossível", "óbvio", "evidente", "pena",
          "tudo", "frio", "calor" };
    private static readonly Regex EnglishRelativeSubject = new(
        @"\b(?:(?:who|which)\s+|(?:the|a|an|some|any|everything)\s+(?:\p{L}+\s+){0,5}that\s+)" +
        @"(?:used\s+to|got|has|had|is|was|were|are|can|could|would|will|\p{L}+(?:s|ed))\b|" +
        @"\b(?:the|a|an)\s+\p{L}+\s+(?:we|I|he|she|they)(?:['’](?:re|ve|ll|d))?\s+[^.!?;]{1,60}\b\p{L}+s\b", Ci);
    private static readonly Regex EnglishOtherWhSubject = new(
        @"\b(?:what|which)(?:['’]s\b|\s+(?:\p{L}+(?:s|ed)|is|was|were|will|would|can|could|has|had)\b(?!\s+you\b))|" +
        @"\b(?:what|way)\s+(?:I|he|she|it|they|we)\b", Ci);
    private static readonly Regex EnglishWhStart = new(@"\b(?:what(?:['’](?:s|d|re))?|which)\b", Ci);
    private static readonly Regex EnglishYouWhSubject = new(
        @"\bwhat(?:['’](?:d|re)\s+|\s+(?:(?:the\s+(?:hell|fuck)|exactly|else|really|on\s+earth)\s+){0,2}" +
        @"(?:(?:do|did|are|were|can|could|will|would|have|had)\s+)?)you\b|\bwhich\s+you\b", Ci);
    private readonly record struct Token(string Text, int Start, int End);
    private readonly record struct Clause(int First, int Last, int Parent, SubjectOwner Owner, bool Conditional);
    private readonly Token[] _tokens;
    private readonly Clause[] _clauses;

    internal ClauseAgreementAnalysis(string text)
    {
        // Preserve offsets when called directly on unmasked text; converter markup is already masked.
        var plain = Regex.Replace(text, @"<[^>]*>|\{[^}]*\}", m => new string(' ', m.Length));
        _tokens = Tokens.Matches(plain).Select(m => new Token(m.Value, m.Index, m.Index + m.Length)).ToArray();
        var clauses = new List<Clause>();
        var first = 0;
        var parent = -1;
        var conditional = false;
        for (var i = 0; i < _tokens.Length; i++)
        {
            var word = _tokens[i].Text;
            var punctuation = word is "." or "!" or "?" or "…" or ";" or ":" or "–" or "—" or "-";
            var link = ClauseLinks.Contains(word);
            if (!punctuation && !link) continue;
            if (i > first)
            {
                clauses.Add(new Clause(first, i, parent, Owner(first, i), conditional));
                parent = clauses.Count - 1;
            }
            if (punctuation || word.Equals("mas", StringComparison.OrdinalIgnoreCase)) parent = -1;
            conditional = word.Equals("se", StringComparison.OrdinalIgnoreCase) ||
                word.Equals("quando", StringComparison.OrdinalIgnoreCase) ||
                word.Equals("que", StringComparison.OrdinalIgnoreCase) && i > 0 &&
                Regex.IsMatch(_tokens[i - 1].Text, @"^(?:assim|sempre|menos|vez|depois|antes|até|logo|desde)$", Ci);
            first = i + 1;
        }
        if (first < _tokens.Length)
            clauses.Add(new Clause(first, _tokens.Length, parent, Owner(first, _tokens.Length), conditional));
        _clauses = clauses.ToArray();
    }

    internal static VerbCandidate Candidates(string word)
    {
        var result = VerbCandidate.None;
        if (PortugueseVerbs.TryThirdToSecond(word, out _)) result |= VerbCandidate.ThirdPerson;
        if (PortugueseVerbs.IsSecondPersonForm(word)) result |= VerbCandidate.SecondPerson;
        if (!result.HasFlag(VerbCandidate.ThirdPerson) && PortugueseVerbs.TryGuessThirdToSecond(word, out _))
            result |= VerbCandidate.DictionaryValidated;
        if (NominalHomographs.Contains(word)) result |= VerbCandidate.NominalHomograph;
        return result;
    }

    private static readonly Regex FunctionWord = new(
        @"^(?:como|onde|aonde|donde|porque|quando)$", Ci);

    private SubjectOwner Owner(int first, int last)
    {
        for (var i = first; i < last; i++)
        {
            var word = _tokens[i].Text;
            if (word == "," || Adverbs.Contains(word) || AddresseeAdverb.IsMatch(word)) continue;
            if (word.Equals("tu", StringComparison.OrdinalIgnoreCase) || word.Equals("você", StringComparison.OrdinalIgnoreCase))
                return SubjectOwner.Addressee;
            if (Regex.IsMatch(word, @"^(?:eu|ele|ela|eles|elas|nós|vocês|isto|isso|aquilo|alguém|ninguém)$", Ci))
                return SubjectOwner.Other;
            if (Determiners.Contains(word) && i + 1 < last &&
                !PortugueseVerbs.IsKnownVerbForm(_tokens[i + 1].Text))
                return SubjectOwner.Other;
            if (PortugueseVerbs.IsSecondPersonForm(word)) return SubjectOwner.Addressee;
            if (Candidates(word).HasFlag(VerbCandidate.ThirdPerson)) return SubjectOwner.Unknown;
            // A capitalized lexical subject before a finite verb, not a vocative.
            if (char.IsUpper(word[0]) && !FunctionWord.IsMatch(word) && i + 1 <= last && i + 1 < _tokens.Length && _tokens[i + 1].Text != "," &&
                Candidates(_tokens[i + 1].Text).HasFlag(VerbCandidate.ThirdPerson))
                return SubjectOwner.Other;
            return SubjectOwner.Unknown;
        }

        return SubjectOwner.Unknown;
    }

    private bool IsWhatClause(Clause clause) => clause.First > 1 &&
        _tokens[clause.First - 1].Text.Equals("que", StringComparison.OrdinalIgnoreCase) &&
        _tokens[clause.First - 2].Text.Equals("o", StringComparison.OrdinalIgnoreCase);

    private string WhReference(Clause clause, ReferenceHint? reference)
    {
        var english = reference?.EnglishText ?? "";
        var starts = EnglishWhStart.Matches(english);
        var clauses = _clauses.Where(IsWhatClause).ToArray();
        if (starts.Count != clauses.Length) return english;
        var index = Array.IndexOf(clauses, clause);
        if (index < 0) return english;
        var end = index + 1 < starts.Count ? starts[index + 1].Index : english.Length;
        return english[starts[index].Index..end];
    }

    internal AgreementDecision Decide(int verbOffset, ReferenceHint? reference)
    {
        var index = Array.FindIndex(_tokens, t => t.Start == verbOffset);
        if (index < 0) return new(false, AgreementBlock.NotFiniteCandidate, SubjectOwner.Unknown, VerbCandidate.None);
        var candidate = Candidates(_tokens[index].Text);
        if ((candidate & (VerbCandidate.ThirdPerson | VerbCandidate.SecondPerson | VerbCandidate.DictionaryValidated)) == 0)
            return new(false, AgreementBlock.NotFiniteCandidate, SubjectOwner.Unknown, candidate);
        var clause = _clauses.FirstOrDefault(c => c.First <= index && index < c.Last);
        var owner = Owner(clause.First, index);
        if (owner == SubjectOwner.Other)
            return new(false, AgreementBlock.OtherSubject, owner, candidate);
        if (owner == SubjectOwner.Addressee)
            return new(true, AgreementBlock.None, owner, candidate);
        if (candidate.HasFlag(VerbCandidate.NominalHomograph) &&
            (!candidate.HasFlag(VerbCandidate.ThirdPerson) && !candidate.HasFlag(VerbCandidate.SecondPerson) ||
             index + 1 < clause.Last && (NominalFollowers.Contains(_tokens[index + 1].Text) ||
             Regex.IsMatch(_tokens[index + 1].Text, @"^(?:é|era|foi|será|seria|está|estava)$", Ci))))
            return new(false, AgreementBlock.NominalUse, owner, candidate);
        if (PortugueseVerbs.IsKnownInfinitive(_tokens[index].Text) && !clause.Conditional && clause.First > 0 &&
            _tokens[clause.First - 1].Text.Equals("que", StringComparison.OrdinalIgnoreCase) &&
            (IsWhatClause(clause) || clause.First > 1 &&
             _tokens[clause.First - 2].Text.Equals("para", StringComparison.OrdinalIgnoreCase) ||
             index + 1 < clause.Last && Regex.IsMatch(_tokens[index + 1].Text, @"^(?:é|era|foi|será|seria)$", Ci)) &&
            (!Regex.IsMatch(reference?.EnglishText ?? "", @"\b(?:what|whatever|anything|everything)\s+you\b", Ci) ||
             Regex.IsMatch(reference?.EnglishText ?? "", @"\bwhat\s+you\s+(?:need|have|ought|want)\s+to\b", Ci)))
            return new(false, AgreementBlock.NonFiniteUse, owner, candidate);
        if (Regex.IsMatch(_tokens[index].Text, @"^(?:é|era|foi|será|seria|está|estava)$", Ci) &&
            index + 1 < clause.Last && ImpersonalComplements.Contains(_tokens[index + 1].Text))
            return new(false, AgreementBlock.Impersonal, owner, candidate);
        var whReference = IsWhatClause(clause) ? WhReference(clause, reference) : "";
        if (IsWhatClause(clause) &&
            (EnglishOtherWhSubject.IsMatch(whReference) && !EnglishYouWhSubject.IsMatch(whReference) ||
             EnglishRelativeSubject.IsMatch(whReference) && !EnglishYouWhSubject.IsMatch(whReference) ||
             _tokens[index].Text.Equals("quer", StringComparison.OrdinalIgnoreCase) && index + 1 < _tokens.Length &&
             _tokens[index + 1].Text.Equals("que", StringComparison.OrdinalIgnoreCase)))
            return new(false, AgreementBlock.RelativeSubject, SubjectOwner.RelativeAntecedent, candidate);
        if (clause.Parent >= 0 && reference is not { RepeatedYouSubject: true })
        {
            var parent = _clauses[clause.Parent];
            if (!clause.Conditional && _tokens[clause.First - 1].Text.Equals("que", StringComparison.OrdinalIgnoreCase) &&
                parent.Last - parent.First >= 2 &&
                Regex.IsMatch(_tokens[parent.Last - 2].Text, @"^(?:o|a|os|as|um|uma|uns|umas|do|da|dos|das|dum|duma)$", Ci) &&
                !PortugueseVerbs.IsKnownVerbForm(_tokens[parent.Last - 1].Text) &&
                owner != SubjectOwner.Addressee &&
                EnglishRelativeSubject.IsMatch(reference?.EnglishText ?? ""))
                return new(false, AgreementBlock.RelativeSubject, SubjectOwner.RelativeAntecedent, candidate);
            // A relative whose antecedent precedes a reporting bridge owns the embedded subject.
            if (parent.Parent >= 0 && parent.Owner == SubjectOwner.Addressee)
            {
                var ancestor = _clauses[parent.Parent];
                if (ancestor.Owner == SubjectOwner.Other && _tokens[parent.First - 1].Text.Equals("que", StringComparison.OrdinalIgnoreCase))
                    return new(false, AgreementBlock.RelativeSubject, SubjectOwner.RelativeAntecedent, candidate);
            }
        }
        return new(true, AgreementBlock.None, owner, candidate);
    }
}
