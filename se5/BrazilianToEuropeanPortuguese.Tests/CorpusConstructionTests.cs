using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class CorpusConstructionTests
{
    [Theory]
    [InlineData("Ele pisou no meu rosto\ncom sua bota.", "He stepped on my face\nwith his boot.", "Ele pisou-me o rosto\ncom a bota.")]
    [InlineData("Ele pisou no meu rosto com sua bota.", "He stepped on my face with his boot.", "Ele pisou-me o rosto com a bota.")]
    [InlineData("Você tinha uma arma na sua mão.", "You did have a gun in your hand.", "Tinhas uma arma na mão.")]
    [InlineData("Ela bateu na minha cara.", null, "Ela bateu-me na cara.")]
    [InlineData("Ele agarrou o meu braço.", null, "Ele agarrou-me o braço.")]
    [InlineData("Parti o meu braço.", null, "Parti o braço.")]
    [InlineData("Toquei na minha cara.", null, "Toquei na cara.")]
    [InlineData("Ele não pisou no meu pé.", null, "Ele não me pisou o pé.")]
    [InlineData("Eu pisei no seu pé.", "I stepped on your foot.", "Eu pisei-te o pé.")]
    [InlineData("Ela pisava no meu pé.", "She was stepping on my foot.", "Ela pisava-me o pé.")]
    [InlineData("Eles torceram o meu pulso.", null, "Eles torceram-me o pulso.")]
    public void InalienablePossessionUsesDativeOrBareArticle(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("Ele fugiu com a sua carteira.", "He ran off with your wallet.")]
    [InlineData("Ele tocou na minha vida.", null)]
    [InlineData("Ele bateu na minha porta.", null)]
    [InlineData("Pisava no meu pé.", null)]
    [InlineData("Ele pisou no seu pé.", null)]
    [InlineData("Ele pisou no seu pé.", "He stepped on your foot, and her foot too.")]
    public void InalienablePossessionDoesNotGuessOwnersOrOtherSenses(string source, string? english)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(converter.Convert(source), converter.Convert(source, reference: ReferenceHint.FromText(english)));
    }

    [Fact]
    public void InalienablePossessionRespectsOptionsFormalityAndLocalChoices()
    {
        const string source = "Ele pisou no meu rosto com sua bota.";
        var reference = ReferenceHint.FromText("He stepped on my face with his boot.");
        var withoutRule = new PortugueseConverter(new ConversionOptions(DisabledRules: "inalienable-possession"));
        Assert.DoesNotContain("pisou-me", withoutRule.Convert(source, reference: reference));
        Assert.DoesNotContain("pisou-me", new PortugueseConverter(new ConversionOptions(Pronouns: false))
            .Convert(source, reference: reference));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("pisar");
        Assert.DoesNotContain("pisou-me", new PortugueseConverter(localWordList: words)
            .Convert(source, reference: reference));
        Assert.Equal("Eu pisei-lhe o pé.", new PortugueseConverter().Convert("Eu pisei no seu pé.",
            reference: ReferenceHint.FromText("I stepped on your foot.")! with { FormalAddress = true }));
    }

    [Theory]
    [InlineData("- Ele pisou\n- no meu rosto.")]
    [InlineData("Ele pisou\n\nno meu rosto.")]
    [InlineData("- arma\n- na sua mão.")]
    [InlineData("arma\n\nna sua mão.")]
    public void InalienablePossessionDoesNotCrossSpeakersOrBlankLines(string source)
    {
        var changes = new List<RuleChange>();
        new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText("He stepped on my face with his boot."),
            trace: changes);
        Assert.DoesNotContain(changes, c => c.Id == "inalienable-possession");
    }

    [Fact]
    public void InalienablePossessionCueUsesAlignedReferenceAndPreservesSource()
    {
        var source = new[] { new SrtBlock { StartMs = 274590, EndMs = 276923, Text = "Ele pisou no meu rosto\ncom sua bota." } };
        var english = new[] { new SrtBlock { StartMs = 274241, EndMs = 276977, Text = "He stepped on my face\nwith his boot." } };
        var aligned = ReferenceAligner.AlignByTiming(source, english);
        var converted = Assert.Single(SubtitleConversion.Convert(source, ConversionOptions.Default, english: aligned.Texts));
        Assert.Equal("Ele pisou-me o rosto\ncom a bota.", converted.Text);
        Assert.Equal((274590, 276923, "Ele pisou no meu rosto\ncom sua bota."), (source[0].StartMs, source[0].EndMs, source[0].Text));
    }

    [Theory]
    [InlineData("e contou sobre isso.", "and told her about it.", "e contou-lhe sobre isso.")]
    [InlineData("Ele contou sobre isso.", "He told me about it.", "Ele contou-me sobre isso.")]
    [InlineData("Contou sobre isso.", "He told you about it.", "Contou-te sobre isso.")]
    [InlineData("Ela não disse a verdade.", "She never told him the truth.", "Ela não lhe disse a verdade.")]
    [InlineData("Ele vai contar tudo.", "He's going to tell them everything.", "Ele vai contar-lhes tudo.")]
    [InlineData("Ele mentiu sobre isso.", "He lied to her about it.", "Ele mentiu-lhe sobre isso.")]
    [InlineData("Ele mostrou isso.", "He showed us that.", "Ele mostrou-nos isso.")]
    [InlineData("Ela perguntou sobre isso.", "She asked me about it.", "Ela perguntou-me sobre isso.")]
    public void TellingVerbsRecoverTheReferenceConfirmedRecipient(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("e contou sobre isso.", null)]
    [InlineData("e contou sobre isso.", "and told her and told me about it.")]
    [InlineData("e não contou sobre isso.", "and told her about it.")]
    [InlineData("A conta sobre isso.", "He told her about it.")]
    [InlineData("Ele contou à esposa sobre isso.", "He told her about it.")]
    [InlineData("Ele me contou sobre isso.", "He told me about it.")]
    [InlineData("Ele falou com ela. Ela falava que era inocente.", "He spoke to her. She said she was innocent.")]
    [InlineData("- e contou\n- sobre isso.", "and told her about it.")]
    [InlineData("e contou\n\nsobre isso.", "and told her about it.")]
    public void TellingVerbsDoNotGuessRecipientsWithoutUnambiguousEvidence(string source, string? english)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(converter.Convert(source), converter.Convert(source, reference: ReferenceHint.FromText(english)));
    }

    [Fact]
    public void TellingRecipientHonoursOptionsFormalityAndLocalChoices()
    {
        const string source = "e contou sobre isso.";
        var reference = ReferenceHint.FromText("and told you about it.");
        Assert.Equal("e contou-lhe sobre isso.", new PortugueseConverter()
            .Convert(source, reference: reference! with { FormalAddress = true }));
        Assert.DoesNotContain("-lhe", new PortugueseConverter(new ConversionOptions(Pronouns: false))
            .Convert("e contou sobre isso.", reference: ReferenceHint.FromText("and told her about it.")));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("contar");
        Assert.DoesNotContain("-lhe", new PortugueseConverter(localWordList: words)
            .Convert("e contou sobre isso.", reference: ReferenceHint.FromText("and told her about it.")));
    }

    [Fact]
    public void TellingRecipientCueUsesAlignedReferenceAndPreservesSource()
    {
        var source = new[] { new SrtBlock { StartMs = 111580, EndMs = 112736, Text = "e contou sobre isso." } };
        var english = new[] { new SrtBlock { StartMs = 111579, EndMs = 112913, Text = "and told her about it." } };
        var aligned = ReferenceAligner.AlignByTiming(source, english);
        var converted = Assert.Single(SubtitleConversion.Convert(source, ConversionOptions.Default, english: aligned.Texts));
        Assert.Equal("e contou-lhe sobre isso.", converted.Text);
        Assert.Equal((111580, 112736, "e contou sobre isso."), (source[0].StartMs, source[0].EndMs, source[0].Text));
    }

    [Theory]
    [InlineData("Não, mas não pensou", "No, but you didn't think...", "Não, mas não pensaste")]
    [InlineData("Não, você não pensou", "No, you didn't think...", "Não, não pensaste")]
    public void InitialNegationCommaStillTakesReferenceSecondPerson(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("Não, mas não pensou", "No, but he didn't think...")]
    [InlineData("Não, mas ele não pensou", "No, but you didn't think...")]
    [InlineData("Não, mas não pensou", null)]
    public void InitialNegationCommaDoesNotGuessWithoutSecondPersonEvidence(string source, string? english)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(converter.Convert(source), converter.Convert(source, reference: ReferenceHint.FromText(english)));
    }

    [Fact]
    public void InitialNegationCueUsesAlignedReferenceAndPreservesSource()
    {
        var source = new[] { new SrtBlock { StartMs = 2322061, EndMs = 2323654, Text = "Não, mas não pensou" } };
        var english = new[] { new SrtBlock { StartMs = 2321953, EndMs = 2323622, Text = "No, but you\ndidn't think..." } };
        var aligned = ReferenceAligner.AlignByTiming(source, english);
        var converted = Assert.Single(SubtitleConversion.Convert(source, ConversionOptions.Default, english: aligned.Texts));
        Assert.Equal("Não, mas não pensaste", converted.Text);
        Assert.Equal((2322061, 2323654, "Não, mas não pensou"), (source[0].StartMs, source[0].EndMs, source[0].Text));
    }

    [Theory]
    [InlineData("Como divertisse você?", "Like I amuse you?", "Como se eu te divertisse?")]
    [InlineData("Como eu fosse um palhaço?", "Like I'm a clown?", "Como se eu fosse um palhaço?")]
    [InlineData("Como ela conhecesse você?", "As if she knew you?", "Como se ela te conhecesse?")]
    [InlineData("Como nós ajudássemos você?", "As though we helped you?", "Como se nós te ajudássemos?")]
    [InlineData("Como ele não visse você?", "Like he didn't see you?", "Como se ele não te visse?")]
    [InlineData("Como ele divertisse você?", "Like he's amusing you?", "Como se ele te divertisse?")]
    [InlineData("Como eu gostasse dele?", "As if I liked him?", "Como se eu gostasse dele?")]
    [InlineData("Como eu nunca ajudasse você?", "Like I never helped you?", "Como se eu nunca te ajudasse?")]
    public void HypotheticalComparisonsRestoreConjunctionAndSubject(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("Como eu fosse um palhaço?", null)]
    [InlineData("Como eu fosse um palhaço?", "I like clowns.")]
    [InlineData("Como eu fosse um palhaço?", "Like he's a clown?")]
    [InlineData("Como divertisse você?", "Like I amuse him?")]
    [InlineData("Como divertisse você?", "Like I amuse you? He's lying.")]
    [InlineData("Como se sentiu?", "How did he feel?")]
    [InlineData("Como eu divertisse você?", "Like I didn't amuse you?")]
    [InlineData("Como eu nunca ajudasse você?", "Like I didn't help you?")]
    [InlineData("Como divertisse eu?", "Like I amuse you?")]
    [InlineData("Como se eu fosse um palhaço?", "Like I'm a clown?")]
    public void HypotheticalComparisonsDoNotGuessWithoutMatchingEvidence(string source, string? english)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(converter.Convert(source), converter.Convert(source, reference: ReferenceHint.FromText(english)));
    }

    [Fact]
    public void HypotheticalComparisonsRespectOptionsAndLocalChoices()
    {
        const string source = "Como divertisse você?";
        var reference = ReferenceHint.FromText("Like I amuse you?");
        var withoutRule = new PortugueseConverter(new ConversionOptions(DisabledRules: "hypothetical-comparison"));
        Assert.Equal(withoutRule.Convert(source), withoutRule.Convert(source, reference: reference));
        var withoutColloquial = new PortugueseConverter(new ConversionOptions(Colloquial: false));
        Assert.Equal(withoutColloquial.Convert(source), withoutColloquial.Convert(source, reference: reference));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("divertir");
        var ignored = new PortugueseConverter(localWordList: words);
        Assert.Equal(ignored.Convert(source), ignored.Convert(source, reference: reference));
        Assert.Equal("Como se eu divertisse você?", new PortugueseConverter(new ConversionOptions(YouToTu: false))
            .Convert(source, reference: reference));
        Assert.Equal("Como se eu o divertisse?", new PortugueseConverter()
            .Convert(source, reference: reference! with { FormalAddress = true }));
        Assert.Equal("Como se eu te divertisse?", new PortugueseConverter(new ConversionOptions(Pronouns: false))
            .Convert(source, reference: reference));
    }

    [Theory]
    [InlineData("- Como\n- divertisse você?")]
    [InlineData("Como\n\ndivertisse você?")]
    [InlineData("- Como eu\n- fosse um palhaço?")]
    [InlineData("Como eu\n\nfosse um palhaço?")]
    [InlineData("Como divertisse\n- você?")]
    [InlineData("Como divertisse\n\nvocê?")]
    public void HypotheticalComparisonsDoNotCrossSpeakersOrBlankLines(string source)
    {
        var changes = new List<RuleChange>();
        new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText("Like I amuse you?"), trace: changes);
        Assert.DoesNotContain(changes, c => c.Id == "hypothetical-comparison");
    }

    [Fact]
    public void HypotheticalComparisonCueUsesAlignedReferenceAndPreservesSource()
    {
        var source = new[] { new SrtBlock { StartMs = 2352514, EndMs = 2355504, Text = "Como divertisse você?" } };
        var english = new[] { new SrtBlock { StartMs = 2352451, EndMs = 2355053, Text = "Like I amuse you?" } };
        var aligned = ReferenceAligner.AlignByTiming(source, english);
        var converted = Assert.Single(SubtitleConversion.Convert(source, ConversionOptions.Default, english: aligned.Texts));
        Assert.Equal("Como se eu te divertisse?", converted.Text);
        Assert.Equal((2352514, 2355504, "Como divertisse você?"), (source[0].StartMs, source[0].EndMs, source[0].Text));
    }

    [Fact]
    public void HypotheticalComparisonSupportsWrappedEnglishReference()
    {
        const string english = "Like I amuse you?";
        var converter = new PortugueseConverter();
        for (var index = 0; index < english.Length; index++)
        {
            if (english[index] != ' ') continue;
            foreach (var newline in new[] { "\n", "\r\n" })
            {
                var wrapped = english[..index] + newline + english[(index + 1)..];
                Assert.Equal("Como se eu te divertisse?", converter.Convert("Como divertisse você?",
                    reference: ReferenceHint.FromText(wrapped)));
            }
        }
    }

    [Theory]
    [InlineData("Num trote de celular?", null, "Numa chamada de brincadeira?")]
    [InlineData("O trote telefônico foi estranho.", null, "A chamada de brincadeira foi estranha.")]
    [InlineData("Os trotes por telefone.", null, "As chamadas de brincadeira.")]
    [InlineData("Uma ligação de trote.", null, "Uma chamada de brincadeira.")]
    [InlineData("O trote estudantil.", null, "A praxe académica.")]
    [InlineData("Os trotes universitários.", null, "As praxes académicas.")]
    [InlineData("Era um trote.", "It was a prank.", "Era uma brincadeira.")]
    [InlineData("São trotes.", "They're practical jokes.", "São brincadeiras.")]
    [InlineData("O trote foi engraçado.", "The prank was funny.", "A brincadeira foi engraçada.")]
    [InlineData("Ele passou um trote.", null, "Ele pregou uma partida.")]
    [InlineData("Eles passaram uns trotes.", null, "Eles pregaram umas partidas.")]
    public void PrankAndStudentInitiationVocabularyUsesTheRightSense(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("O cavalo passou a trote.", "The horse went at a trot.")]
    [InlineData("O trote do cavalo.", "The horse's trot.")]
    [InlineData("O trote foi rápido.", null)]
    [InlineData("O trote do peludo.", "A prank.")]
    [InlineData("Ele deu um trote no cavalo.", "He played a prank.")]
    [InlineData("Ele passou um trote no colega.", "He played a prank.")]
    [InlineData("O trote.", "The freshman hazing was a prank.")]
    public void PrankVocabularyDoesNotGuessOtherOrUnsafeSenses(string source, string? english)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(converter.Convert(source), converter.Convert(source, reference: ReferenceHint.FromText(english)));
    }

    [Fact]
    public void PrankVocabularyHonoursOptionsAndLocalChoices()
    {
        const string source = "Num trote de celular?";
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(DisabledRules: "word-list"))
            .Convert(source));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Vocabulary: false)).Convert(source));
        Assert.Equal("Ele passou um trote.", new PortugueseConverter(new ConversionOptions(DisabledRules: "prank-actions"))
            .Convert("Ele passou um trote.", reference: ReferenceHint.FromText("It was a prank.")));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("trote");
        Assert.Equal("Num trote de telemóvel?", new PortugueseConverter(localWordList: words).Convert(source));
        words.Ignored.Clear();
        words.Words.Add(("trote", "partida"));
        Assert.Equal("Num partida de telemóvel?", new PortugueseConverter(localWordList: words).Convert(source));
    }

    [Theory]
    [InlineData("Um trote\n\nde celular.")]
    [InlineData("O trote\n\nestudantil.")]
    [InlineData("- Um trote\n- de celular.")]
    [InlineData("- O trote\n- estudantil.")]
    public void QualifiedPrankTermsDoNotCrossBlankLinesOrSpeakers(string source)
    {
        var result = new PortugueseConverter().Convert(source);
        Assert.DoesNotContain("chamada", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("praxe", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PrankCallCueRetainsTimingAndSecondSpeaker()
    {
        var source = new[] { new SrtBlock { StartMs = 305552, EndMs = 307578, Text = "-Num trote de celular?\n-E deveriam" } };
        var english = new[] { new SrtBlock { StartMs = 305739, EndMs = 307741, Text = "On a prank phone call?\nAnd they should" } };
        var aligned = ReferenceAligner.AlignByTiming(source, english);
        var converted = Assert.Single(SubtitleConversion.Convert(source, ConversionOptions.Default, english: aligned.Texts));
        Assert.StartsWith("-Numa chamada de brincadeira?\n-", converted.Text);
        Assert.Equal((305552, 307578, "-Num trote de celular?\n-E deveriam"), (source[0].StartMs, source[0].EndMs, source[0].Text));
    }

    [Theory]
    [InlineData("<i>Então decidiu revidar.</i>", "So you decided to get back at him.", "<i>Então decidiste vingar-te.</i>")]
    [InlineData("Eu revidarei.", "I will get back at him.", "Eu vingar-me-ei.")]
    [InlineData("Eu revidava.", "I wanted revenge.", "Eu vingava-me.")]
    [InlineData("Nós revidaríamos.", "We would get back at him.", "Nós vingar-nos-íamos.")]
    [InlineData("Eles revidariam.", "They would get back at him.", "Eles vingar-se-iam.")]
    [InlineData("Ela revidou.", "She got back at him.", "Ela vingou-se.")]
    [InlineData("Eles revidaram.", "They got back at him.", "Eles vingaram-se.")]
    [InlineData("Ela revidou.", "She got even with the witness.", "Ela vingou-se.")]
    [InlineData("Ela não revidou.", "She did not get back at him.", "Ela não se vingou.")]
    [InlineData("Ele quer revidar.", "He wants to get back at her.", "Ele quer vingar-se.")]
    [InlineData("Ele está revidando.", "He is getting back at her.", "Ele está a vingar-se.")]
    [InlineData("Eu estava revidando.", "I was getting back at him.", "Eu estava a vingar-me.")]
    [InlineData("Decidi revidar.", "I decided to get back at him.", "Decidi vingar-me.")]
    [InlineData("Ela retrucou.", "She replied.", "Ela respondeu.")]
    [InlineData("Eles retrucaram.", "They retorted.", "Eles responderam.")]
    [InlineData("Ela revidou.", "She replied.", "Ela respondeu.")]
    public void RevengeAndRetortsUseReferenceQualifiedAlternatives(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: hint));
        Assert.Equal(expected, converter.Convert(expected, reference: hint));
    }

    [Theory]
    [InlineData("Ele revidou.", null)]
    [InlineData("Ele revidou.", "He fought back.")]
    [InlineData("Ele revidou.", "He replied and wanted revenge.")]
    [InlineData("Ele revidou o golpe.", "He got back at him.")]
    [InlineData("Ela revidou.", "He got back at her.")]
    [InlineData("Ela decidiu revidar.", "He decided to get back at her.")]
    [InlineData("Ela revidou.", "She did not get back at him.")]
    [InlineData("Ela não revidou.", "She got back at him.")]
    [InlineData("Ele retrucou.", "He played another card.")]
    [InlineData("Ele revidava.", "They sought revenge.")]
    [InlineData("A revida.", "She replied.")]
    public void RevengeAndRetortsPreserveUnconfirmedOrDifferentSenses(string source, string? english)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(converter.Convert(source), converter.Convert(source, reference: ReferenceHint.FromText(english)));
    }

    [Fact]
    public void RevengeRulesRespectOptionsAndLocalChoices()
    {
        var hint = ReferenceHint.FromText("She got back at him.");
        const string source = "Ela revidou.";
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Vocabulary: false)).Convert(source, reference: hint));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Pronouns: false)).Convert(source, reference: hint));
        Assert.Equal("Ela respondeu.", new PortugueseConverter(new ConversionOptions(Pronouns: false))
            .Convert("Ela retrucou.", reference: ReferenceHint.FromText("She replied.")));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(DisabledRules: "retaliation-vocabulary")).Convert(source, reference: hint));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("revidar");
        Assert.Equal(source, new PortugueseConverter(localWordList: words).Convert(source, reference: hint));
    }

    [Fact]
    public void RevengeCueUsesAlignedReferenceWithoutChangingTimingOrMarkup()
    {
        var source = new[] { new SrtBlock { StartMs = 2186224, EndMs = 2187985, Text = "<i>Então decidiu revidar.</i>" } };
        var english = new[] { new SrtBlock { StartMs = 2186452, EndMs = 2187953, Text = "DETECTIVE: So you decided\nto get back at him." } };
        var aligned = ReferenceAligner.AlignByTiming(source, english);
        var converted = Assert.Single(SubtitleConversion.Convert(source, ConversionOptions.Default, english: aligned.Texts));
        Assert.Equal("<i>Então decidiste vingar-te.</i>", converted.Text);
        Assert.Equal((2186224, 2187985, "<i>Então decidiu revidar.</i>"), (source[0].StartMs, source[0].EndMs, source[0].Text));
    }

    [Theory]
    [InlineData("Seu advogado veio ao esquadrão,", "His lawyer came by the squad,", "O seu advogado veio à esquadra,")]
    [InlineData("Veio para acompanhá-lo à sala do esquadrão.", "Came to escort you to the squad room.", "Veio para acompanhá-lo à sala da esquadra.")]
    [InlineData("Ele está no esquadrão.", "He is at the squad.", "Ele está na esquadra.")]
    [InlineData("Ele saiu do esquadrão.", "He left the precinct.", "Ele saiu da esquadra.")]
    [InlineData("Vou para o distrito policial.", "I'm going to the precinct.", "Vou para a esquadra.")]
    [InlineData("NO ESQUADRÃO.", "At the squad.", "NA ESQUADRA.")]
    public void PolicePlacesNeedReferenceAndUseFeminineArticles(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("O esquadrão chegou.", "The squad arrived.")]
    [InlineData("Ele é do esquadrão.", "He works at the squad.")]
    [InlineData("Está no esquadrão de resgate.", "He is at the squad.")]
    [InlineData("Está no esquadrão.", "He is in the army squad.")]
    [InlineData("O esquadrão está em formação.", "The squad is in formation.")]
    [InlineData("Ele veio ao esquadrão.", null)]
    public void PoliceTeamAndMilitaryMeaningsAreNotPoliceStations(string source, string? english)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(converter.Convert(source), converter.Convert(source, reference: ReferenceHint.FromText(english)));
    }

    [Theory]
    [InlineData("Ele mente.", "He's lying.", "Ele está a mentir.")]
    [InlineData("Ela mente.", "She's lying.", "Ela está a mentir.")]
    [InlineData("Eu minto.", "I'm lying.", "Eu estou a mentir.")]
    [InlineData("Eles mentem.", "They're lying.", "Eles estão a mentir.")]
    [InlineData("Nós mentimos.", "We're lying.", "Nós estamos a mentir.")]
    [InlineData("Ele não mente.", "He isn't lying.", "Ele não está a mentir.")]
    [InlineData("Ele não mente.", "He's not lying.", "Ele não está a mentir.")]
    [InlineData("Tu mentias.", "You were lying.", "Tu estavas a mentir.")]
    [InlineData("Você mente.", "You're lying.", "Estás a mentir.")]
    [InlineData("Ele mentia.", "He was lying.", "Ele estava a mentir.")]
    [InlineData("Ela fala.", "She is talking.", "Ela está a falar.")]
    [InlineData("Ele trabalha.", "He is working.", "Ele está a trabalhar.")]
    [InlineData("Eles esperam.", "They are waiting.", "Eles estão a esperar.")]
    [InlineData("Ela dorme.", "She is sleeping.", "Ela está a dormir.")]
    [InlineData("Ela chora.", "She is crying.", "Ela está a chorar.")]
    [InlineData("Ele corre.", "He is running.", "Ele está a correr.")]
    [InlineData("Ela come.", "She is eating.", "Ela está a comer.")]
    [InlineData("Ele bebe.", "He is drinking.", "Ele está a beber.")]
    [InlineData("Ele está mentindo.", "He's lying.", "Ele está a mentir.")]
    public void ExplicitOngoingActionsUseProgressiveWhenEnglishConfirmsThem(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("Ele mente.", null)]
    [InlineData("Ele mente.", "He lies.")]
    [InlineData("Ele mente.", "She is lying.")]
    [InlineData("Ele mente.", "He isn't lying.")]
    [InlineData("Ele mente.", "He's lying. He isn't lying.")]
    [InlineData("Ele mente. Ele mente.", "He's lying.")]
    [InlineData("Ele não mente.", "He is lying.")]
    [InlineData("Ele mente.", "He is lying in bed.")]
    [InlineData("Ele mente.", "He is lying on a sofa.")]
    [InlineData("Ele sempre mente.", "He is always lying.")]
    [InlineData("Ele mentiu.", "He is lying.")]
    [InlineData("Eles mentem.", "He is lying.")]
    [InlineData("Ela trabalha.", "She was working.")]
    public void ProgressiveDoesNotInventPersonPolarityOrAspect(string source, string? english)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(converter.Convert(source), converter.Convert(source, reference: ReferenceHint.FromText(english)));
    }

    [Fact]
    public void PoliceAndProgressiveRulesRespectSettingsAndLocalWords()
    {
        var station = ReferenceHint.FromText("His lawyer came by the squad.");
        var lying = ReferenceHint.FromText("He's lying.");
        Assert.Equal("Ele veio ao esquadrão.", new PortugueseConverter(new ConversionOptions(Vocabulary: false))
            .Convert("Ele veio ao esquadrão.", reference: station));
        Assert.Equal("Ele veio ao esquadrão.", new PortugueseConverter(new ConversionOptions(DisabledRules: "police-station"))
            .Convert("Ele veio ao esquadrão.", reference: station));
        Assert.Equal("Ele mente.", new PortugueseConverter(new ConversionOptions(Progressive: false))
            .Convert("Ele mente.", reference: lying));
        Assert.Equal("Ele mente.", new PortugueseConverter(new ConversionOptions(DisabledRules: "reference-progressive"))
            .Convert("Ele mente.", reference: lying));
        Assert.Equal("Ele mente.", new PortugueseConverter(new ConversionOptions(DisabledRules: "progressive"))
            .Convert("Ele mente.", reference: lying));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("esquadrão");
        words.Ignored.Add("mentir");
        var converter = new PortugueseConverter(localWordList: words);
        Assert.Equal("Ele veio ao esquadrão.", converter.Convert("Ele veio ao esquadrão.", reference: station));
        Assert.Equal("Ele mente.", converter.Convert("Ele mente.", reference: lying));
    }

    [Fact]
    public void PoliceAndLyingCuesKeepTimingAndCorrespondingReference()
    {
        var blocks = new[]
        {
            new SrtBlock { StartMs = 1083239, EndMs = 1084315, Text = "Veio para acompanhá-lo\nà sala do esquadrão." },
            new SrtBlock { StartMs = 1747969, EndMs = 1749413, Text = "Seu advogado\nveio ao esquadrão," },
            new SrtBlock { StartMs = 2038809, EndMs = 2039878, Text = "Ele está mentindo." },
            new SrtBlock { StartMs = 2039878, EndMs = 2041771, Text = "-Certo.\n-Não é verdade! Ele mente." },
            new SrtBlock { StartMs = 2047346, EndMs = 2049166, Text = "-Entende? Ele mente.\n-Ótimo. Então não terá problemas" },
            new SrtBlock { StartMs = 2049259, EndMs = 2052020, Text = "-em vir ao esquadrão.\n-Ele mente." },
        };
        var originals = blocks.Select(b => (b.Text, b.StartMs, b.EndMs)).ToArray();
        var english = new[]
        {
            new SrtBlock { StartMs = 1082149, EndMs = 1084251, Text = "Came down to escort you\nto the squad room." },
            new SrtBlock { StartMs = 1747914, EndMs = 1749381, Text = "His lawyer\ncame by the squad," },
            new SrtBlock { StartMs = 2038570, EndMs = 2040072, Text = "He's lying." },
            new SrtBlock { StartMs = 2040106, EndMs = 2041740, Text = "Okay...\nThat's not true!\nHe's lying." },
            new SrtBlock { StartMs = 2047346, EndMs = 2049248, Text = "Do you understand? He's lying.\nThat's great.\nSo you won't have any problems" },
            new SrtBlock { StartMs = 2049281, EndMs = 2051717, Text = "coming down to the squad.\nCome on.\nHe's lying." },
        };
        var aligned = ReferenceAligner.AlignByTiming(blocks, english);
        Assert.Contains("He's lying", aligned.Texts[5]);
        var converted = SubtitleConversion.Convert(blocks, ConversionOptions.Default, english: aligned.Texts).ToArray();
        Assert.Contains("sala da esquadra", converted[0].Text.Replace("\n", " "));
        Assert.Contains("à esquadra", converted[1].Text.Replace("\n", " "));
        Assert.All(converted.Skip(2), cue => Assert.Contains("está a mentir", cue.Text.Replace("\n", " ")));
        Assert.Contains("à esquadra", converted[5].Text.Replace("\n", " "));
        Assert.Equal(originals, blocks.Select(b => (b.Text, b.StartMs, b.EndMs)).ToArray());
    }

    [Theory]
    [InlineData("Alguém que você acha que é calmo.", "Someone you think is calm.", "Alguém que achas que é calmo.")]
    [InlineData("O homem que você pensa que trabalha aqui.", "The man you think works here.", "O homem que pensas que trabalha aqui.")]
    [InlineData("Uma pessoa que você acredita que pode ajudar.", "A person you believe can help.", "Uma pessoa que acreditas que pode ajudar.")]
    [InlineData("Alguém que pensa que você é esperto.", "Someone who thinks you're smart.", "Alguém que pensa que és esperto.")]
    [InlineData("Você acha que é calmo.", "You think you are calm.", "Achas que és calmo.")]
    [InlineData("Diz.", "But you say your experience tells you it's a gun. It does.", "Diz.")]
    [InlineData("Sabe.", "You know. He does.", "Sabe.")]
    [InlineData("Diz.", "You say.", "Dizes.")]
    [InlineData("Continue fazendo isso.", "You keep doing that.", "Continua a fazer isso.")]
    [InlineData("Continues fazendo isso.", "I hope you keep doing that.", "Continues a fazer isso.")]
    [InlineData("Começa a ganhar dinheiro.", "You start making money.", "Começas a ganhar dinheiro.")]
    public void ReferenceAgreementPreservesEmbeddedSubjectsAndCommands(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("Espero não ter deixado você esperando.", "I hope I didn't keep you waiting.", "Espero não te ter deixado à espera.")]
    [InlineData("Tenho deixado você esperando.", "I have kept you waiting.", "Tenho-te deixado à espera.")]
    [InlineData("Nunca tinha deixado você esperando.", "I had never kept you waiting.", "Nunca te tinha deixado à espera.")]
    [InlineData("<i>Espero não ter deixado você esperando.</i>", "I hope I didn't keep you waiting.", "<i>Espero não te ter deixado à espera.</i>")]
    public void KeptWaitingUsesTheExistingAuxiliaryAndCliticRules(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("e a música ainda tocando?", "e a música ainda a tocar?")]
    [InlineData("As pessoas ainda esperando.", "As pessoas ainda a esperar.")]
    [InlineData("A DCO na minha casa, tirando fotos.", "A DCO na minha casa, a tirar fotos.")]
    [InlineData("A DCO na minha casa tirando fotos.", "A DCO na minha casa a tirar fotos.")]
    [InlineData("Na rua correndo.", "Na rua a correr.")]
    [InlineData("Na casa Fernando.", "Na casa Fernando.")]
    [InlineData("A música é linda.", "A música é linda.")]
    [InlineData("A pessoa ainda linda.", "A pessoa ainda linda.")]
    [InlineData("Na sala de comando.", "Na sala de comando.")]
    [InlineData("Na minha casa, pensando nisso.", "Na minha casa, pensando nisso.")]
    [InlineData("Num caso envolvendo apostas.", "Num caso envolvendo apostas.")]
    [InlineData("Num contrato incluindo despesas.", "Num contrato incluindo despesas.")]
    [InlineData("Num documento contendo dados.", "Num documento contendo dados.")]
    public void ProgressiveComplementsPreserveNonverbalAndPunctuatedUses(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Fact]
    public void OptionsAndIgnoredTextKeepPriority()
    {
        const string source = "Espero não ter deixado você esperando.";
        var reference = ReferenceHint.FromText("I hope I didn't keep you waiting.")!;
        Assert.Equal(source, new PortugueseConverter().Convert(source, reference: reference with { FormalAddress = true }));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Progressive: false)).Convert(source, reference: reference));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Pronouns: false)).Convert(source, reference: reference));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(DisabledRules: "waiting-complements")).Convert(source, reference: reference));
        var list = new PortugueseLocalWordList();
        list.Ignored.Add("deixado você esperando");
        Assert.Equal(source, new PortugueseConverter(localWordList: list).Convert(source, reference: reference));
        Assert.Equal("Espero não ter deixado você esperando.", new PortugueseConverter().Convert(source));
        Assert.Equal("e a música ainda tocando?", new PortugueseConverter(new ConversionOptions(Progressive: false)).Convert("e a música ainda tocando?"));
    }

    [Fact]
    public void ShortRepliesAndWaitingComplementsPreserveCueTimes()
    {
        var blocks = SubRipParser.Parse("1\n00:00:01,000 --> 00:00:03,000\nEspero não ter deixado\nvocê esperando.\n\n" +
            "2\n00:00:03,100 --> 00:00:04,100\nDiz.\n\n");
        var results = SubtitleConversion.Convert(blocks, new ConversionOptions(),
            english: new string?[] { "I hope I didn't keep you waiting.", "You say it. It does." });
        Assert.Equal("Espero não te ter deixado à espera.", string.Join(" ", results[0].Text.Split('\n')));
        Assert.Equal("Diz.", results[1].Text);
        Assert.Equal(1000L, blocks[0].StartMs);
        Assert.Equal(4100L, blocks[1].EndMs);
        Assert.Equal("Espero não ter deixado\nvocê esperando.", blocks[0].Text);
    }
}
