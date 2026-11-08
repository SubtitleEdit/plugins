using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class ConflictVocabularyTests
{
    [Theory]
    [InlineData("A briga estourou\nfaz 20 minutos.", "Fighting broke out\n20 minutes ago.",
        "A luta estourou\nfaz 20 minutos.")]
    [InlineData("As brigas terminaram.", "The physical fights ended.", "As lutas terminaram.")]
    [InlineData("Uma briga violenta.", "A violent brawl.", "Uma rixa violenta.")]
    [InlineData("As brigas violentas.", "The violent brawls.", "As rixas violentas.")]
    [InlineData("Uma briga.", "A physical scuffle.", "Uma rixa.")]
    [InlineData("A briga foi por dinheiro.", "The argument was about money.", "A discussão foi por dinheiro.")]
    [InlineData("As brigas foram por dinheiro.", "The arguments were about money.", "As discussões foram por dinheiro.")]
    [InlineData("A briga pela propriedade.", "The dispute over the property.", "A disputa pela propriedade.")]
    [InlineData("A briga pelo título.", null, "A luta pelo título.")]
    [InlineData("Uma briga de rua.", null, "Uma luta de rua.")]
    [InlineData("Uma briga de faca.", null, "Uma luta de faca.")]
    [InlineData("Uma briga corpo a corpo.", null, "Uma luta corpo a corpo.")]
    [InlineData("A briga contra o câncer.", null, "A luta contra o cancro.")]
    [InlineData("A briga verbal.", null, "A discussão verbal.")]
    [InlineData("A BRIGA ESTOUROU.", "Fighting broke out.", "A LUTA ESTOUROU.")]
    [InlineData("<i>A briga estourou.</i>", "Fighting broke out.", "<i>A luta estourou.</i>")]
    [InlineData("Um bate-boca acalorado.", "A heated argument.", "Uma discussão acalorada.")]
    [InlineData("Os bate-bocas acalorados.", "The heated arguments.", "As discussões acaloradas.")]
    [InlineData("Um bate-boca.", "A chat.", "Uma conversa.")]
    [InlineData("Um bate-boca.", "An uproar.", "Uma algazarra.")]
    [InlineData("Eles brigaram.", "They fought with their fists.", "Eles lutaram.")]
    [InlineData("Ele brigou.", "He fought in battle.", "Ele lutou.")]
    [InlineData("Eles estavam brigando.", "They were fighting with their fists.", "Eles estavam a lutar.")]
    [InlineData("Ele vai brigar.", "He will fight in battle.", "Ele vai lutar.")]
    [InlineData("Se eles brigarem.", "If they fight with their fists.", "Se eles lutarem.")]
    [InlineData("Eles brigaram a socos.", null, "Eles lutaram a socos.")]
    [InlineData("Eles brigam pela liberdade.", null, "Eles lutam pela liberdade.")]
    [InlineData("Ele briga.", "He fights with his fists.", "Ele luta.")]
    [InlineData("Tu brigas.", "You fight with your fists.", "Tu lutas.")]
    [InlineData("Ele briga por dinheiro.", "He argues about money.", "Ele discute por dinheiro.")]
    public void ContextDistinguishesCombatArgumentsAndDisputes(string source, string? english, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Theory]
    [InlineData("Uma briga.", null, "Uma zanga.")]
    [InlineData("Eles brigaram.", null, "Eles discutiram.")]
    [InlineData("Eles brigaram.", "They argued about dinner.", "Eles discutiram.")]
    [InlineData("Eles brigaram.", "They fight about money.", "Eles discutiram.")]
    [InlineData("Estamos brigados.", "We had a fight.", "Estamos brigados.")]
    [InlineData("Uma briga de palavras.", "A physical fight.", "Uma zanga de palavras.")]
    [InlineData("A luta terminou.", "The argument ended.", "A luta terminou.")]
    [InlineData("A zanga terminou.", "The fighting ended.", "A zanga terminou.")]
    [InlineData("Obrigado, brigada.", "The fighting ended.", "Obrigado, brigada.")]
    [InlineData("Uma briga e uma briga.", "A fight and an argument.", "Uma zanga e uma zanga.")]
    [InlineData("Uma pancadaria.", "A brawl.", "Uma pancadaria.")]
    [InlineData("Um barraco.", "A small shack.", "Um barraco.")]
    [InlineData("Briga começou.", "Fighting started.", "Briga começou.")]
    [InlineData("Brigas violentas.", "Violent fights.", "Brigas violentas.")]
    [InlineData("Uma briga.", "A fight and an argument.", "Uma zanga.")]
    [InlineData("Eles brigaram.", "They argued, then fought.", "Eles discutiram.")]
    [InlineData("Eles brigavam como um velho casal.", "They would fight like an old married couple.",
        "Eles discutiam como um velho casal.")]
    [InlineData("Uma briga doméstica.", "A domestic dispute.", "Uma discussão doméstica.")]
    [InlineData("Uma briga.", "A fight.", "Uma zanga.")]
    [InlineData("Eles brigaram.", "They fought.", "Eles discutiram.")]
    [InlineData("Uma briga.", "A battle of words.", "Uma discussão.")]
    [InlineData("Um bate-boca.", "A physical fight.", "Uma discussão.")]
    [InlineData("Eles brigaram com o NCIS.", "They got into a scuffle with NCIS over a theory.",
        "Eles discutiram com o NCIS.")]
    [InlineData("brigue com o NCIS por um palpite\nde que a Força Quds está",
        "get into a scuffle with NCIS over some harebrained notion\nthat Quds Force is picking off SEALs",
        "discuta com o NCIS por um palpite\nde que a Força Quds está")]
    public void AmbiguousAndAlreadyEuropeanReadingsAreNotRewritten(string source, string? english, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Fact]
    public void SeparateClausesKeepTheirOwnConflictSense()
    {
        const string source = "A briga acabou. A briga começou.";
        var hint = ReferenceHint.FromText("The physical fight ended. The argument started.");
        Assert.Equal("A luta acabou. A discussão começou.", new PortugueseConverter().Convert(source, reference: hint));
        Assert.Equal("- Uma luta.\n- Uma discussão.", new PortugueseConverter().Convert("- Uma briga.\n- Uma briga.",
            reference: ReferenceHint.FromText("- A physical fight.\n- An argument.")));
    }

    [Fact]
    public void EveryUnambiguousGeneratedVerbFormRetainsTenseAndPerson()
    {
        var entry = new VerbEntry("brigar", "discutir", null, null,
            new[] { "brigado", "brigada", "brigados", "brigadas" }, new[] { "briga", "brigas" });
        var verbs = new VerbVocabulary(new[] { entry }, _ => false, _ => false);
        Assert.True(VerbConjugator.TryConjugate("brigar", out var source));
        Assert.True(VerbConjugator.TryConjugate("lutar", out var combat));
        Assert.True(VerbConjugator.TryConjugate("discutir", out var argument));
        var checkedForms = 0;
        foreach (var group in source.Select((form, index) => (form, index)).GroupBy(pair => pair.form))
        {
            if (entry.Skip.Contains(group.Key) || group.Select(pair => argument[pair.index]).Distinct().Count() != 1 ||
                group.Select(pair => combat[pair.index]).Distinct().Count() != 1) continue;
            var index = group.First().index;
            Assert.Equal("Eles " + combat[index], verbs.Apply("Eles " + group.Key, ReferenceHint.FromText("They fought with their fists.")));
            Assert.Equal("Eles " + argument[index], verbs.Apply("Eles " + group.Key, ReferenceHint.FromText("They argued.")));
            checkedForms++;
        }
        Assert.True(checkedForms >= 30, $"Only {checkedForms} inflections checked.");
    }

    [Fact]
    public void ExistingControlsAndLocalEntriesTakePrecedence()
    {
        var hint = ReferenceHint.FromText("Fighting broke out.")!;
        const string source = "A briga estourou.";
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Vocabulary: false)).Convert(source, reference: hint));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(DisabledRules: "word-list")).Convert(source, reference: hint));
        var ignored = new PortugueseLocalWordList();
        ignored.Ignored.Add("briga");
        Assert.Equal(source, new PortugueseConverter(localWordList: ignored).Convert(source, reference: hint));
        var local = new PortugueseLocalWordList();
        local.Words.Add(("briga", "contenda"));
        Assert.Equal("A contenda estourou.", new PortugueseConverter(localWordList: local).Convert(source, reference: hint));
        Assert.Equal("Eles brigaram.", new PortugueseConverter(new ConversionOptions(DisabledRules: "verb-vocabulary"))
            .Convert("Eles brigaram.", reference: ReferenceHint.FromText("They fought.")));
        var ignoredVerb = new PortugueseLocalWordList();
        ignoredVerb.Ignored.Add("brigar");
        Assert.Equal("Eles brigaram.", new PortugueseConverter(localWordList: ignoredVerb)
            .Convert("Eles brigaram.", reference: ReferenceHint.FromText("They fought.")));
        var nounOverride = new PortugueseLocalWordList();
        nounOverride.Words.Add(("briga", "disputa"));
        Assert.Equal("Ele disputa.", new PortugueseConverter(localWordList: nounOverride)
            .Convert("Ele briga.", reference: ReferenceHint.FromText("He fights.")));
    }

    [Fact]
    public void BatchPipelinePreservesOriginalsTimingsAndRepeatedOutput()
    {
        const string source = "A briga estourou\nfaz 20 minutos.";
        var blocks = new List<SrtBlock> { new() { StartMs = 1045145, EndMs = 1046574, Text = source } };
        var results = SubtitleConversion.Convert(blocks, new ConversionOptions(),
            english: new string?[] { "Fighting broke out\n20 minutes ago." });
        Assert.Equal("A luta estourou\nfaz 20 minutos.", results[0].Text);
        Assert.Equal(source, blocks[0].Text);
        Assert.Equal(1045145, blocks[0].StartMs);
        Assert.Equal(1046574, blocks[0].EndMs);
        Assert.Contains(results[0].Rules, r => r.Id == "word-list");
        Assert.Equal(results[0].Text, new PortugueseConverter().Convert(results[0].Text,
            reference: ReferenceHint.FromText("Fighting broke out\n20 minutes ago.")));
    }
}
