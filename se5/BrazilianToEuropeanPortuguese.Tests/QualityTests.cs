using System.Text.Json;
using System.Text.RegularExpressions;
using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class QualityTests
{
    private static SrtBlock Cue(string text, long start = 0, long end = 4000) => new() { Text = text, StartMs = start, EndMs = end };

    [Fact]
    public void SettingsRoundTripAndOldSettingsPreserveConservativeDefaults()
    {
        var options = new ConversionOptions(Address: AddressMode.Formal, SentenceContext: true, Rewrap: true,
            LineLength: 38, MaxLines: 3, CharactersPerSecond: 18, DisabledRules: "address,word-list");
        var request = new PluginRequest { SettingsVersion = ConversionOptions.Version, Settings = options.ToJson() };
        Assert.Equal(options, ConversionOptions.Load(request));
        request.Settings = JsonSerializer.SerializeToElement(new { spelling = true });
        Assert.Equal(ConversionOptions.Default, ConversionOptions.Load(request));
    }

    [Theory]
    [InlineData("""{"address":"invalid"}""")]
    [InlineData("""{"lineLength":0}""")]
    [InlineData("""{"maxLines":"two"}""")]
    [InlineData("""{"charactersPerSecond":100}""")]
    [InlineData("""{"sentenceContext":"yes"}""")]
    [InlineData("""{"rewrap":null}""")]
    [InlineData("""{"disabledRules":[]}""")]
    public void InvalidNewSettingsAreExplicitErrors(string json) =>
        Assert.Throws<InvalidDataException>(() => ConversionOptions.Load(new PluginRequest
        {
            SettingsVersion = ConversionOptions.Version, Settings = JsonDocument.Parse(json).RootElement.Clone(),
        }));

    [Fact]
    public void AddressOverrideAppliesAcrossCuesWithoutReference()
    {
        var blocks = new[] { Cue("Você está bem?"), Cue("Certo, veja.", 4500, 6000) };
        var formal = SubtitleConversion.Convert(blocks, new ConversionOptions(Address: AddressMode.Formal));
        Assert.Equal("Está bem?", formal[0].Text);
        Assert.Equal("Certo, veja.", formal[1].Text);
        var informal = SubtitleConversion.Convert(new[] { Cue("Senhor, você está bem?") },
            new ConversionOptions(Address: AddressMode.Informal));
        Assert.Contains("estás", informal[0].Text);
    }

    [Fact]
    public void RegisterStopsAtSceneGapsAndExplicitSpeakerChanges()
    {
        var blocks = new[] { Cue("Senhor, espere."), Cue("Certo, veja.", 11000, 15000) };
        Assert.False(SubtitleConversion.Convert(blocks, new ConversionOptions())[1].Hint?.FormalAddress == true);
        Assert.Equal("Certo, vê.", SubtitleConversion.Convert(blocks, new ConversionOptions())[1].Text);
        blocks = new[] { Cue("JOÃO: Senhor, espere."), Cue("MARIA: Certo, veja.", 4100, 8000) };
        Assert.True(DialogueRegister.DifferentSpeaker(blocks[0].Text, blocks[1].Text));
        Assert.False(SubtitleConversion.Convert(blocks, new ConversionOptions())[1].Hint?.FormalAddress == true);
    }

    [Fact]
    public void SpeakerIdentityDoesNotLeakAcrossUnlabelledCues()
    {
        var blocks = new[] { Cue("JOÃO: Senhor, espere."), Cue("Não sei.", 4100, 6000), Cue("MARIA: Certo, veja.", 6100, 9000) };
        Assert.False(DialogueRegister.IsFormal(blocks, 2));
    }

    [Fact]
    public void SubtitleActorMetadataBoundsRegisterAndSentenceContext()
    {
        var blocks = new[] { Cue("Senhor, espere."), Cue("Não sei.", 4100, 6000), Cue("Certo, veja.", 6100, 9000) };
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(), speakers: new string?[] { "A", "A", "B" });
        Assert.False(result[2].Hint?.FormalAddress == true);
        Assert.Equal("Certo, vê.", result[2].Text);
        result = SubtitleConversion.Convert(new[] { Cue("Não vou"), Cue("fala mais.", 4100, 8000) },
            new ConversionOptions(SentenceContext: true), speakers: new string?[] { "A", "B" });
        Assert.All(result, r => Assert.Equal(-1, r.SentenceGroup));
    }

    [Fact]
    public void MismatchedSpeakerMetadataIsReportedNotSilentlyApplied()
    {
        var request = new PluginRequest
        {
            Subtitle = new PluginSubtitle { Paragraphs = new List<PluginParagraph> { new() { Actor = "A", StartMs = 100, EndMs = 500 } } },
        };
        Assert.Null(SubtitleConversion.Speakers(request, new[] { Cue("Senhor, espere.") }, out var note));
        Assert.Contains("ignored", note);
    }

    [Fact]
    public void ConflictingRegisterWithholdsAddressChangesAndFlagsReview()
    {
        var result = SubtitleConversion.Convert(new[] { Cue("Senhor, tu sabes que você está bem.") }, new ConversionOptions())[0];
        Assert.Contains("você", result.Text);
        Assert.Contains("conflict", result.ContextNote);
        Assert.DoesNotContain(result.Rules, r => r.Id == "address");
    }

    [Fact]
    public void SentenceConversionRepairsCrossCueInfinitiveAndPreservesTimings()
    {
        var blocks = new[] { Cue("Não vou", 100, 2000), Cue("fala mais.", 2100, 4000) };
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(SentenceContext: true, YouToTu: false));
        Assert.Equal("Não vou", result[0].Text);
        Assert.Equal("falar mais.", result[1].Text);
        Assert.Equal(result[0].SentenceGroup, result[1].SentenceGroup);
        Assert.Equal(0, result[0].SentenceGroup);
        Assert.Equal((100L, 2000L, 2100L, 4000L), (blocks[0].StartMs, blocks[0].EndMs, blocks[1].StartMs, blocks[1].EndMs));
        Assert.Contains(result[1].Rules, r => r.Id == "missing-infinitive");
    }

    [Theory]
    [InlineData("Não vou.", "fala mais.", 4100)]
    [InlineData("Não vou", "Fala mais.", 4100)]
    [InlineData("Não vou", "- fala mais.", 4100)]
    [InlineData("<i>Não vou</i>", "fala mais.", 4100)]
    [InlineData("Não vou", "fala mais.", 11000)]
    public void UnsafeSentenceBoundariesAreNotJoined(string first, string second, long start)
    {
        var result = SubtitleConversion.Convert(new[] { Cue(first), Cue(second, start, start + 4000) },
            new ConversionOptions(SentenceContext: true));
        Assert.All(result, r => Assert.Equal(-1, r.SentenceGroup));
    }

    [Fact]
    public void SentenceConversionDoesNotTouchUnselectedCues()
    {
        var result = SubtitleConversion.Convert(new[] { Cue("Não vou"), Cue("fala mais.", 4100, 8000) },
            new ConversionOptions(SentenceContext: true), selected: new HashSet<int> { 0 });
        Assert.Equal("fala mais.", result[1].Text);
        Assert.All(result, r => Assert.Equal(-1, r.SentenceGroup));
    }

    [Fact]
    public void SentenceMappingKeepsOriginalInternalLineBreaksAndNonEmptyCues()
    {
        var result = SubtitleConversion.Convert(new[] { Cue("Não vou\nmesmo"), Cue("fala com vocês.", 4100, 8000) },
            new ConversionOptions(SentenceContext: true, YouToTu: false));
        Assert.Contains('\n', result[0].Text);
        Assert.Contains("falar", result[1].Text);
        Assert.All(result, r => Assert.False(string.IsNullOrWhiteSpace(r.Text)));
    }

    [Fact]
    public void SentencePassPreservesPerCueContextCorrectionsAndAmbiguousOutputs()
    {
        var result = SubtitleConversion.Convert(new[]
        {
            Cue("A gente sempre escuta,"), Cue("mas nunca acha que vai acontecer contigo.", 4100, 8000),
        }, new ConversionOptions(SentenceContext: true));
        Assert.Contains("nunca achamos", result[1].Text);
        result = SubtitleConversion.Convert(new[]
        {
            Cue("Ele usa um terno"), Cue("azul.", 4100, 8000),
        }, new ConversionOptions(SentenceContext: true));
        Assert.Contains("fato", string.Join(" ", result.Select(r => r.Text)));
        Assert.DoesNotContain("facto", string.Join(" ", result.Select(r => r.Text)));
    }

    [Fact]
    public void TraceStagesCanBeDisabledWithoutDisablingOtherStages()
    {
        var options = new ConversionOptions(DisabledRules: "word-list");
        var result = SubtitleConversion.Convert(new[] { Cue("Você está no trem.") }, options)[0];
        Assert.Contains("trem", result.Text);
        Assert.DoesNotContain("Você", result.Text);
        Assert.Contains(result.Rules, r => r.Id == "address");
        Assert.DoesNotContain(result.Rules, r => r.Id == "word-list");
    }

    [Fact]
    public void RewrapMeetsConfiguredLimitsAndIsStable()
    {
        var options = new ConversionOptions(Rewrap: true, LineLength: 30);
        var text = "Vou falar com o diretor amanhã de manhã.";
        var wrapped = SubtitleLayout.Rewrap(text, options);
        Assert.NotEqual(text, wrapped);
        Assert.All(wrapped.Split('\n'), line => Assert.InRange(line.Length, 1, 30));
        Assert.InRange(wrapped.Split('\n').Length, 1, 2);
        Assert.Equal(text, wrapped.Replace('\n', ' '));
        Assert.Equal(wrapped, SubtitleLayout.Rewrap(wrapped, options));
        Assert.DoesNotMatch(@"(?:\bo|\bcom|\bde|\bVou)\n", wrapped);
    }

    [Theory]
    [InlineData("<i>Vou falar com o diretor amanhã de manhã.</i>")]
    [InlineData("- Vou falar com o diretor.\n- Espera um momento.")]
    [InlineData(@"{\an8}Vou falar com o diretor amanhã de manhã.")]
    public void RewrapPreservesFormattingAndSpeakerMarkers(string text) =>
        Assert.Equal(text, SubtitleLayout.Rewrap(text, new ConversionOptions(Rewrap: true, LineLength: 20)));

    [Fact]
    public void InfeasibleLayoutIsKeptAndFlagged()
    {
        const string text = "Esta frase é demasiado comprida para caber em duas linhas de vinte caracteres.";
        var options = new ConversionOptions(Rewrap: true, LineLength: 20, LayoutWarnings: true);
        Assert.Equal(text, SubtitleLayout.Rewrap(text, options));
        Assert.Contains("exceeds 20", SubtitleLayout.Review(text, Cue(text), options));
    }

    [Fact]
    public void ReadingSpeedUsesDurationAndExactThreshold()
    {
        var options = new ConversionOptions(CharactersPerSecond: 20, LayoutWarnings: true);
        Assert.Null(SubtitleLayout.Review("12345678901234567890", Cue("", end: 1000), options));
        Assert.Contains("21.0", SubtitleLayout.Review("123456789012345678901", Cue("", end: 1000), options));
        Assert.Contains("zero or negative", SubtitleLayout.Review("Olá.", Cue("", end: 0), options));
    }

    [Fact]
    public void VisibleCharacterWarningsDecodeEntitiesAndCountGraphemes()
    {
        var options = new ConversionOptions(CharactersPerSecond: 5, LayoutWarnings: true);
        Assert.Null(SubtitleLayout.Review("&amp;&amp;&amp;&amp;&amp;", Cue("", end: 1000), options));
        Assert.Contains("6.0", SubtitleLayout.Review("😀😀😀😀😀😀", Cue("", end: 1000), options));
    }

    [Fact]
    public void RewrapAvoidsNameAndAuxiliarySplitsAndSupportsThreeLines()
    {
        var options = new ConversionOptions(Rewrap: true, LineLength: 22, MaxLines: 3);
        var names = SubtitleLayout.Rewrap("O John Smith chegou a casa cedo.", options);
        Assert.DoesNotContain("John\nSmith", names);
        var auxiliary = SubtitleLayout.Rewrap("Vou falar com a diretora amanhã.", options);
        Assert.DoesNotContain("Vou\nfalar", auxiliary);
        var three = SubtitleLayout.Rewrap("Este texto tem muitas palavras para caber em três linhas.", options);
        Assert.Equal(3, three.Split('\n').Length);
        Assert.All(three.Split('\n'), line => Assert.InRange(line.Length, 1, 22));
    }

    [Fact]
    public void FindingTypesDistinguishRulesModelsReviewsAndManualEdits()
    {
        var rules = new ChangeProposal(0, "A", "B", rules: new[] { new RuleChange("word-list", "Vocabulary") });
        Assert.Equal(FindingType.RuleBased, rules.FindingTypes);
        var mini = new ChangeProposal(1, "A", "A", reviewNote: "MiniLM (before the rules): check it.");
        Assert.Equal(FindingType.MiniLm, mini.FindingTypes);
        var judge = new ChangeProposal(2, "A", "B", reviewNote: "Gervásio chose an alternative.",
            rules: new[] { new RuleChange("local-judge", "Model choice") });
        Assert.Equal(FindingType.Judge, judge.FindingTypes);
        var combined = new ChangeProposal(3, "A", "B",
            reviewNote: "MiniLM: check it.\nGervásio chose an alternative.\nGrammar: check agreement.",
            rules: new[] { new RuleChange("word-list", "Vocabulary") });
        Assert.Equal(FindingType.RuleBased | FindingType.MiniLm | FindingType.Judge | FindingType.Grammar,
            combined.FindingTypes);
        var grammar = new ChangeProposal(6, "A", "A", reviewNote: "LanguageTool [TEST]: check grammar.");
        Assert.Equal(FindingType.Grammar, grammar.FindingTypes);
        var corrected = new ChangeProposal(7, "A", "B", rules: new[] { new RuleChange("languagetool", "Grammar correction") });
        Assert.Equal(FindingType.RuleBased | FindingType.Grammar, corrected.FindingTypes);
        var mixed = new ChangeProposal(8, "A", "A", reviewNote: "LanguageTool [TEST]: check grammar.\nReview this wording.");
        Assert.Equal(FindingType.Grammar | FindingType.OtherReview, mixed.FindingTypes);
        mini.ConvertedText = "Manual correction";
        Assert.Equal(FindingType.MiniLm | FindingType.ManualEdit, mini.FindingTypes);
        mini.ConvertedText = "A";
        Assert.Equal(FindingType.MiniLm, mini.FindingTypes);
        var layout = new ChangeProposal(4, "A", "A", wrappedText: "A\n", languageLayoutNote: "Layout: long line.");
        Assert.Equal(FindingType.RuleBased, layout.FindingTypes);
        layout.IncludeLayout = false;
        Assert.Equal(FindingType.RuleBased | FindingType.OtherReview, layout.FindingTypes);
        Assert.True(new ChangeProposal(5, "A", "A").VisibleInReview);
    }

    [Fact]
    public void LayoutCanBeRejectedWithoutRejectingLanguage()
    {
        var proposal = new ChangeProposal(0, "Estou no trem.", "Estou no comboio.", wrappedText: "Estou no\ncomboio.");
        Assert.True(proposal.HasLayoutChange);
        proposal.IncludeLayout = false;
        Assert.Equal("Estou no comboio.", proposal.ConvertedText);
        Assert.True(proposal.Include);
    }

    [Fact]
    public void RejectingLayoutRefreshesVisibleWarnings()
    {
        var proposal = new ChangeProposal(0, "Original.", "Texto sem quebra.", wrappedText: "Texto sem\nquebra.",
            wrappedLayoutNote: null, languageLayoutNote: "Layout: long line.");
        Assert.False(proposal.HasReview);
        proposal.IncludeLayout = false;
        Assert.Contains("long line", proposal.ReviewNote);
        Assert.True(proposal.HasReview);
    }

    [Fact]
    public void ExportIsStructuredAndDoesNotIncludeReferenceOrFileMetadata()
    {
        var proposal = new ChangeProposal(0, "Estou no trem.", "Estou no comboio.", "English secret reference",
            rules: new[] { new RuleChange("word-list", "Vocabulary") });
        proposal.ExpectedText = "Estou no comboio!";
        var exported = CorrectionExport.Create(new[] { proposal });
        Assert.Equal(1, exported.GetProperty("schemaVersion").GetInt32());
        var test = exported.GetProperty("cases")[0];
        Assert.Equal("Estou no comboio!", test.GetProperty("expected").GetString());
        Assert.Equal(new[] { "original", "expected", "rules", "sentenceGroup" }, test.EnumerateObject().Select(p => p.Name));
        Assert.DoesNotContain("English secret", exported.GetRawText());
    }

    [Theory]
    [InlineData("Nós está aqui.", "estamos")]
    [InlineData("Eles falas muito.", "plural")]
    [InlineData("Tu sabe a resposta.", "sabes")]
    [InlineData("Não conheço-te.", "pronoun placement")]
    [InlineData("Uma casaco velho.", "gender agreement")]
    [InlineData("A equipa está cansado.", "noun/adjective")]
    [InlineData("O casaco está velha.", "noun/adjective")]
    [InlineData("Eu gosto o chocolate.", "preposition de")]
    public void FinalGrammarReviewDetectsAgreementCliticsAndGovernment(string text, string? fragment)
    {
        if (fragment == null) return;
        Assert.Contains(fragment, GrammarReview.Note(text));
    }

    [Theory]
    [InlineData("Nós estamos aqui.")]
    [InlineData("Eles falam muito.")]
    [InlineData("Tu sabes a resposta.")]
    [InlineData("Não te conheço.")]
    [InlineData("Um casaco velho.")]
    [InlineData("A equipa está cheia.")]
    [InlineData("A polícia está aqui.")]
    [InlineData("O casaco está velho.")]
    [InlineData("Eu gosto do chocolate.")]
    [InlineData("Gosto um pouco de chocolate.")]
    [InlineData("É preciso um plano.")]
    [InlineData("Se tu falares, ouvimos.")]
    public void FinalGrammarReviewPreservesValidCounterexamples(string text) => Assert.Null(GrammarReview.Note(text));

    public static IEnumerable<object[]> BenchmarkCases()
    {
        yield return new object[] { "police", "Vamos pegar este cara.", "Vamos apanhar este tipo." };
        yield return new object[] { "police", "Ele é um tira.", "Ele é um polícia." };
        yield return new object[] { "police", "Ele foi pego.", "Ele foi apanhado." };
        yield return new object[] { "police", "Foi um sequestro.", "Foi um rapto." };
        yield return new object[] { "police", "Não fala.", "Não fales." };
        yield return new object[] { "family", "Minha geladeira está vazia.", "O meu frigorífico está vazio." };
        yield return new object[] { "family", "Papai está trabalhando.", "Pai está a trabalhar." };
        yield return new object[] { "family", "O café da manhã está pronto.", "O pequeno-almoço está pronto." };
        yield return new object[] { "family", "Gosto do suco.", "Gosto do sumo." };
        yield return new object[] { "family", "Estou com saudade.", "Tenho saudades." };
        yield return new object[] { "family", "Vou te ajudar.", "Vou ajudar-te." };
        yield return new object[] { "family", "Você tem razão.", "Tens razão." };
        yield return new object[] { "family", "Você se lembra?", "Lembras-te?" };
        yield return new object[] { "comedy", "Que cara de pau!", "Que descaramento!" };
        yield return new object[] { "comedy", "Que saco!", "Que seca!" };
        yield return new object[] { "comedy", "Legal!", "Fixe!" };
        yield return new object[] { "comedy", "Sério?", "A sério?" };
        yield return new object[] { "comedy", "Tá bom.", "Está bem." };
        yield return new object[] { "comedy", "Oi!", "Olá!" };
        yield return new object[] { "formal", "Doutor, você está bem?", "Doutor, está bem?" };
        yield return new object[] { "formal", "Senhor, isto é para você.", "Senhor, isto é para si." };
        yield return new object[] { "formal", "Detetive, posso falar com você?", "Detetive, posso falar consigo?" };
        yield return new object[] { "formal", "Doutora, não se preocupe.", "Doutora, não se preocupe." };
        yield return new object[] { "formal", "A senhora está bem?", "A senhora está bem?" };
        yield return new object[] { "preservation", "Não te conheço.", "Não te conheço." };
        yield return new object[] { "preservation", "O comboio chegou.", "O comboio chegou." };
        yield return new object[] { "preservation", "A equipa está pronta.", "A equipa está pronta." };
        yield return new object[] { "preservation", "Ela tira o casaco.", "Ela tira o casaco." };
        yield return new object[] { "preservation", "O frigorífico está vazio.", "O frigorífico está vazio." };
        yield return new object[] { "preservation", "Nunca nos esquecemos.", "Nunca nos esquecemos." };
    }

    [Theory]
    [MemberData(nameof(BenchmarkCases))]
    public void CrossDomainBenchmarkHasNoWrongOrMissedChangesAndIsIdempotent(string domain, string source, string expected)
    {
        Assert.NotEmpty(domain);
        var converter = new PortugueseConverter();
        var output = converter.Convert(source);
        Assert.Equal(expected, output);
        Assert.Equal(output, converter.Convert(output));
    }

    [Fact]
    public void BenchmarkMeasuresWrongChangesMissesAndPreservation()
    {
        var cases = BenchmarkCases().ToArray();
        var converter = new PortugueseConverter();
        var rows = cases.Select(c => (Source: (string)c[1], Expected: (string)c[2], Output: converter.Convert((string)c[1]))).ToArray();
        var wrongChanges = rows.Count(r => r.Output != r.Source && r.Output != r.Expected);
        var missedChanges = rows.Count(r => r.Output == r.Source && r.Expected != r.Source);
        var preservationFailures = rows.Count(r => r.Expected == r.Source && r.Output != r.Source);
        Assert.Equal(0, wrongChanges);
        Assert.Equal(0, missedChanges);
        Assert.Equal(0, preservationFailures);
        Assert.Equal(5, cases.Select(c => c[0]).Distinct().Count());
    }

    [Fact]
    public void BatchAndSharedPipelineAgreeOnOptionalSentenceAndLayoutBehavior()
    {
        var options = new ConversionOptions(SentenceContext: true, Rewrap: true, LineLength: 30, UseReference: false, YouToTu: false);
        var blocks = new List<SrtBlock>
        {
            Cue("Não vou", 0, 2000), Cue("fala mais com vocês.", 2100, 4000),
            Cue("Vou falar com o diretor amanhã de manhã.", 8000, 12000),
        };
        var request = new PluginRequest
        {
            RequestType = PluginRequestType.Batch, Settings = options.ToJson(), SettingsVersion = ConversionOptions.Version,
            Subtitle = new PluginSubtitle { Format = "SubRip", SubRip = SubRipParser.Serialize(blocks) },
        };
        var output = BatchMode.BuildBatchResponse(request, Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml"),
            new EmbeddingSettings(CheckMeaning: false, UseJudge: false));
        Assert.Equal(PluginStatus.Ok, output.Status);
        var actual = SubRipParser.Parse(output.Subtitle!.Native);
        var expected = SubtitleConversion.Convert(blocks, options);
        Assert.Equal(blocks.Count, actual.Count);
        for (var i = 0; i < blocks.Count; i++)
        {
            Assert.Equal(SubtitleLayout.Rewrap(expected[i].Text, options), actual[i].Text);
            Assert.Equal(blocks[i].StartMs, actual[i].StartMs);
            Assert.Equal(blocks[i].EndMs, actual[i].EndMs);
        }
    }
}
