using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class RelativeAndModalGrammarTests
{
    [Theory]
    [InlineData("Havia algo sobre o que esperava falar contigo.", "Havia algo de que esperava falar contigo.")]
    [InlineData("Há algo\nsobre o que falar.", "Há algo\nde que falar.")]
    [InlineData("Não há nada sobre o que possamos falar.", "Não há nada de que possamos falar.")]
    [InlineData("Uma coisa sobre o que eu queria falar.", "Uma coisa de que eu queria falar.")]
    [InlineData("A questão sobre o que falámos.", "A questão de que falámos.")]
    [InlineData("O assunto sobre o que falaste.", "O assunto de que falaste.")]
    [InlineData("HÁ ALGO SOBRE O QUE FALAR.", "HÁ ALGO DE QUE FALAR.")]
    [InlineData("<i>Há algo sobre o que falar.</i>", "<i>Há algo de que falar.</i>")]
    public void FalarTopicRelativesKeepTheAntecedent(string source, string expected) =>
        Check(source, null, expected);

    [Theory]
    [InlineData("Falámos sobre o que aconteceu.")]
    [InlineData("Quero saber sobre o que falaste.")]
    [InlineData("Há algo sobre o qual esperava falar contigo.")]
    [InlineData("Há algo de que esperava falar contigo.")]
    [InlineData("Há algo sobre o que aconteceu antes de falar.")]
    [InlineData("Há algo sobre o que escrever.")]
    [InlineData("Há algo. Sobre o que falar?")]
    [InlineData("Uma questão sobre o que é falar.")]
    public void FreeRelativesQuestionsAndOtherComplementsAreNotRewritten(string source) =>
        Assert.Equal(source, RelativeAndModalGrammar.TopicRelatives(source));

    [Theory]
    [InlineData("Sim. E ela falarei nisto\naté eu ceder.", "Yeah. And she'd bring it up until I relented.",
        "Sim. E ela falaria nisto\naté eu ceder.")]
    [InlineData("Ele falará contigo.", "He would talk to you.", "Ele falaria contigo.")]
    [InlineData("Ela dirá isso.", "She would say that.", "Ela diria isso.")]
    [InlineData("Eles farão isso.", "They would do that.", "Eles fariam isso.")]
    [InlineData("Nós traremos isso.", "We would bring it.", "Nós traríamos isso.")]
    [InlineData("Eu esperarei.", "I would wait.", "Eu esperaria.")]
    [InlineData("Tu esperarás.", "You would wait.", "Tu esperarias.")]
    [InlineData("Vocês esperarão.", "You would wait.", "Vocês esperariam.")]
    [InlineData("Ela não falarei.", "She wouldn't talk.", "Ela não falaria.")]
    [InlineData("Ela nunca dirá isso.", "She would never say that.", "Ela nunca diria isso.")]
    [InlineData("Ela manterei isso.", "She would keep that.", "Ela manteria isso.")]
    [InlineData("ELA FALAREI NISTO.", "She'd bring it up.", "ELA FALARIA NISTO.")]
    [InlineData("<i>Ela falarei nisto.</i>", "She'd bring it up.", "<i>Ela falaria nisto.</i>")]
    public void EnglishWouldRepairsTenseAndPerson(string source, string english, string expected) =>
        Check(source, english, expected);

    [Theory]
    [InlineData("Ela falarei.", null)]
    [InlineData("Ela falará.", "She will talk.")]
    [InlineData("Ela falará.", "He would talk.")]
    [InlineData("Ela falará.", "She wouldn't talk.")]
    [InlineData("Ela não falará.", "She would talk.")]
    [InlineData("Ela falará.", "She'd already left.")]
    [InlineData("Ela falará.", "She'd read it.")]
    [InlineData("Ela virá.", "She'd come home.")]
    [InlineData("Ela falará.", "She would rather leave.")]
    [InlineData("Ela falará.", "She could talk.")]
    [InlineData("Ela falará.", "She would talk but he will leave.")]
    [InlineData("Ela falará. Ele dirá isso.", "She would talk. He would say that.")]
    [InlineData("Ela falará. Ela dirá isso.", "She would talk.")]
    [InlineData("Ela falar-me-á.", "She would talk to me.")]
    [InlineData("A mulher falará.", "She would talk.")]
    [InlineData("Ela falava.", "She would talk.")]
    public void AmbiguousModalContextsAreNotRewritten(string source, string? english) =>
        Assert.Equal(source, RelativeAndModalGrammar.Conditionals(source, english));

    [Theory]
    [InlineData("Ela falarei.", true)]
    [InlineData("Ele dirás.", true)]
    [InlineData("Nós farei.", true)]
    [InlineData("Eu falarei.", false)]
    [InlineData("Ela falará.", false)]
    [InlineData("Ela falaria.", false)]
    [InlineData("Eles falariam.", false)]
    public void WrongFuturePersonIsReviewedWithoutGuessingTense(string text, bool review) =>
        Assert.Equal(review, GrammarReview.Note(text)?.Contains("future/conditional") == true);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ActualSplitCuesPreserveTextAndTimings(bool context)
    {
        var blocks = new[]
        {
            new SrtBlock { StartMs = 114548, EndMs = 116604, Text = "Na verdade, Danny,\nhavia algo" },
            new SrtBlock { StartMs = 116609, EndMs = 118344, Text = "sobre o que esperava\nfalar com você." },
            new SrtBlock { StartMs = 181723, EndMs = 185361, Text = "Sim. E ela falarei nisto\naté eu ceder." },
        };
        var originals = blocks.Select(b => (b.Text, b.StartMs, b.EndMs)).ToArray();
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(SentenceContext: context),
            english: new string?[] { "Actually, Danny, there was something",
                "I was hoping to talk to you about.", "Yeah. And she'd bring it up until I relented." });
        Assert.Equal(originals[0].Text, result[0].Text);
        Assert.Equal("de que esperava\nfalar contigo.", result[1].Text);
        Assert.Equal("Sim. E ela falaria nisto\naté eu ceder.", result[2].Text);
        if (context)
        {
            Assert.Contains(result[1].Rules, r => r.Id == "topic-relatives");
            Assert.Same(result[0].MeaningSentence, result[1].MeaningSentence);
            Assert.Contains("havia algo sobre o que", result[1].MeaningSentence!.Original);
            Assert.Contains("there was something I was hoping", result[1].MeaningSentence!.Reference);
        }
        else Assert.Null(result[1].MeaningSentence);
        Assert.Equal(originals, blocks.Select(b => (b.Text, b.StartMs, b.EndMs)).ToArray());
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public void SplitAntecedentsDoNotCrossUnsafeBoundaries(bool speakerChange, bool timeGap, bool selection)
    {
        var blocks = new[]
        {
            new SrtBlock { StartMs = 0, EndMs = 1000, Text = "Havia algo" },
            new SrtBlock { StartMs = timeGap ? 10000 : 1005, EndMs = 12000, Text = "sobre o que esperava falar contigo." },
        };
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(SentenceContext: true),
            selected: selection ? new HashSet<int> { 1 } : null,
            speakers: speakerChange ? new string?[] { "Ana", "João" } : null);
        Assert.Equal(blocks[1].Text, result[1].Text);
        Assert.Null(result[1].MeaningSentence);
    }

    [Fact]
    public void RuleSettingsAndIgnoredPhrasesWin()
    {
        const string relative = "Há algo sobre o que falar.";
        Assert.Equal(relative, new PortugueseConverter(new ConversionOptions(DisabledRules: "topic-relatives")).Convert(relative));
        Assert.Equal(relative, new PortugueseConverter(new ConversionOptions(Colloquial: false)).Convert(relative));
        var hint = ReferenceHint.FromText("She would talk.");
        const string modal = "Ela falarei.";
        Assert.Equal(modal, new PortugueseConverter(new ConversionOptions(DisabledRules: "reference-conditional"))
            .Convert(modal, reference: hint));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("ela falarei");
        words.Ignored.Add("algo sobre o que falar");
        Assert.Equal(modal, new PortugueseConverter(localWordList: words).Convert(modal, reference: hint));
        Assert.Equal(relative, new PortugueseConverter(localWordList: words).Convert(relative));
        Assert.Equal("Ela falaria.", new PortugueseConverter(new ConversionOptions(YouToTu: false))
            .Convert(modal, reference: hint! with { FormalAddress = true }));
    }

    [Fact]
    public void DisabledRelativeRuleStillGetsSentenceReview()
    {
        var blocks = new[]
        {
            new SrtBlock { StartMs = 0, EndMs = 1000, Text = "Havia algo" },
            new SrtBlock { StartMs = 1005, EndMs = 2000, Text = "sobre o que esperava falar contigo." },
        };
        var result = SubtitleConversion.Convert(blocks,
            new ConversionOptions(SentenceContext: true, DisabledRules: "topic-relatives"));
        Assert.Equal(blocks[1].Text, result[1].Text);
        Assert.Contains("explicit antecedent", result[1].ContextNote);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void PreviousCueDoesNotRequireEnglishAndHonorsIgnoredAntecedents(bool context, bool ignore)
    {
        var blocks = new[]
        {
            new SrtBlock { StartMs = 0, EndMs = 1000, Text = "Havia algo" },
            new SrtBlock { StartMs = 1005, EndMs = 2000, Text = "sobre o que esperava falar contigo." },
        };
        var words = new PortugueseLocalWordList();
        if (ignore) words.Ignored.Add("algo");
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(SentenceContext: context), words);
        Assert.Equal(ignore ? blocks[1].Text : "de que esperava falar contigo.", result[1].Text);
    }

    [Theory]
    [InlineData("Havia algo.", "sobre o que esperava falar contigo.")]
    [InlineData("Havia algo", "Sobre o que esperava falar contigo?")]
    [InlineData("<i>Havia algo</i>", "sobre o que esperava falar contigo.")]
    [InlineData("ANA: Havia algo", "JOÃO: sobre o que esperava falar contigo.")]
    public void PreviousCueNeedsAnUnbrokenRelativeContinuation(string previous, string text)
    {
        var result = SubtitleConversion.Convert(new[]
        {
            new SrtBlock { StartMs = 0, EndMs = 1000, Text = previous },
            new SrtBlock { StartMs = 1005, EndMs = 2000, Text = text },
        }, new ConversionOptions());
        Assert.Equal(text, result[1].Text);
    }

    [Theory]
    [InlineData(.8f, .8f, false)]
    [InlineData(.2f, .2f, true)]
    [InlineData(.8f, .75f, true)]
    [InlineData(.8f, .78f, false)]
    [InlineData(.4501f, .4501f, false)]
    [InlineData(.4499f, .4499f, true)]
    [InlineData(.5999f, .54f, false)]
    [InlineData(.6001f, .54f, true)]
    [InlineData(.8f, .7601f, false)]
    [InlineData(.8f, .7599f, true)]
    public async Task SentenceMeaningUsesActualCombinedTextAndExistingThresholds(float before, float after, bool flagged)
    {
        var sentence = new SentenceMeaning(35, 36, "Havia algo sobre o que falar.", "There was something to talk about.");
        const string converted = "Havia algo de que falar.";
        var seen = new List<string?>();
        Task<float[]?> Embed(string? text)
        {
            seen.Add(text);
            return Task.FromResult<float[]?>(text == sentence.Reference
                ? new[] { 1f, 0f, 0f }
                : new[] { text == sentence.Original ? before : after,
                    MathF.Sqrt(1 - MathF.Pow(text == sentence.Original ? before : after, 2)), 0f });
        }
        var note = await LocalChecks.SentenceMeaningNoteAsync(sentence, converted, Embed);
        Assert.Equal(flagged, note != null);
        Assert.Equal(new[] { sentence.Original, converted, sentence.Reference }, seen);
        if (flagged)
        {
            Assert.Contains("cues 36-37", note);
            Assert.Contains("does not validate relative pronouns or verb agreement", note);
        }
    }

    [Fact]
    public async Task SentenceMeaningCanFlagOriginalChangeWithoutEnglishAndDoesNotHideFailures()
    {
        var sentence = new SentenceMeaning(0, 1, "Havia algo para falar.", null);
        var calls = 0;
        Task<float[]?> Embed(string? text)
        {
            calls++;
            return Task.FromResult<float[]?>(text == sentence.Original ? new[] { 1f, 0f } : new[] { .69f, .7f });
        }
        Assert.NotNull(await LocalChecks.SentenceMeaningNoteAsync(sentence, "Não havia nada para falar.", Embed));
        Assert.Equal(2, calls);
        await Assert.ThrowsAsync<IOException>(() => LocalChecks.SentenceMeaningNoteAsync(sentence, "Mudou.",
            _ => throw new IOException("Embedding failed.")));
        Assert.Null(await LocalChecks.SentenceMeaningNoteAsync(sentence, sentence.Original,
            _ => throw new InvalidOperationException("Unchanged unreferenced text needs no scoring.")));
    }

    [Fact]
    public async Task UnchangedSentenceStillChecksEnglishAndReusesOriginalEmbedding()
    {
        var sentence = new SentenceMeaning(0, 1, "Há algo para falar.", "There is something to talk about.");
        var calls = 0;
        Task<float[]?> Embed(string? text)
        {
            calls++;
            return Task.FromResult<float[]?>(text == sentence.Reference ? new[] { 1f, 0f } : new[] { .3f, .95f });
        }
        Assert.NotNull(await LocalChecks.SentenceMeaningNoteAsync(sentence, sentence.Original, Embed));
        Assert.Equal(2, calls);
    }

    private static void Check(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter();
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: hint));
        Assert.Equal(expected, converter.Convert(expected, reference: hint));
    }
}
