using System.Text.RegularExpressions;
using SubtitleEdit.Plugins.Shared;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal static class ContextualComplements
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    private const string Word = @"[\p{L}\p{M}]+";
    private const string Boundary = @"(?![\p{L}\p{M}-])";
    private const string PlainSubject = @"(?:ele|ela|eles|elas|eu|tu|nós|vós|você|vocês)";
    private const string Subject = @"(?:" + PlainSubject +
        @"|(?:o|a|os|as|um|uma|uns|umas)\s+(?!que\b)" + Word +
        @"(?:\s+(?!(?:que|não|nunca|já|também|se|me|te|nos|lhe|lhes|e|ou|mas|quando|como|onde|aonde|porque|enquanto|embora|caso|quem|cujo|cuja|cujos|cujas|com|sem|de|em|para|por|sobre|contra|até|desde|durante)\b)" + Word + @"){0,5})";
    private static readonly Regex MissingQue = new(
        @"(?<![\p{L}\p{M}-])(?<verb>parece|parecia|pareceu|parecerá|pareceria|pareça|parecesse)" +
        @"(?<dative>-(?:me|te|lhe|nos|vos|lhes))?(?<gap>\s+)", Ci);
    private static readonly Regex ComplementTail = new(
        @"^" + Word + @"(?:\s+" + Word + @"){1,11}" + Boundary, Ci);
    private static readonly Regex ComplementSubject = new(
        @"^(?<subject>" + Subject + @")\s+(?:(?:não|nunca|já|ainda|também)\s+)?" +
        @"(?:(?:me|te|se|nos|vos|lhe|lhes|o|a|os|as)\s+)?$", Ci);
    private static readonly Regex Arrival = new(
        @"^\s*[\uE000-\uF8FF]*(?<person>ele|ela|eles|elas)\s+" +
        @"(?:(?:está|estão)\s+(?:aqui|aí|ali|lá|à\s+espera)|chegou|chegaram)" +
        @"[.!…\s\uE000-\uF8FF]*$", Ci);
    private static readonly Regex Admission = new(
        @"(?<head>^[\s\uE000-\uF8FF]*(?:[-–—]\s*)?)(?<negative>não\s+)?" +
        @"(?<verb>manda|mande|mandem|mandai|deixa|deixe|deixem|deixai)(?<gap>\s+)" +
        @"(?<inf>entrar|sair|subir|descer|passar|esperar)" +
        @"(?=\s*(?:[.!…]|[\uE000-\uF8FF]|$))", Ci);
    private static string Plain(string text) => Regex.Replace(text, @"<[^>]*>|\{[^}]*\}", "");

    internal static ReferenceHint? WithContext(IReadOnlyList<SrtBlock> blocks, int index, ReferenceHint? hint,
        IReadOnlyList<string?>? english, IReadOnlyList<string?>? speakers, ISet<int>? selected)
    {
        bool Selected(int i) => selected is not { Count: > 0 } || selected.Contains(i);
        if (!Selected(index)) return hint;
        if (index + 1 < blocks.Count && Selected(index + 1) &&
            blocks[index + 1].StartMs >= blocks[index].StartMs &&
            blocks[index + 1].StartMs - blocks[index].EndMs <= 1500 &&
            !DialogueRegister.DifferentSpeaker(blocks[index].Text, blocks[index + 1].Text) &&
            (speakers == null || string.IsNullOrWhiteSpace(speakers[index]) ||
                string.IsNullOrWhiteSpace(speakers[index + 1]) || speakers[index] == speakers[index + 1]) &&
            !Regex.IsMatch(Plain(blocks[index].Text), @"[.!?…:;]\s*$") &&
            Regex.IsMatch(Plain(blocks[index + 1].Text), @"^\s*[\p{Ll}]"))
            hint = (hint ?? new ReferenceHint(false, false, false, false)) with
                { ComplementContinuation = Plain(blocks[index + 1].Text) };
        if (index > 0 && Selected(index - 1) && blocks[index].StartMs >= blocks[index - 1].StartMs &&
            blocks[index].StartMs - blocks[index - 1].EndMs <= 5000)
        {
            var arrival = Arrival.Match(Plain(blocks[index - 1].Text));
            if (arrival.Success)
            {
                var person = arrival.Groups["person"].Value.ToLowerInvariant();
                var reference = english?[index] ?? "";
                var expected = person switch { "ela" => "her", "ele" => "him", _ => "them" };
                var references = Regex.Matches(reference, @"\b(?:him|her|them|you|me|us|it)\b", Ci);
                var command = Admission.Match(Plain(blocks[index].Text));
                if (command.Success && (string.IsNullOrWhiteSpace(reference) ||
                    references.Count == 1 && references[0].Value.Equals(expected, StringComparison.OrdinalIgnoreCase) &&
                    ConfirmsAdmission(reference, expected, command.Groups["inf"].Value, command.Groups["negative"].Success)))
                    hint = (hint ?? new ReferenceHint(false, false, false, false)) with
                        { ArrivalObject = person switch { "ela" => "a", "ele" => "o", "elas" => "as", _ => "os" } };
            }
        }
        return hint;
    }

    private static bool ConfirmsAdmission(string english, string person, string infinitive, bool negative)
    {
        var action = infinitive.ToLowerInvariant() switch
        {
            "entrar" => @"(?:in|inside|enter|come\s+in|come\s+inside)",
            "sair" => @"(?:out|outside|leave|go\s+out)",
            "subir" => @"(?:up|upstairs|come\s+up)",
            "descer" => @"(?:down|downstairs|come\s+down)",
            "passar" => @"(?:through|pass)",
            _ => "wait",
        };
        var negation = negative ? @"(?:don['’]t|do\s+not)\s+" : "";
        return Regex.IsMatch(english, @"^\s*(?:please[,\s]+)?" + negation + @"(?:let|send|bring|show|have|tell|ask|make)\s+" +
            person + @"\s+(?:to\s+)?" + action + @"[.!…\s]*$", Ci) ||
            !negative && infinitive.Equals("entrar", StringComparison.OrdinalIgnoreCase) &&
            Regex.IsMatch(english, @"^\s*let['’]s\s+have\s+" + person + @"[.!…\s]*$", Ci);
    }

    private static bool Finite(string word) =>
        PortugueseVerbs.IsFiniteCliticForm(word) && !PortugueseVerbs.IsInfinitive(word) &&
        !Regex.IsMatch(word, @"^(?:que|não|nunca|já|ainda|também|se|me|te|nos|vos|lhe|lhes|o|a|os|as|e|ou|mas)$", Ci) &&
        (PortugueseVerbs.IsKnownVerbForm(word) || PortugueseVerbs.IsSecondPersonForm(word) ||
         PortugueseVerbs.IsPorFiniteForm(word) ||
         Regex.IsMatch(word, @"(?:ou|eu|iu|aram|eram|iram|ávamos|íamos|aste|este|iste|asse|esse|isse|ava)$", Ci)) &&
        !Regex.IsMatch(word, @"(?:ndo|[ai]d[oa]s?)$", Ci) &&
        !ClauseAgreementAnalysis.Candidates(word).HasFlag(VerbCandidate.NominalHomograph);

    internal static string Parecer(string text, ReferenceHint? reference, Func<string, bool> blocked)
    {
        if (blocked("parecer")) return text;
        var continuation = reference?.ComplementContinuation;
        var combined = continuation == null ? text : text + " " + continuation;
        foreach (Match m in MissingQue.Matches(combined).Cast<Match>().Reverse())
        {
            if (blocked(m.Groups["verb"].Value)) continue;
            var tailStart = m.Index + m.Length;
            var tail = ComplementTail.Match(combined[tailStart..]);
            if (!tail.Success) continue;
            var finite = Regex.Matches(tail.Value, Word).Cast<Match>().Skip(1).FirstOrDefault(w =>
            {
                if (!Finite(w.Value) || blocked(w.Value)) return false;
                var subject = ComplementSubject.Match(tail.Value[..w.Index]).Groups["subject"];
                if (!subject.Success || tailStart + subject.Index + subject.Length > text.Length ||
                    Regex.IsMatch(subject.Value, @"\b(?:de|do|da|dos|das|em|no|na|nos|nas|a|o|os|as|um|uma|com|para|por|pelo|pela)$", Ci))
                    return false;
                return Regex.IsMatch(subject.Value, "^" + PlainSubject + "$", Ci) ||
                    PortugueseVerbs.IsPorFiniteForm(w.Value) ||
                    Regex.IsMatch(w.Value, @"^(?:é|está|estão|tem|têm|há|pode|podem|quer|querem|vai|vão|diz|dizem|vê|veem|sabe|sabem|" +
                        @"foi|foram|era|eram|estava|estavam|tinha|tinham|havia|haviam|houve|houveram|fez|fizeram|disse|disseram|viu|viram|deu|deram)$", Ci) ||
                    Regex.IsMatch(w.Value, @"(?:ou|eu|iu|aram|eram|iram|ávamos|íamos|aste|este|iste|asse|esse|isse|ava)$", Ci);
            });
            if (finite == null) continue;
            // The whole span from "parecer" to the finite verb must stay within one paragraph and speaker.
            if (Regex.IsMatch(combined[m.Index..(tailStart + finite.Index + finite.Length)],
                    @"\r?\n[ \t]*\r?\n|\r?\n[ \t]*[-–—]")) continue;
            var gap = m.Groups["gap"];
            text = text.Insert(gap.Index + gap.Length, CaseHelper.IsAllUpper(m.Value + tail.Value) ? "QUE " : "que ");
        }
        return text;
    }

    internal static string RecoverArrivalObject(string text, ReferenceHint? reference, Func<string, bool> blocked)
    {
        if (reference?.ArrivalObject is not { } clitic) return text;
        return Admission.Replace(text, m =>
        {
            var verb = m.Groups["verb"].Value;
            var inf = m.Groups["inf"].Value;
            if (blocked(verb) || blocked(inf) || blocked(verb.StartsWith("mand", StringComparison.OrdinalIgnoreCase)
                ? "mandar" : "deixar")) return m.Value;
            var upper = CaseHelper.IsAllUpper(m.Value);
            var pronoun = upper ? clitic.ToUpperInvariant() : clitic;
            return m.Groups["head"].Value + m.Groups["negative"].Value +
                (m.Groups["negative"].Success ? pronoun + " " + verb :
                    PortugueseVerbs.AttachEnclitic(verb, pronoun)) + m.Groups["gap"].Value + inf;
        });
    }
}
