using System.Text;
using System.Text.RegularExpressions;
using SubtitleEdit.Plugins.Shared;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

/// <summary>
/// Who an English reference cue talks about. It only steers ambiguous rules (seu/sua, dropped "você");
/// it never rewrites text by itself.
/// </summary>
public sealed record ReferenceHint(bool SecondPerson, bool SecondPossessive, bool ThirdPerson, bool ThirdPossessive)
{
    /// <summary>A nearby explicit form of address establishes a formal dialogue register.</summary>
    public bool FormalAddress { get; init; }
    public bool ForceInformal { get; init; }
    public bool IgnorePreviousAddress { get; init; }
    public bool InclusiveContinuation { get; init; }
    public bool CatchUpLater { get; init; }
    internal bool RepeatedYouSubject { get; init; }
    internal bool RepeatedConditionalYouSubject { get; init; }
    internal bool ConditionalYouSubject { get; init; }
    internal bool SelfActionQuestion { get; init; }
    internal bool SecondPersonWhatSubject { get; init; }
    internal string? EnglishText { get; init; }

    public static ReferenceHint? FromContext(IReadOnlyList<SrtBlock> source, int index, IReadOnlyList<string?>? english = null,
        IReadOnlyList<string?>? speakers = null)
    {
        var hint = FromText(english?[index]);
        if (hint != null && index > 0 && english != null &&
            InitialConditionalYouSubject.IsMatch(ReferenceAligner.Clean(english[index - 1] ?? "")) &&
            source[index].StartMs >= source[index - 1].StartMs &&
            source[index].StartMs - source[index - 1].EndMs <= 1500 &&
            Regex.IsMatch(source[index - 1].Text, @"(?:\.\.\.|…)\s*$") &&
            Regex.IsMatch(hint.EnglishText ?? "", @"^(?:call|phone|ring|ask|tell|wait|leave|go|come|help|take|do)\b[^.!;]*\?$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            hint = hint with { SelfActionQuestion = true };
        if (index > 0 && source[index].StartMs >= source[index - 1].StartMs &&
            source[index].StartMs - source[index - 1].EndMs <= 5000 &&
            !DialogueRegister.DifferentSpeaker(source[index - 1].Text, source[index].Text) &&
            (speakers == null || string.IsNullOrWhiteSpace(speakers[index - 1]) || string.IsNullOrWhiteSpace(speakers[index]) ||
             speakers[index - 1] == speakers[index]) &&
            Regex.IsMatch(source[index - 1].Text, @"(?<!\p{L})(?:a\s+gente|nós)\b", RegexOptions.IgnoreCase) &&
            Regex.IsMatch(source[index].Text, @"^\s*mas\s+nunca\s+(?:acha|pensa|imagina)\s+que\b", RegexOptions.IgnoreCase) &&
            (english == null || hint is { SecondPerson: true } &&
             !Regex.IsMatch(english[index] ?? string.Empty, @"\b(?:he|she|they)\b", RegexOptions.IgnoreCase)))
        {
            hint = (hint ?? new ReferenceHint(false, false, false, false)) with { InclusiveContinuation = true };
        }
        return DialogueRegister.IsFormal(source, index, english, speakers)
            ? (hint ?? new ReferenceHint(false, false, false, false)) with { FormalAddress = true }
            : hint;
    }

    private static readonly Regex Word = new(@"\p{L}+(?:['’]\p{L}+)?", RegexOptions.Compiled);
    private static readonly HashSet<string> Second = new(StringComparer.Ordinal) { "you", "ya", "yourself", "yourselves", "y'all" };
    private static readonly HashSet<string> SecondOwn = new(StringComparer.Ordinal) { "your", "yours" };
    private static readonly HashSet<string> Third = new(StringComparer.Ordinal)
    {
        "he", "she", "it", "they", "him", "them", "himself", "herself", "itself", "themselves",
    };
    private static readonly HashSet<string> ThirdOwn = new(StringComparer.Ordinal) { "his", "her", "hers", "its", "their", "theirs" };
    private static readonly HashSet<string> BoyWords = new(StringComparer.Ordinal)
    {
        "boy", "boys", "guy", "guys", "lad", "lads", "fella", "fellas", "fellow", "dude", "dudes", "man", "men",
    };
    private static readonly HashSet<string> GirlWords = new(StringComparer.Ordinal) { "girl", "girls", "lass", "lady", "ladies", "woman", "women" };
    private static readonly HashSet<string> KidWords = new(StringComparer.Ordinal)
    {
        "kid", "kids", "kiddo", "child", "children", "baby", "babies", "little", "toddler", "toddlers", "brat", "brats",
    };

    /// <summary>The English says "boy/guy" (not "kid"): "garoto" is "rapaz", not "miúdo".</summary>
    public bool Boy { get; init; }

    /// <summary>The English says "girl" (not "kid"): "garota" is "rapariga", not "miúda".</summary>
    public bool Girl { get; init; }

    /// <summary>The English says "kid/child/little": "miúdo" is right.</summary>
    public bool Kid { get; init; }

    /// <summary>The English cue has "you" as a grammatical subject, rather than only as an object.</summary>
    public bool SecondPersonSubject { get; init; }

    /// <summary>The aligned English cue starts with "you" as its subject.</summary>
    public bool StartsWithSecondPersonSubject { get; init; }

    /// <summary>The reference uses never + verb + any as an object, not "any more/longer".</summary>
    public bool NeverAnyObject { get; init; }

    /// <summary>The English names an "officer" ("cop" stays polícia): the Brazilian "policial" is pt-PT "agente", not "polícia".</summary>
    public bool Officer { get; init; }

    /// <summary>Hearing about news/events, not deliberately listening to a person or sound.</summary>
    public bool HearAbout { get; init; }

    /// <summary>
    /// The English says "this/these" and never "that/those": Brazilian "isso/esse" (used for both) is pt-PT "isto/este".
    /// </summary>
    public bool This { get; init; }

    /// <summary>The English verb is in the past ("stepped", "did", "was") with no modal: "Eu nunca sair" → "Eu nunca saí".</summary>
    public bool Past { get; init; }

    /// <summary>The English verb is in the present ("do", "is", "don't") with no past or modal: "Eu nunca sair" → "Eu nunca saio".</summary>
    public bool Present { get; init; }

    private static readonly HashSet<string> PastWords = new(StringComparer.Ordinal)
    {
        "did", "didn't", "was", "wasn't", "were", "weren't", "had", "hadn't", "went", "came", "saw", "said", "told", "took", "got",
        "made", "left", "ran", "gave", "found", "knew", "thought", "felt", "kept", "lost", "met", "paid", "sat", "stood", "wrote",
        "broke", "brought", "bought", "caught", "fought", "taught", "drove", "ate", "fell", "forgot", "heard", "held", "meant",
        "sent", "slept", "spent", "spoke", "stole", "swore", "threw", "understood", "woke", "won", "wore", "began", "chose", "drank",
        "flew", "grew", "hid", "led", "rode", "rang", "sang", "sold", "shot", "sank", "swam", "tore", "became", "built", "dealt",
    };
    private static readonly HashSet<string> PresentWords = new(StringComparer.Ordinal)
    {
        "do", "does", "don't", "doesn't", "am", "i'm", "is", "isn't", "are", "aren't", "you're", "we're", "they're", "he's", "she's",
    };
    private static readonly HashSet<string> Modals = new(StringComparer.Ordinal)
    {
        "will", "won't", "would", "wouldn't", "can", "can't", "cannot", "could", "couldn't", "should", "shouldn't", "must", "might",
        "may", "gonna", "going", "i'll", "you'll", "we'll", "they'll", "he'll", "she'll", "i'd", "you'd", "we'd", "they'd", "let's",
    };
    private static readonly HashSet<string> NotPast = new(StringComparer.Ordinal) { "red", "bed", "shed", "wed", "sled", "hundred", "naked", "sacred", "wicked" };
    private static readonly string SubjectVerbs = string.Join("|", PastWords.Concat(PresentWords).Concat(Modals)
        .Concat(new[] { "know", "think", "want", "need", "go", "come", "get", "say", "tell", "make", "take", "give", "find", "call", "help", "work", "live", "like", "love", "hate", "feel", "look", "hear", "remember", "forget", "understand", "believe", "mean", "let", "keep", "leave", "put", "run", "pay", "speak", "talk", "ask", "try", "wait", "stay", "move", "use", "open", "close", "drive", "eat", "drink", "follow", "plan", "start", "stop", "change", "touch", "hold", "kill", "bring", "buy", "meet", "spend", "send", "win", "stand", "sit", "wear", "read", "play", "walk", "happen" })
        .Distinct(StringComparer.Ordinal)
        .OrderByDescending(word => word.Length)
        .Select(Regex.Escape));
    private static readonly Regex InitialYouSubject = new(
        @"^\s*(?:(?:no|yes)\s*[.,!]\s*)?" +
        @"(?:(?:well|so|then|but|and|now|hey|look|listen|okay|ok|all\s+right)\b[,!?;:]?\s*)?" +
        @"(?:(?:maybe|perhaps|probably|surely|certainly|at\s+least)\s+){0,2}(?:you\b|you['’](?:re|ll|ve|d)\b|y['’]all\b)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex InitialConditionalYouSubject = new(
        @"^\s*(?:(?:well|so|then|but|and|or)\b[,!?;:]?\s*)?" +
        @"(?:(?:(?:what|even|only)\s+)?if|when|whenever|unless|as\s+soon\s+as)\s+" +
        @"you(?=\s+(?!(?:and|or|all|both|guys)\b)\p{L})",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex YouSubject = new(
        @"(?<![\p{L}\p{M}])(?:you|y['’]all)\s+(?:(?:never|ever|always|already|just|really|still|only|also|actually)\s+){0,3}(?:" + SubjectVerbs + @"|\p{L}+(?:ed|ing))\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    // "Why did you decide to help us?": the inverted "you" of a wh-question is the subject.
    private static readonly Regex InvertedYouSubject = new(
        @"\b(?:how|where|why|what|when)\s+(?:the\s+(?:hell|heck|fuck)\s+)?(?:did|do|does|were|are|have|had|will|would|can|could|should|must|shall)(?:n['’]t)?\s+you\s+(?:(?:not|never|ever|always|already|just|really|still|even)\s+){0,2}" +
        @"(?!(?:guys|all|two|both|people|two|lot|and|or|there|here)\b)\p{L}",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex ConditionalYouSubjects = new(
        @"\bif\s+you\b(?:(?!\b(?:he|she|they|we|I|it)\b)[^.!?;]){1,100}\b" +
        @"you(?:['’](?:re|ll|ve|d)\b|\s+(?:(?:never|always|just|really|still|only|also)\s+){0,3}" +
        @"(?:" + SubjectVerbs + @"|\p{L}+ed)\b)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex NeverAny = new(
        @"\bnever\s+(?:\p{L}+\s+){1,3}any\s+(?!more\b|longer\b)\p{L}+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex HearingAbout = new(
        @"\b(?:hear|hears|heard|hearing)\s+(?:(?:always|often|never|ever|just|sometimes|usually)\s+){0,2}about\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>The reference says "his/her/their" and not "your": keep "o seu/a sua".</summary>
    public bool KeepsThirdPersonPossessive => ThirdPossessive && !SecondPossessive;

    /// <summary>The reference has a 3rd-person subject and no "you": a dropped subject is not "você".</summary>
    public bool ExcludesDroppedYou => ThirdPerson && !SecondPerson && !SecondPossessive;

    public static ReferenceHint? FromText(string? english)
    {
        if (string.IsNullOrWhiteSpace(english))
        {
            return null;
        }

        bool second = false, secondOwn = false, third = false, thirdOwn = false, boy = false, girl = false, kid = false, near = false, far = false;
        bool past = false, present = false, modal = false;
        var clean = ReferenceAligner.Clean(english);
        foreach (Match m in Word.Matches(clean))
        {
            var word = m.Value.ToLowerInvariant().Replace('’', '\'');
            var stem = word.Split('\'')[0];
            past |= PastWords.Contains(word) || word.Length >= 5 && word.EndsWith("ed") && !word.EndsWith("eed") && !NotPast.Contains(word);
            present |= PresentWords.Contains(word);
            modal |= Modals.Contains(word);
            boy |= BoyWords.Contains(stem);
            girl |= GirlWords.Contains(stem);
            kid |= KidWords.Contains(stem);
            near |= stem is "this" or "these";
            far |= stem is "that" or "those";
            if (word == "y'all" || Second.Contains(stem)) second = true;
            else if (SecondOwn.Contains(stem)) secondOwn = true;
            else if (ThirdOwn.Contains(stem))
            {
                thirdOwn = true;
                third = true;
            }
            else if (Third.Contains(stem)) third = true;
        }

        var subjectText = Regex.Replace(clean, @"^\s*you\s+know\s*,\s*", "",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        var startsWithSecondPersonSubject = InitialYouSubject.IsMatch(subjectText);
        var secondPersonSubject = startsWithSecondPersonSubject || YouSubject.IsMatch(subjectText) || InvertedYouSubject.IsMatch(subjectText);
        return new ReferenceHint(second, secondOwn, third, thirdOwn)
        {
            EnglishText = clean,
            RepeatedConditionalYouSubject = ConditionalYouSubjects.Matches(clean).Count == 1,
            ConditionalYouSubject = InitialConditionalYouSubject.IsMatch(clean),
            RepeatedYouSubject = Regex.IsMatch(clean,
                @"\byou\s+(?:think|believe|know|hope|said|thought|knew|realized|realised|discovered)\s+(?:that\s+)?you\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
            SecondPersonWhatSubject = Regex.IsMatch(clean,
                @"\bwhat\s+(?:(?:are|were|do|did|have|had|can|could|will|would|shall|should|must|may|might)\s+)?you\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
            Boy = boy && !kid,
            Girl = girl && !kid,
            Kid = kid,
            This = near && !far,
            SecondPersonSubject = secondPersonSubject,
            StartsWithSecondPersonSubject = startsWithSecondPersonSubject,
            NeverAnyObject = NeverAny.IsMatch(clean),
            Officer = Regex.IsMatch(clean, @"\bofficers?\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
            HearAbout = HearingAbout.IsMatch(clean) && !Regex.IsMatch(clean, @"\blisten(?:s|ed|ing)?\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
            CatchUpLater = Regex.IsMatch(clean, @"\bcatch(?:\s+up\s+with)?\s+you(?:\s+guys|\s+all)?\s+later\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
            Past = past && !modal, Present = present && !past && !modal };
    }
}

internal static class DialogueRegister
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    private const string Name = @"(?:\s+(?-i:\p{Lu})[\p{L}\p{M}'’-]*){0,3}";
    private const string Start = @"(?:^|[,!?;:\n]\s*|(?<![\p{L}\p{M}])[-–—]\s*)\s*(?:(?:bem|certo|sim|não|well|okay|yes|no)\s*,?\s+)?";
    private const string End = @"(?=\s*(?:[,!?…]|\.\.\.(?=\s|$)|\.(?=\s*$)|$))";
    private static readonly Regex PortugueseAddress = new(
        Start + @"(?:(?:sr|sra|srta|senhor|senhora)\.?\s+)?(?:" +
        PortugueseConverter.YouToTuRules.FormalTitles + @")\.?" + Name + End, Ci);
    private static readonly Regex EnglishAddress = new(
        Start + @"(?:(?:Mr|Mrs|Ms|Dr)\.?\s+(?:(?:Deputy|Vice)[ -])?(?:Consul|Ambassador|President)" + Name +
        @"|(?:Mr|Mrs|Ms|Dr)\.?" + Name +
        @"|sir|ma'am|madam|Detective|Officer|Inspector|Captain|Lieutenant|Sergeant|Colonel|General|Commander|Chief|Judge|Professor|Doctor|Ambassador|Consul|Your\s+(?:Honor|Majesty|Highness|Excellency))" +
        Name + End, Ci);
    private static readonly Regex Informal = new(
        @"(?<![\p{L}\p{M}])(?:tu|teus?|tuas?|ti|contigo)(?![\p{L}\p{M}])", Ci);
    private static readonly Regex NamedTitle = new(
        @"\b(?:Sr|Sra|Srta|Dr|Dra|Mr|Mrs|Ms|senhor|senhora|doutor|doutora|Doctor|Detective|detetive)\.?\s+" +
        @"(?-i:\p{Lu}[\p{Ll}\p{M}'’-]+)", Ci);
    private static readonly Regex PersonApposition = new(
        @"(?:\b(?:é|era|foi|são|eram|is|was|are|were)|['’]s)\s+[^.!?;]{0,60}\b" +
        @"(?:amig[oa]s?|vizinh[oa]s?|colegas?|irmã[oa]s?|mãe|pai|esposa|marido|" +
        @"friend|neighbou?r|colleague|brother|sister|mother|father|wife|husband)" +
        @"[^.!?;,]{0,40},\s*$", Ci);

    private static readonly Regex DirectAddressTail = new(
        @"^\s*,\s*(?:(?:você|o\s+senhor|a\s+senhora)\s+)?" +
        @"(?:pode|poderia|quer|sabe|diga|faça|venha|you|can\s+you|could\s+you|would\s+you|will\s+you)\b", Ci);

    private static bool HasAddress(string text, Regex pattern) => pattern.Matches(text).Cast<Match>().Any(m =>
    {
        if (!NamedTitle.IsMatch(m.Value) || DirectAddressTail.IsMatch(text[(m.Index + m.Length)..])) return true;
        var comma = m.Value.IndexOf(',');
        var before = text[..(m.Index + (comma >= 0 ? comma + 1 : 0))];
        return !PersonApposition.IsMatch(before);
    });

    internal static bool IsFormal(IReadOnlyList<SrtBlock> source, int index, IReadOnlyList<string?>? english = null,
        IReadOnlyList<string?>? speakers = null)
    {
        if (Informal.IsMatch(Plain(source[index].Text))) return false;
        if (Address(index)) return true;
        foreach (var direction in new[] { -1, 1 })
        {
            var previous = index;
            var speaker = speakers?[index]?.Trim() ?? SpeakerOf(source[index].Text);
            for (var distance = 1; distance <= 12; distance++)
            {
                var candidate = index + direction * distance;
                if (candidate < 0 || candidate >= source.Count) break;
                var left = source[Math.Min(previous, candidate)];
                var right = source[Math.Max(previous, candidate)];
                var candidateSpeaker = speakers?[candidate]?.Trim() ?? SpeakerOf(source[candidate].Text);
                if (right.StartMs < left.StartMs || right.StartMs - left.EndMs > 5000 ||
                    Math.Abs(source[candidate].StartMs - source[index].StartMs) > 30000 ||
                    Informal.IsMatch(Plain(source[candidate].Text))) break;
                // Dash-separated exchanges are the same conversation: their address still counts, and the scan continues past them.
                var labelChange = speaker.Length > 0 && candidateSpeaker.Length > 0 && speaker != candidateSpeaker ||
                                  SpeakerOf(left.Text).Length > 0 && SpeakerOf(right.Text).Length > 0 && SpeakerOf(left.Text) != SpeakerOf(right.Text);
                if (labelChange) break;
                if (Address(candidate)) return true;
                if (candidateSpeaker.Length > 0) speaker = candidateSpeaker;
                previous = candidate;
            }
        }
        return false;

        bool Address(int i) => HasAddress(Plain(source[i].Text), PortugueseAddress) ||
                               english != null && HasAddress(Plain(english[i] ?? string.Empty), EnglishAddress);
    }

    private static string Plain(string text) => Regex.Replace(text, @"<[^<>]*>|\{[^{}]*\}", " ");

    internal static bool DifferentSpeaker(string left, string right)
    {
        var a = SpeakerOf(left);
        var b = SpeakerOf(right);
        return a.Length > 0 && b.Length > 0 && a != b ||
               Regex.Matches(Plain(left), @"(?m)^\s*[-–—]\s+").Count > 1 ||
               Regex.Matches(Plain(right), @"(?m)^\s*[-–—]\s+").Count > 1;
    }

    private static string SpeakerOf(string text) =>
        Regex.Match(Plain(text), @"^\s*(?<name>[\p{Lu}][\p{Lu}\d _'-]{1,35}):").Groups["name"].Value;

    internal static bool ConflictingAddress(string text) =>
        HasAddress(Plain(text), PortugueseAddress) && Informal.IsMatch(Plain(text));
}

public sealed record TimeMap(double Scale, double OffsetMs)
{
    public static readonly TimeMap Identity = new(1, 0);
    public double Map(double sourceMs) => Scale * sourceMs + OffsetMs;
    public bool IsIdentity => Math.Abs(Scale - 1) < 1e-9 && Math.Abs(OffsetMs) < 0.5;
}

public sealed class ReferenceAlignment
{
    public required IReadOnlyList<string?> Texts { get; init; }
    public required int Matched { get; init; }
    public required int Total { get; init; }
    public required TimeMap Map { get; init; }
    public required string Method { get; init; }
    public int Rejected { get; init; }
    public double Coverage => Total == 0 ? 0 : (double)Matched / Total;

    public string Describe()
    {
        var shift = Map.IsIdentity ? string.Empty
            : $" (timing corrected: ×{Map.Scale:0.0000}, {Map.OffsetMs / 1000:+0.0;-0.0} s)";
        var rejected = Rejected > 0 ? $"; {Rejected} weak match(es) ignored" : string.Empty;
        return $"Reference matched {Matched} of {Total} cue(s) ({Coverage:P0}) by {Method}{shift}{rejected}.";
    }
}

public static class ReferenceAligner
{
    public const double MinimumUsefulCoverage = 0.6;
    public const double GlobalMatchSimilarity = 0.68;
    public const double GlobalMatchMargin = 0.08;
    public const double PairSimilarityFloor = 0.2;
    private const double ToleranceMs = 1500;
    private static readonly Regex Tags = new(@"<[^<>]*>|\{[^{}]*\}|\\[Nnh]", RegexOptions.Compiled);
    private static readonly Regex Spaces = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex LineDash = new(@"(?m)^\s*[-–—]\s*", RegexOptions.Compiled);
    // Hearing-impaired extras: "(sighs)", "[door closes]", "♪", and speaker labels such as "TERESA:" or "DEMARCUS (groans):".
    private static readonly Regex HearingImpaired = new(
        @"\([^()]*\)|\[[^\[\]]*\]|[♪♫*#]|(?m)(?<=^\s*|[-–—]\s*)\p{Lu}[\p{Lu}\p{N} .'-]*\p{Lu}\s*:(?!\d)", RegexOptions.Compiled);
    private static readonly Regex Letters = new(@"[\p{L}\p{N}]", RegexOptions.Compiled);
    private static readonly Regex EnglishWord = new(
        @"\b(?:the|you|your|and|is|are|what|i'm|it's|don't|that|this|have|was|with)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    static ReferenceAligner() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    public static List<SrtBlock> Load(string path)
    {
        var info = new FileInfo(path);
        if (info.Length > 20 * 1024 * 1024)
        {
            throw new InvalidDataException("The reference subtitle is larger than 20 MB.");
        }

        var cues = SubRipParser.Parse(Decode(File.ReadAllBytes(path)));
        if (cues.Count == 0)
        {
            throw new InvalidDataException("No SubRip cues were found in the reference file.");
        }

        return cues;
    }

    public static string Decode(byte[] bytes)
    {
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE) return Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF) return Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);
        var start = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
        try
        {
            return new UTF8Encoding(false, true).GetString(bytes, start, bytes.Length - start);
        }
        catch (DecoderFallbackException)
        {
            return Encoding.GetEncoding(1252).GetString(bytes);
        }
    }

    public static string Clean(string text) =>
        Spaces.Replace(LineDash.Replace(HearingImpaired.Replace(HearingImpaired.Replace(Tags.Replace(text, " "), " "), " "), " "), " ").Trim();

    public static bool LooksEnglish(IReadOnlyList<SrtBlock> reference)
    {
        var withText = reference.Where(c => Letters.IsMatch(c.Text)).ToList();
        return withText.Count > 0 && withText.Count(c => EnglishWord.IsMatch(Clean(c.Text))) >= withText.Count * 0.3;
    }

    /// <summary>
    /// Every source cue gets the text of the reference cues it overlaps on the (mapped) timeline.
    /// <paramref name="pairSimilarity"/> can veto a pair, e.g. a MiniLM check.
    /// </summary>
    public static ReferenceAlignment AlignByTiming(IReadOnlyList<SrtBlock> source, IReadOnlyList<SrtBlock> reference,
        TimeMap? map = null, Func<int, int, double>? pairSimilarity = null, string method = "timing",
        Func<double, double>? localOffset = null)
    {
        map ??= TimeMap.Identity;
        var order = Enumerable.Range(0, reference.Count)
            .Where(j => Letters.IsMatch(reference[j].Text) && reference[j].EndMs > reference[j].StartMs)
            .OrderBy(j => reference[j].StartMs).ToArray();
        var longest = order.Length == 0 ? 0 : order.Max(j => reference[j].EndMs - reference[j].StartMs);
        var texts = new string?[source.Count];
        int matched = 0, total = 0, rejected = 0;

        for (var i = 0; i < source.Count; i++)
        {
            if (!Letters.IsMatch(source[i].Text)) continue;
            total++;
            var shift = localOffset?.Invoke(source[i].StartMs) ?? 0;
            var start = map.Map(source[i].StartMs) + shift;
            var end = map.Map(source[i].EndMs) + shift;
            var duration = Math.Max(1, end - start);
            var first = LowerBound(order, reference, start - longest);
            var parts = new List<int>();
            for (var k = first; k < order.Length && reference[order[k]].StartMs < end; k++)
            {
                var r = reference[order[k]];
                var overlap = Math.Min(end, r.EndMs) - Math.Max(start, r.StartMs);
                var shorter = Math.Min(duration, r.EndMs - r.StartMs);
                if (overlap >= Math.Max(150, 0.3 * shorter)) parts.Add(order[k]);
            }

            if (parts.Count == 0) continue;
            if (pairSimilarity != null && parts.Max(j => pairSimilarity(i, j)) < PairSimilarityFloor)
            {
                rejected++;
                continue;
            }

            texts[i] = string.Join("\n", parts.Select(j => Clean(reference[j].Text)));
            matched++;
        }

        return new ReferenceAlignment { Texts = texts, Matched = matched, Total = total, Map = map, Method = method, Rejected = rejected };
    }

    private static int LowerBound(int[] order, IReadOnlyList<SrtBlock> reference, double value)
    {
        int low = 0, high = order.Length;
        while (low < high)
        {
            var mid = (low + high) / 2;
            if (reference[order[mid]].StartMs < value) low = mid + 1;
            else high = mid;
        }
        return low;
    }

    public static bool IsUsableForGlobalMatch(string text) => Letters.Matches(Clean(text)).Count >= 12;

    /// <summary>Mutually best, clearly separated translation pairs (source index, reference index, similarity).</summary>
    public static List<(int Source, int Reference, double Score)> MutualMatches(
        IReadOnlyList<float[]?> source, IReadOnlyList<float[]?> reference,
        IReadOnlyList<bool>? sourceUsable = null, IReadOnlyList<bool>? referenceUsable = null)
    {
        var left = Enumerable.Range(0, source.Count).Where(i => source[i] != null && (sourceUsable?[i] ?? true)).ToArray();
        var right = Enumerable.Range(0, reference.Count).Where(j => reference[j] != null && (referenceUsable?[j] ?? true)).ToArray();
        var result = new List<(int, int, double)>();
        if (left.Length < 2 || right.Length < 2) return result;

        var scores = new double[left.Length, right.Length];
        for (var a = 0; a < left.Length; a++)
            for (var b = 0; b < right.Length; b++)
                scores[a, b] = Dot(source[left[a]]!, reference[right[b]]!);

        for (var a = 0; a < left.Length; a++)
        {
            var (best, score, second) = Best(right.Length, b => scores[a, b]);
            if (score < GlobalMatchSimilarity || score - second < GlobalMatchMargin) continue;
            var (back, _, backSecond) = Best(left.Length, x => scores[x, best]);
            if (back != a || score - backSecond < GlobalMatchMargin) continue;
            result.Add((left[a], right[best], score));
        }

        return result;
    }

    private static (int Index, double Score, double Second) Best(int count, Func<int, double> score)
    {
        int index = -1;
        double best = double.NegativeInfinity, second = double.NegativeInfinity;
        for (var i = 0; i < count; i++)
        {
            var value = score(i);
            if (value > best)
            {
                second = best;
                best = value;
                index = i;
            }
            else if (value > second) second = value;
        }
        return (index, best, second);
    }

    public static double Dot(float[] a, float[] b)
    {
        if (a.Length != b.Length) throw new InvalidDataException("Embedding sizes differ.");
        double sum = 0;
        for (var i = 0; i < a.Length; i++) sum += a[i] * b[i];
        return sum;
    }

    /// <summary>
    /// Robust linear fit reference = scale × source + offset over translation matches (start times).
    /// Returns null when the matches do not agree on one timeline.
    /// </summary>
    public static TimeMap? FitTimeMap(IReadOnlyList<(double Source, double Reference)> matches)
    {
        if (matches.Count < 8) return null;
        var sampled = Enumerable.Range(0, Math.Min(64, matches.Count))
            .Select(k => matches[(int)Math.Round(k * (matches.Count - 1) / Math.Max(1.0, Math.Min(64, matches.Count) - 1))])
            .Distinct().ToArray();
        var candidates = new List<TimeMap> { TimeMap.Identity };
        foreach (var m in sampled) candidates.Add(new TimeMap(1, m.Reference - m.Source));
        for (var a = 0; a < sampled.Length; a++)
            for (var b = a + 1; b < sampled.Length; b++)
            {
                var distance = sampled[b].Source - sampled[a].Source;
                if (Math.Abs(distance) < 60_000) continue;
                var scale = (sampled[b].Reference - sampled[a].Reference) / distance;
                if (scale is < 0.8 or > 1.25) continue;
                candidates.Add(new TimeMap(scale, sampled[a].Reference - scale * sampled[a].Source));
            }

        var identityInliers = Inliers(TimeMap.Identity, matches).Count;
        var best = candidates
            .Select(map => (map, inliers: Inliers(map, matches)))
            .OrderByDescending(c => c.inliers.Count)
            .ThenBy(c => c.inliers.Count == 0 ? double.MaxValue : c.inliers.Average(m => Math.Abs(c.map.Map(m.Source) - m.Reference)))
            .First();
        if (best.inliers.Count < Math.Max(6, matches.Count * 0.6)) return null;
        // Identity explains (almost) as much: the files already share one timeline.
        if (identityInliers >= best.inliers.Count - Math.Max(1, matches.Count / 20)) return TimeMap.Identity;

        var refined = LeastSquares(best.inliers) ?? best.map;
        var final = Inliers(refined, matches);
        return final.Count >= best.inliers.Count ? refined : best.map;
    }

    private static List<(double Source, double Reference)> Inliers(TimeMap map, IReadOnlyList<(double Source, double Reference)> matches) =>
        matches.Where(m => Math.Abs(map.Map(m.Source) - m.Reference) <= ToleranceMs).ToList();

    private static TimeMap? LeastSquares(IReadOnlyList<(double Source, double Reference)> points)
    {
        var meanX = points.Average(p => p.Source);
        var meanY = points.Average(p => p.Reference);
        var sxx = points.Sum(p => (p.Source - meanX) * (p.Source - meanX));
        if (sxx <= 0) return new TimeMap(1, meanY - meanX);
        var scale = points.Sum(p => (p.Source - meanX) * (p.Reference - meanY)) / sxx;
        if (scale is < 0.8 or > 1.25) return null;
        // A rate difference smaller than cue-boundary noise over the whole span is just an offset.
        var span = points.Max(p => p.Source) - points.Min(p => p.Source);
        if (Math.Abs(scale - 1) * span < 500) return new TimeMap(1, meanY - meanX);
        return new TimeMap(scale, meanY - scale * meanX);
    }

    /// <summary>
    /// Timing first; MiniLM translation matches verify it and fix an offset or frame-rate difference,
    /// and pairs whose texts clearly differ are dropped.
    /// </summary>
    public static ReferenceAlignment AlignWithEmbeddings(IReadOnlyList<SrtBlock> source, IReadOnlyList<SrtBlock> reference,
        IReadOnlyList<float[]?> sourceVectors, IReadOnlyList<float[]?> referenceVectors)
    {
        var matches = MutualMatches(sourceVectors, referenceVectors,
            source.Select(b => IsUsableForGlobalMatch(b.Text)).ToList(),
            reference.Select(b => IsUsableForGlobalMatch(b.Text)).ToList());
        var points = matches.Select(m => ((double)source[m.Source].StartMs, (double)reference[m.Reference].StartMs)).ToList();
        var map = FitTimeMap(points);
        double Similarity(int i, int j) => sourceVectors[i] is { } a && referenceVectors[j] is { } b ? Dot(a, b) : 1;
        var method = points.Count < 8
            ? $"timing (MiniLM found only {matches.Count} translation match(es), too few to check the timing)"
            : $"timing checked with {matches.Count} MiniLM translation match(es)";
        return AlignByTiming(source, reference, map ?? TimeMap.Identity, Similarity, method,
            points.Count < 8 ? null : LocalOffsets(points, map ?? TimeMap.Identity));
    }

    /// <summary>
    /// What a single straight line misses - a cut scene or a drifting section - comes from the median residual of
    /// the translation matches near each cue.
    /// </summary>
    public static Func<double, double> LocalOffsets(IReadOnlyList<(double Source, double Reference)> matches, TimeMap map)
    {
        var residuals = matches.Select(m => (m.Source, Residual: m.Reference - map.Map(m.Source)))
            .OrderBy(m => m.Source).ToArray();
        return time =>
        {
            foreach (var window in new[] { 60_000.0, 180_000.0 })
            {
                var near = residuals.Where(r => Math.Abs(r.Source - time) <= window).Select(r => r.Residual).OrderBy(r => r).ToArray();
                if (near.Length >= 3)
                {
                    var median = near[near.Length / 2];
                    return Math.Abs(median) < 250 ? 0 : median;
                }
            }
            return 0;
        };
    }
}

/// <summary>
/// Lines to check by hand: the English addresses someone ("you/your") but the line still says
/// "você" or "seu/sua", which may be formal or may need "tu/teu". Nothing is changed automatically.
/// </summary>
public static class ReferenceReview
{
    private static readonly Regex Voce = new(@"(?<![\p{L}\p{N}])[Vv]ocê(?![\p{L}\p{N}])", RegexOptions.Compiled);
    private static readonly Regex Seu = new(@"(?<![\p{L}\p{N}])(?:[Ss]eus?|[Ss]uas?)(?![\p{L}\p{N}])", RegexOptions.Compiled);
    private static readonly Regex Formal = new(@"(?<![\p{L}\p{N}])(?:[Ss]enhora?s?|[Vv]ossa|[Dd]outora?|[Ss]r\.|[Ss]ra\.|[Ee]xcelência|[Mm]ajestade|[Aa]lteza|[Ss]enhoria|[Ee]minência|[Ss]antidade|[Mm]eritíssim[oa])(?![\p{L}\p{N}])", RegexOptions.Compiled);
    private static readonly Regex Tags = new(@"<[^<>]*>|\{[^{}]*\}", RegexOptions.Compiled);

    public static string? Note(string text, ReferenceHint? hint)
    {
        if (hint == null || hint.FormalAddress || !(hint.SecondPerson || hint.SecondPossessive)) return null;
        var plain = Tags.Replace(text, " ");
        if (Formal.IsMatch(plain)) return null;

        var kept = new List<string>();
        if (Voce.IsMatch(plain)) kept.Add("você");
        if (hint.SecondPossessive && !hint.ThirdPossessive && Seu.IsMatch(plain)) kept.Add("seu/sua");
        if (kept.Count == 0) return null;

        var english = hint.SecondPossessive && kept.Contains("seu/sua") ? "your" : "you";
        var informal = string.Join("/", kept.Select(k => k == "você" ? "tu" : "teu/tua"));
        return $"Review: the English says “{english}” and the line keeps {string.Join(" and ", kept)}. " +
               $"Keep it if formal; change to {informal} if informal.";
    }
}

/// <summary>
/// Grammar errors, including a missing infinitive ending after ir and ambiguous tense or ter constructions.
/// Checked on every line, changed or not. Nothing is changed here.
/// </summary>
public static class GrammarReview
{
    private static readonly Regex TerInfinitive = new(
        @"(?<![\p{L}\p{M}-])(?<ter>ter|tenho|tens|tem|temos|têm|tinha|tinhas|tínhamos|tinham|terá|teria|teriam)\s+(?<inf>\p{Ll}+(?:ar|er|ir)|pôr)(?![\p{L}\p{M}\d_-])",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly HashSet<string> Nouns = new(StringComparer.OrdinalIgnoreCase)
    {
        "lugar", "jantar", "almoçar", "poder", "prazer", "dever", "olhar", "par", "mar", "lar", "bar", "colher", "mulher", "talher",
        "qualquer", "sequer", "ar", "andar", "pesar", "saber", "ser", "haver", "ver", "existir",
    };

    private static readonly Dictionary<string, string> Participles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ser"] = "sido", ["fazer"] = "feito", ["dizer"] = "dito", ["ver"] = "visto", ["vir"] = "vindo", ["pôr"] = "posto",
        ["escrever"] = "escrito", ["abrir"] = "aberto", ["cobrir"] = "coberto", ["ganhar"] = "ganho", ["gastar"] = "gasto", ["pagar"] = "pago",
    };

    private static readonly Regex ExplicitSubject = new(
        @"(?:^|[.!?;]\s+)(?<subject>tu|nós|eles|elas)\s+(?:(?:não|já|sempre|nunca|também|ainda)\s+){0,2}(?<verb>\p{L}+)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    internal static readonly Dictionary<string, string[]> SingularPlural = new(StringComparer.OrdinalIgnoreCase)
    {
        ["sou"] = ["somos", "são"], ["é"] = ["somos", "são"], ["está"] = ["estamos", "estão"],
        ["estou"] = ["estamos", "estão"], ["tenho"] = ["temos", "têm"], ["tem"] = ["temos", "têm"],
        ["vai"] = ["vamos", "vão"], ["vou"] = ["vamos", "vão"], ["quer"] = ["queremos", "querem"],
        ["quero"] = ["queremos", "querem"], ["pode"] = ["podemos", "podem"], ["posso"] = ["podemos", "podem"],
        ["sabe"] = ["sabemos", "sabem"], ["sei"] = ["sabemos", "sabem"], ["faz"] = ["fazemos", "fazem"],
        ["faço"] = ["fazemos", "fazem"], ["diz"] = ["dizemos", "dizem"], ["digo"] = ["dizemos", "dizem"],
        ["foi"] = ["fomos", "foram"], ["estava"] = ["estávamos", "estavam"], ["tinha"] = ["tínhamos", "tinham"],
    };
    private static readonly Regex AttractedClitic = new(
        @"(?<!\p{L})(?:não|nunca|já|também|sempre)\s+(?<verb>\p{L}+)-(?<clitic>me|te|se|nos|vos|lhe|lhes|o|a|os|as)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex GenderMismatch = new(
        @"(?<!\p{L})(?:(?<f>a|uma|esta|essa|minha|tua)\s+(?<masc>frigorífico|casaco|autocarro|comboio|telemóvel|polícia)|" +
        @"(?<m>o|um|este|esse|meu|teu)\s+(?<fem>equipa|violação|carrinha))\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static string? Note(string text)
    {
        var plain = Regex.Replace(text, @"<[^<>]*>|\{[^{}]*\}", " ");
        var notes = new List<string>();
        foreach (Match m in ExplicitSubject.Matches(plain))
        {
            var subject = m.Groups["subject"].Value;
            var verb = m.Groups["verb"].Value;
            string? expected = null;
            if (subject.Equals("tu", StringComparison.OrdinalIgnoreCase))
            {
                if (!PortugueseVerbs.IsKnownInfinitive(verb) && PortugueseVerbs.TryThirdToSecond(verb, out var second))
                    expected = second;
            }
            else if (SingularPlural.TryGetValue(verb, out var plural))
                expected = plural[subject.Equals("nós", StringComparison.OrdinalIgnoreCase) ? 0 : 1];
            if (expected != null)
                notes.Add($"Review: explicit subject “{subject}” disagrees with “{verb}”; check “{expected}”.");
            else if (!subject.Equals("tu", StringComparison.OrdinalIgnoreCase) &&
                     !PortugueseVerbs.IsKnownInfinitive(verb) &&
                     (PortugueseVerbs.ThirdToSecond.ContainsKey(verb.ToLowerInvariant()) || PortugueseVerbs.IsSecondPersonForm(verb)))
                notes.Add($"Review: explicit plural subject “{subject}” with singular verb “{verb}”; check plural conjugation.");
        }
        foreach (Match m in AttractedClitic.Matches(plain))
        {
            var verb = m.Groups["verb"].Value;
            if (PortugueseVerbs.IsKnownVerbForm(verb) && !PortugueseVerbs.IsKnownInfinitive(verb))
                notes.Add($"Review: “{m.Value}” has an attracting word; check pronoun placement before the finite verb.");
        }
        foreach (Match m in GenderMismatch.Matches(plain))
        {
            // Polícia is common-gender when it refers to an officer.
            if (!m.Groups["masc"].Value.Equals("polícia", StringComparison.OrdinalIgnoreCase))
                notes.Add($"Review: determiner/noun gender agreement in “{m.Value}”.");
        }
        foreach (Match m in Regex.Matches(plain,
                     @"(?<!\p{L})(?<noun>frigoríficos?|casacos?|autocarros?|comboios?|telemóveis|telemóvel|equipas?|violações|violação|carrinhas?)\s+" +
                     @"(?:está|estão|estava|estavam|é|são|era|eram|ficou|ficaram)\s+" +
                     @"(?<adj>(?:velh|nov|vazi|chei|bonit|estragad|danificad|gast|pequen|cansad)[oa]s?)\b",
                     RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            var noun = m.Groups["noun"].Value.ToLowerInvariant();
            var adjective = m.Groups["adj"].Value.ToLowerInvariant();
            var feminine = noun.StartsWith("equipa", StringComparison.Ordinal) || noun.StartsWith("viola", StringComparison.Ordinal) ||
                           noun.StartsWith("carrinha", StringComparison.Ordinal);
            var plural = noun.EndsWith('s');
            var ending = (feminine ? "a" : "o") + (plural ? "s" : "");
            if (!adjective.EndsWith(ending, StringComparison.Ordinal) || adjective.EndsWith('s') != plural)
                notes.Add($"Review: noun/adjective agreement in “{m.Value}”.");
        }
        foreach (Match m in Regex.Matches(plain,
                     @"(?:^|[.!?]\s+)(?:eu\s+)?(?<verb>gosto|gostas|gostamos|gostam|preciso|precisas|precisamos|precisam)\s+" +
                     @"(?<object>o|a|os|as|um|uma|uns|umas)\s+(?!(?:pouco|bocado|bocadinho)\b)\p{L}+",
                     RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            notes.Add($"Review: “{m.Value}” appears to omit the preposition de required by the verb.");
        foreach (var (phrase, correction) in PortugueseConverter.MissingInfinitiveEnding.Find(plain))
        {
            notes.Add($"Review: “{phrase}” needs an infinitive after ir - use “{correction}”.");
        }

        if (PortugueseConverter.BareInfinitive.Find(plain) is { } bare)
        {
            notes.Add($"Review: “{bare.Infinitive}” is in the infinitive after the subject - conjugate it: " +
                      $"{bare.Past} (past) or {bare.Present} (present).");
        }

        foreach (Match m in TerInfinitive.Matches(plain))
        {
            var inf = m.Groups["inf"].Value;
            var ter = m.Groups["ter"].Value;
            if (Nouns.Contains(inf) && !(inf.Equals("ser", StringComparison.OrdinalIgnoreCase) && ter.Equals("ter", StringComparison.OrdinalIgnoreCase)))
                continue;
            if (!PortugueseVerbs.IsKnownInfinitive(inf) && !Participles.ContainsKey(inf)) continue;
            var participle = Participles.TryGetValue(inf, out var p) ? p
                : VerbConjugator.TryConjugate(inf.ToLowerInvariant(), out var forms) ? forms[35] : null;
            notes.Add($"Review: “{ter} {inf}” is not grammatical - probably “{ter} de {inf}”" +
                      (participle != null ? $" or “{ter} {participle}”." : "."));
            break;
        }

        return notes.Count == 0 ? null : string.Join("\n", notes);
    }
}

/// <summary>
/// English idioms in the reference that Brazilian subtitles often translate badly or literally: the note gives the
/// PT-PT equivalent, unless the line already has one of the expected words. Nothing is changed automatically.
/// </summary>
public static class IdiomHints
{
    private sealed record Idiom(Regex English, string Name, string PtPt, Regex Expected, string? Meaning);

    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled;
    private const string Own = @"(?:my|your|his|her|our|their|its|one's)";

    private static Idiom I(string english, string name, string ptPt, string expected, string? meaning = null) =>
        new(new Regex(@"\b(?:" + english + @")\b", Ci), name, ptPt,
            new Regex(@"(?<![\p{L}\p{M}])(?:" + expected + ")", Ci), meaning);

    private static readonly Idiom[] Idioms =
    {
        I(@"bust(?:s|ed|ing)?\s+" + Own + @"\s+(?:hump|ass|butt|back|tail)", "bust my hump",
            "dar cabo das costas / matar-se a trabalhar / fartar-se de trabalhar",
            @"cabo|mat\w*-(?:me|te|se|nos)|mat\w*\s+a\s+trabalhar|fart\w*|esfor[cç]\w*|suad\w*|suor", "work extremely hard"),
        I(@"bust(?:s|ed|ing)?\s+" + Own + @"\s+(?:chops|balls)|break(?:s|ing)?\s+" + Own + @"\s+balls", "bust my chops",
            "chatear / pegar com alguém / moer o juízo",
            @"chate\w*|peg\w*\s+com|mo\w*\s+o\s+juízo|impli\w*|goz\w*", "harass"),
        I(@"piece\s+of\s+cake", "piece of cake", "canja / pera doce / facílimo", @"canja|pera\s+doce|fácil|facílim\w*|simples", "very easy task"),
        I(@"break\s+a\s+leg", "break a leg", "boa sorte / muita merda (teatro)", @"sorte|merda", "good luck"),
        I(@"(?:cut|knock)\s+it\s+out|knock\s+it\s+off", "cut it out", "para com isso / chega / basta",
            @"par[ae]m?\s+com|pára|chega|basta|acaba\w*", "stop doing that"),
        I(@"beats\s+me", "beats me", "sei lá / não faço ideia", @"sei\s+lá|ideia|sei", "I do not know"),
        I(@"hang\s+in\s+there", "hang in there", "aguenta-te / aguenta firme", @"aguent\w*|for[cç]a|firme", "keep going despite difficulty"),
        I(@"(?:i'm|i\s+am)\s+all\s+ears", "I'm all ears", "sou todo ouvidos", @"ouvidos|ouvir|diz", "I am ready to listen"),
        I(@"on\s+the\s+house", "on the house", "é oferta da casa", @"casa|ofert\w*|grátis", "free of charge"),
        I(@"(?:cost|costs|costing)\s+(?:me\s+|you\s+|us\s+|him\s+|her\s+|them\s+)?an\s+arm\s+and\s+a\s+leg", "an arm and a leg",
            "custar os olhos da cara / uma fortuna", @"olhos\s+da\s+cara|fortuna|caríssim\w*|balúrdio|dinheirão", "be very expensive"),
        I(@"under\s+the\s+weather", "under the weather", "adoentado / indisposto", @"doente|indispost\w*|adoentad\w*|mal", "slightly ill"),
        I(@"spill(?:s|ed|ing)?\s+the\s+beans|let(?:s|ting)?\s+the\s+cat\s+out\s+of\s+the\s+bag", "spill the beans",
            "dar com a língua nos dentes / contar tudo",             @"língua|cont\w*|revel\w*|desembuch\w*|abr\w*\s+o\s+jogo", "reveal a secret"),
        I(@"pull(?:s|ed|ing)?\s+(?:my|your|his|her|our|their)\s+leg", "pull my leg", "estar a gozar / estar a brincar",
            @"goz\w*|brinc\w*|reinar", "tease"),
        I(@"kick(?:s|ed|ing)?\s+the\s+bucket", "kick the bucket", "bater a bota / esticar o pernil", @"bota|pernil|morr\w*|falec\w*", "die"),
        I(@"hit(?:s|ting)?\s+the\s+(?:sack|hay)", "hit the sack", "ir para a cama / ir dormir", @"cama|dormir|deitar", "go to bed"),
        I(@"call(?:s|ed|ing)?\s+it\s+a\s+(?:day|night)", "call it a day", "ficar por hoje / dar por encerrado",
            @"por\s+(?:hoje|aqui)|encerr\w*|termin\w*|acab\w*", "stop for today"),
        I(@"(?:not|n't)\s+(?:really\s+)?my\s+cup\s+of\s+tea", "not my cup of tea", "não é a minha praia / não é o meu género",
            @"praia|onda|género|estilo|gost\w*|chávena", "not something I like"),
        I(@"sleep\s+on\s+it", "sleep on it", "pensar melhor / consultar a almofada", @"almofada|pens\w*", "think about it before deciding"),
        I(@"(?:get|got|getting|have|having)\s+cold\s+feet", "cold feet", "acobardar-se / ficar com medo à última hora",
            @"medo|cobard\w*|acobard\w*|recu\w*|desist\w*|hesit\w*|arrepend\w*", "lose courage at the last moment"),
        I(@"cover(?:s|ed|ing)?\s+" + Own + @"\s+(?:ass|butt|tail)", "cover my ass", "safar a pele / proteger-se",
            @"pele|coiro|couro|saf\w*|proteg\w*|cobr\w*"),
        I(@"in\s+a\s+nutshell", "in a nutshell", "em suma / resumindo", @"suma|resum\w*|poucas\s+palavras", "in summary"),
        I(@"ball(?:'s|\s+is)\s+in\s+your\s+court", "the ball is in your court", "agora a decisão é tua / agora é contigo",
            @"decis\w*|contigo|consigo|depende|tua\s+vez|sua\s+vez", "it is your turn to decide"),
        I(@"my\s+bad", "my bad", "a culpa é minha / foi engano meu", @"culpa|desculp\w*|err\w*|engan\w*|falha", "it was my mistake"),
        I(@"hold\s+(?:your|ya)\s+horses", "hold your horses", "calma / tem calma", @"calma|devagar|espera\w*|acalm\w*", "wait and calm down"),
        I(@"get\s+a\s+grip", "get a grip", "controla-te / tem juízo", @"control\w*|juízo|calma|recomp\w*|acalm\w*", "control your emotions"),
        I(@"jump(?:s|ed|ing)?\s+the\s+gun", "jump the gun", "precipitar-se", @"precipit\w*|cedo|apress\w*", "act too early"),
        I(@"out\s+of\s+the\s+blue", "out of the blue", "do nada / de repente", @"nada|repente|súbit\w*|inesperad\w*", "unexpectedly"),
        I(@"rule\s+of\s+thumb", "rule of thumb", "regra geral", @"regra", "general practical guideline"),
        I(@"(?:the\s+)?last\s+straw", "the last straw", "a gota de água / foi a gota que fez transbordar o copo",
            @"gota|chega|limite|basta|copo", "final problem that exhausts patience"),
        I(@"blow(?:s|ing)?\s+off\s+(?:some\s+)?steam|blew\s+off\s+(?:some\s+)?steam", "blow off steam", "desanuviar / descomprimir / desabafar",
            @"desanuvi\w*|descompr\w*|desabaf\w*|relax\w*|vapor|espairec\w*", "release tension"),
        I(@"(?:on|walking\s+on)\s+thin\s+ice", "on thin ice", "em maus lençóis / a pisar terreno perigoso",
            @"lençóis|perig\w*|arrisc\w*|fino|cuidado", "in a risky situation"),
        I(@"bite\s+the\s+bullet", "bite the bullet", "aguentar / engolir em seco / enfrentar", @"aguent\w*|engol\w*|enfrent\w*|coragem", "face an unpleasant situation bravely"),
        I(@"(?:throw|throws|threw|throwing|thrown)\s+\w+\s+under\s+the\s+bus", "throw someone under the bus",
            "lixar alguém / atirar alguém às feras", @"feras|lix\w*|tram\w*|tra[ií]\w*|culp\w*|entreg\w*|sacrific\w*"),
        I(@"a\s+dime\s+a\s+dozen", "a dime a dozen", "há aos pontapés / há muitos", @"pontapé\w*|monte|muit\w*|vulgar\w*|comu\w*", "very common"),
        I(@"hit\s+the\s+road", "hit the road", "pôr-se a andar / ir embora", @"andar|embora|caminho|estrada|ir|vamos", "leave on a journey"),
        I(@"bend(?:s|ing)?\s+over\s+backwards|bent\s+over\s+backwards", "bend over backwards", "fazer de tudo / desdobrar-se",
            @"tudo|desdobr\w*|esfor[cç]\w*|imposs\w*", "make an exceptional effort"),
        I(@"by\s+the\s+book", "by the book", "segundo as regras / à risca", @"regra\w*|livro|norma\w*|procediment\w*|manual|risca", "according to the rules"),
        I(@"off\s+the\s+record", "off the record", "em off / extraoficialmente / cá entre nós",
            @"off|oficial\w*|entre\s+nós|confiden\w*|segredo", "confidentially"),
        I(@"(?:a|it's\s+a)\s+long\s+shot", "a long shot", "uma hipótese remota / um tiro no escuro",
            @"remot\w*|escuro|imprová\w*|difícil|arrisc\w*|hipótese", "an unlikely possibility"),
        I(@"(?:you|ya)\s+bet(?![\p{L}])(?!\s+(?:on|that|your|my|the|a|it))", "you bet", "podes crer / claro", @"crer|claro|dúvida|certeza|pois", "certainly"),
        I(@"(?:no|not\s+a|not\s+such\s+a)\s+big\s+deal", "no big deal", "não tem importância / não é nada de especial",
            @"importância|especial|nada|problema|grave|mal", "not important"),
        I(@"(?:keep|kept|keeping)\s+(?:your|my|his|her|our|their)\s+chin\s+up", "keep your chin up", "anima-te / cabeça erguida",
            @"anim\w*|erguid\w*|levantad\w*|força", "stay optimistic"),
        I(@"(?:got|have|has|had)\s+(?:your|my|his|her|our|their)\s+back", "I've got your back", "estou contigo / podes contar comigo",
            @"contigo|consigo|cont\w*\s+comigo|apoi\w*|protej\w*|proteg\w*|cobr\w*"),
        I(@"(?:break|breaks|broke|breaking)\s+the\s+ice", "break the ice", "quebrar o gelo", @"gelo", "ease initial social tension"),
        I(@"(?:i'm|i\s+am)\s+(?:absolutely\s+)?knackered", "I'm knackered", "estou de rastos / estafado",
            @"rastos|estafad\w*|exaust\w*|cansad\w*|esgotad\w*", "I am exhausted"),
        I(@"(?:i'm|i\s+am)\s+(?:totally\s+)?skint", "I'm skint", "estou sem dinheiro / teso",
            @"sem\s+dinheiro|tes[oa]|pobret\w*|falid\w*", "I have no money"),
        I(@"(?:i'm|i\s+am)\s+(?:(?:flat|completely|totally)\s+)?broke", "I'm broke", "estou sem dinheiro / teso",
            @"sem\s+dinheiro|tes[oa]|pobret\w*|falid\w*", "I have no money"),
        I(@"(?:i'm|i\s+am)\s+(?:totally\s+)?beat(?=\s*[.!…]?\s*$)", "I'm beat", "estou de rastos / exausto",
            @"rastos|exaust\w*|cansad\w*|estafad\w*", "I am exhausted"),
        I(@"chuffed(?:\s+to\s+bits)?", "chuffed", "muito contente / satisfeito",
            @"content\w*|feliz|felicíssim\w*|satisfeit\w*|orgulh\w*", "very pleased"),
        I(@"gobsmacked", "gobsmacked", "de boca aberta / estupefacto",
            @"boca\s+aberta|estupefa[ct]\w*|surpre\w*|espant\w*|pasma\w*", "astonished"),
        I(@"(?:tak(?:e|es|ing)|took|taken)\s+the\s+(?:mickey|piss)", "take the mickey / piss",
            "gozar / fazer pouco de alguém", @"goz\w*|tro[cç]\w*|brinc\w*|abus\w*|pouco", "make fun"),
        I(@"(?:do(?:es|ing)?|did)\s+" + Own + @"\s+head\s+in", "do my head in",
            "dar cabo da cabeça / irritar", @"cabeça|irrit\w*|chate\w*|enlouqu\w*", "cause frustration"),
        I(@"touch(?:es|ed|ing)?\s+base", "touch base", "entrar em contacto / falar brevemente",
            @"contact\w*|falar|convers\w*|ponto\s+d[ae]\s+situação", "communicate briefly"),
        I(@"(?:get|gets|got|getting)\s+the\s+ball\s+rolling", "get the ball rolling",
            "dar início / pôr mãos à obra", @"iníci\w*|come[cç]\w*|mãos\s+à\s+obra|arranc\w*", "start the activity"),
        I(@"on\s+the\s+same\s+page", "on the same page", "de acordo / alinhados",
            @"acordo|alinhad\w*|entend\w*|sintonia", "in agreement"),
        I(@"(?:learn|learns|learned|learnt|learning)\s+the\s+ropes", "learn the ropes",
            "aprender o ofício / ganhar prática", @"aprend\w*|prática|ofício|funcion\w*", "learn how things work"),
        I(@"pull(?:s|ed|ing)?\s+" + Own + @"\s+weight", "pull your weight",
            "fazer a sua parte / contribuir", @"parte|contribu\w*|colabor\w*|ajud\w*", "contribute fairly"),
        I(@"back\s+to\s+(?:square\s+one|the\s+drawing\s+board)", "back to square one",
            "voltar à estaca zero / recomeçar", @"estaca\s+zero|recome[cç]\w*|início|come[cç]\w*", "starting again"),
        I(@"in\s+the\s+loop", "in the loop", "a par / ao corrente",
            @"a\s+par|corrente|informad\w*|novidades", "informed about developments"),
        I(@"on\s+the\s+back\s+burner", "on the back burner", "em segundo plano / em banho-maria",
            @"segundo\s+plano|banho.maria|adiad\w*|prioridade|espera", "postponed for now"),
        I(@"ballpark\s+figure", "ballpark figure", "valor aproximado / estimativa",
            @"aproximad\w*|estimativ\w*|previsão", "rough estimate"),
        I(@"(?:pay|pays|paid|paying)\s+through\s+the\s+nose", "pay through the nose",
            "pagar os olhos da cara", @"olhos\s+da\s+cara|fortuna|car[oa]|exorbit\w*|balúrdio", "pay an excessive price"),
        I(@"spend(?:s|ing)?\s+a\s+penny", "spend a penny", "ir à casa de banho",
            @"casa\s+de\s+banho|urin\w*|xixi|mij\w*", "use the toilet"),
        I(@"bob's\s+your\s+uncle", "Bob's your uncle", "e pronto / e já está",
            @"pronto|já\s+está|feito|simples", "it is done"),
        I(@"(?:take|takes|taking|took|taken)\s+a\s+rain\s+check", "take a rain check",
            "ficar para outra vez / aceitar noutra ocasião", @"outra\s+(?:vez|ocasião)|noutra|adi\w*|depois", "postpone accepting the invitation"),
        I(@"bring(?:s|ing)?\s+home\s+the\s+bacon", "bring home the bacon", "sustentar a família / pôr comida na mesa",
            @"sustent\w*|comida|dinheiro|salário|ganh\w*", "earn money to support the family"),
        I(@"full\s+of\s+beans", "full of beans", "cheio de energia",
            @"energia|animad\w*|vivacidade|vigor", "very energetic"),
        I(@"(?:a\s+)?blessing\s+in\s+disguise", "a blessing in disguise", "há males que vêm por bem",
            @"males|bem|bênção|sorte|benefíci\w*|vantag\w*", "an apparent misfortune that turns out well"),
        I(@"(?:go|goes|going|went|gone)\s+pear.shaped", "go pear-shaped", "correr mal / dar para o torto",
            @"mal|torto|falh\w*|errad\w*", "go wrong"),
        I(@"in\s+a\s+pickle", "in a pickle", "num sarilho / numa alhada",
            @"sarilh\w*|alhada|problema|complicad\w*|dificuldade", "in a difficult situation"),
        I(@"make\s+ends\s+meet|makes\s+ends\s+meet|making\s+ends\s+meet|made\s+ends\s+meet", "make ends meet",
            "conseguir pagar as despesas / chegar ao fim do mês", @"despesas|fim\s+do\s+mês|contas|sustent\w*|dinheiro",
            "afford basic living expenses"),
        I(@"in\s+the\s+nick\s+of\s+time", "in the nick of time", "mesmo a tempo / no último instante",
            @"tempo|instante|hora|momento", "just before it was too late"),
        I(@"once\s+in\s+a\s+blue\s+moon", "once in a blue moon", "muito raramente / de longe a longe",
            @"rar\w*|longe\s+a\s+longe|quase\s+nunca|poucas\s+vezes", "very rarely"),
        I(@"(?:a\s+)?storm\s+in\s+a\s+teacup", "a storm in a teacup", "uma tempestade num copo de água",
            @"tempestade|exager\w*|drama", "an exaggerated reaction to a small problem"),
        I(@"bark(?:s|ed|ing)?\s+up\s+the\s+wrong\s+tree", "bark up the wrong tree", "estar enganado / procurar no sítio errado",
            @"engan\w*|errad\w*|equivoc\w*", "look in the wrong place for an answer"),
        I(@"(?:a\s+)?load\s+of\s+(?:old\s+)?(?:cobblers|pony|rubbish)", "a load of rubbish",
            "disparates / tretas", @"disparat\w*|treta\w*|asneir\w*|parvo\w*|porcaria|absurd\w*", "nonsense"),
        I(@"burn(?:s|ed|t|ing)?\s+the\s+midnight\s+oil", "burn the midnight oil", "trabalhar pela noite dentro",
            @"noite|madrugada|tarde|horas", "work late into the night"),
        I(@"(?:kill|kills|killed|killing)\s+two\s+birds\s+with\s+one\s+stone", "kill two birds with one stone",
            "matar dois coelhos de uma cajadada", @"coelhos|cajadada|dois\s+problemas|duas\s+coisas",
            "achieve two goals with one action"),
        I(@"(?:throw|throws|threw|throwing|thrown)\s+a\s+spanner\s+in\s+the\s+works", "throw a spanner in the works",
            "pôr entraves / estragar os planos", @"entrav\w*|estrag\w*|complic\w*|obstáculo|planos", "disrupt the plans"),
        I(@"(?:get|gets|got|getting)\s+(?:something|it|this|that)\s+off\s+" + Own + @"\s+chest", "get something off your chest",
            "desabafar / tirar um peso de cima", @"desabaf\w*|peso|peito|cont\w*|fal\w*", "talk about a worry"),
        I(@"all\s+mouth\s+and\s+no\s+trousers", "all mouth and no trousers", "muito paleio e pouca ação",
            @"paleio|fala\w*|ação|acção|conversa", "all talk and no action"),
        I(@"(?:a\s+)?dog's\s+breakfast", "a dog's breakfast", "uma trapalhada / uma confusão",
            @"trapalhad\w*|confus\w*|desastr\w*|caos", "a badly organized mess"),
        I(@"like\s+two\s+peas\s+in\s+a\s+pod", "like two peas in a pod", "como duas gotas de água",
            @"gotas\s+de\s+água|parecid\w*|igua\w*|semelhan\w*|inseparáve\w*", "very similar to each other"),
        I(@"(?:a\s+)?elephant\s+in\s+the\s+room|the\s+elephant\s+in\s+the\s+room", "the elephant in the room",
            "o assunto óbvio que todos evitam", @"elefante|assunto|problema|evit\w*|ignor\w*", "an obvious problem nobody discusses"),
        I(@"(?:have|has|having|had)\s+a\s+butcher's(?!\s+(?:shop|knife|hook|apron))", "have a butcher's",
            "dar uma vista de olhos", @"vista|olh\w*|espreit\w*|ver\b", "take a look"),
        I(@"(?:throw|throws|threw|throwing|thrown)\s+a\s+wobbly", "throw a wobbly",
            "fazer uma cena / ter um ataque de fúria", @"cena|fúria|furios\w*|birra|irrit\w*|calma", "have an angry outburst"),
        I(@"(?:have|has|having|had)\s+a\s+whale\s+of\s+a\s+time", "have a whale of a time",
            "divertir-se imenso", @"diverti\w*|espetacular|espectacular|maravilh\w*|ótim\w*", "have a wonderful time"),
        I(@"(?:throw|throws|threw|throwing|thrown)\s+in\s+the\s+towel", "throw in the towel",
            "atirar a toalha ao chão / desistir", @"toalha|desist\w*|rend\w*|abandon\w*", "give up"),
        I(@"(?:a\s+)?tough\s+nut\s+to\s+crack", "a tough nut to crack", "um osso duro de roer",
            @"osso|difícil|complicad\w*|duro", "a difficult problem to solve"),
        I(@"(?:a\s+)?wild\s+goose\s+chase", "a wild goose chase", "uma busca inútil",
            @"inútil|infrutífer\w*|vão|perda\s+de\s+tempo|sem\s+sentido", "a futile search"),
        I(@"on\s+" + Own + @"\s+tod", "on your tod", "sozinho / por conta própria",
            @"sozinh\w*|sós|conta\s+própria|só\b", "alone"),
    };

    internal static string? MeaningReference(string? english)
    {
        if (string.IsNullOrWhiteSpace(english)) return null;
        var plain = ReferenceAligner.Clean(english).Replace('’', '\'');
        var result = plain;
        foreach (var idiom in Idioms)
            if (idiom.Meaning != null)
                result = idiom.English.Replace(result, idiom.Meaning);
        return result == plain ? null : result;
    }

    public static string? Note(string text, string? english)
    {
        if (string.IsNullOrWhiteSpace(english)) return null;
        var plain = ReferenceAligner.Clean(english).Replace('’', '\'');
        var line = ReferenceAligner.Clean(text);
        var notes = Idioms.Where(i => i.English.IsMatch(plain) && !i.Expected.IsMatch(line))
            .Select(i => $"Hint: the English “{i.Name}” is an idiom - in PT-PT: {i.PtPt}.")
            .Distinct()
            .ToList();
        return notes.Count == 0 ? null : string.Join("\n", notes);
    }
}

/// <summary>Finds the English subtitle next to the subtitle or video, like Sub_sync: same name with a .en/.eng/.english tag.</summary>
public static class ReferenceFinder
{
    private static readonly string[] English = { "english", "eng", "en", "en-us", "en-gb", "en_us", "en_gb" };
    private static readonly string[] Tags =
    {
        "english", "eng", "en-us", "en-gb", "en_us", "en_gb", "en",
        "portuguese", "português", "brazilian", "por", "pob", "pt-br", "pt_br", "ptbr", "pt-pt", "pt_pt", "pt", "br",
        "hi", "sdh", "cc", "forced",
    };
    private static readonly Regex TrailingTag = new(
        @"[._ \-]+\[?(?<tag>" + string.Join("|", Tags.OrderByDescending(t => t.Length).Select(Regex.Escape)) + @")\]?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex Separators = new(@"[._\s\-\[\]()]+", RegexOptions.Compiled);
    private static readonly Regex Episode = new(@"^(.*?\bs\d{1,2} ?e\d{1,3})\b", RegexOptions.Compiled);

    /// <summary>Base name without language and HI/SDH tags (normalised), and whether a tag was English.</summary>
    public static (string Base, bool English) Parse(string path)
    {
        var stem = Path.GetFileNameWithoutExtension(path);
        var english = false;
        for (var i = 0; i < 3; i++)
        {
            var match = TrailingTag.Match(stem);
            if (!match.Success || match.Index == 0) break;
            english |= English.Contains(match.Groups["tag"].Value.ToLowerInvariant());
            stem = stem[..match.Index];
        }
        return (Separators.Replace(stem.ToLowerInvariant(), " ").Trim(), english);
    }

    /// <summary>
    /// An English .srt with the same name as the subtitle or video; failing that, the only English .srt
    /// of the same episode (e.g. "Show.S01E01.Pilot.English.srt" for "Show.S01E01.720p.pt-br.srt").
    /// </summary>
    public static string? Find(string? subtitlePath, string? videoPath)
    {
        var names = new[] { subtitlePath, videoPath }.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => Path.GetFullPath(p!)).ToList();
        if (names.Count == 0) return null;
        var bases = names.Select(p => Parse(p).Base).Where(b => b.Length > 0).ToHashSet();
        var episodes = bases.Select(EpisodeKey).Where(e => e != null).ToHashSet();

        var candidates = new List<(string Path, bool Exact)>();
        foreach (var folder in names.Select(Path.GetDirectoryName).Distinct().Where(Directory.Exists))
        {
            IEnumerable<string> files;
            try { files = Directory.EnumerateFiles(folder!, "*.srt").ToList(); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { continue; }
            foreach (var file in files)
            {
                if (names.Contains(Path.GetFullPath(file), StringComparer.Ordinal)) continue;
                var (name, english) = Parse(file);
                if (!english) continue;
                if (bases.Contains(name)) candidates.Add((file, true));
                else if (EpisodeKey(name) is { } key && episodes.Contains(key)) candidates.Add((file, false));
            }
        }

        var exact = candidates.Where(c => c.Exact).Select(c => c.Path).Order(StringComparer.OrdinalIgnoreCase).ToList();
        if (exact.Count > 0) return exact[0];
        var sameEpisode = candidates.Select(c => c.Path).Distinct().ToList();
        return sameEpisode.Count == 1 ? sameEpisode[0] : null;
    }

    private static string? EpisodeKey(string normalisedBase)
    {
        var match = Episode.Match(normalisedBase);
        return match.Success ? match.Groups[1].Value.Replace(" e", "e") : null;
    }
}
