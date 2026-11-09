using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class VerbAuditTests
{
    [Theory]
    [InlineData("Aguenta firme.\nVamos sair dessa.", "You just hang on. We're gonna get out of this.", "Aguenta firme.\nVamos sair desta.")]
    [InlineData("Aguente firme.", "You hang in there.", "Aguenta firme.")]
    [InlineData("Considere isso meu presente de despedida.", "You can consider this my going-away present.", "Considera isto meu presente de despedida.")]
    public void ObjectCoverageDoesNotPromoteCommandsToStatements(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source, reference: ReferenceHint.FromText(english)));
    }

    [Theory]
    [InlineData("Ele vai monitorar o suspeito.", "Ele vai monitorizar o suspeito.")]
    [InlineData("Ele estava monitorando o suspeito.", "Ele estava a monitorizar o suspeito.")]
    [InlineData("Ele monitorou o suspeito.", "Ele monitorizou o suspeito.")]
    [InlineData("A monitora trabalha aqui.", "A monitora trabalha aqui.")]
    [InlineData("A boa monitora trabalha aqui.", "A boa monitora trabalha aqui.")]
    [InlineData("Os monitores estão ligados.", "Os monitores estão ligados.")]
    public void MonitoringVocabularyProtectsPeopleAndEquipmentNouns(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Theory]
    [InlineData("A NSA grampeou o celular pessoal.", "The NSA tapped the private cell phone.", "A NSA pôs sob escuta o telemóvel pessoal.")]
    [InlineData("Preciso grampear o telefone.", "", "Preciso de pôr sob escuta o telefone.")]
    [InlineData("Ele grampeia o telefone.", "", "Ele põe sob escuta o telefone.")]
    [InlineData("Não grampeie o telefone.", "", "Não ponhas sob escuta o telefone.")]
    [InlineData("para grampear o local.", "to wire the place.", "para pôr sob escuta o local.")]
    [InlineData("para grampear o local.", "", "para grampear o local.")]
    [InlineData("Ele grampeou os papéis.", "He stapled the papers.", "Ele agrafou os papéis.")]
    [InlineData("Ele grampeou alguma conversa.", "He picked up chatter on the wires.", "Ele intercetou alguma conversa.")]
    [InlineData("Ele grampeou alguma conversa.", "", "Ele grampeou alguma conversa.")]
    [InlineData("O banheiro estava grampeado para transmissão de vídeo.", "The bathroom was wired.", "A casa de banho estava grampeada para transmissão de vídeo.")]
    [InlineData("Bem, melhor você embromar.", "Well, you better stall it.", "Bem, melhor ganhares tempo.")]
    [InlineData("Ele embromou.", "He stalled.", "Ele ganhou tempo.")]
    [InlineData("Posso embromar com o melhor deles.", "I can flap them gums with the best of them.", "Posso embromar com o melhor deles.")]
    [InlineData("Ele embromou.", "The car engine stalled.", "Ele embromou.")]
    [InlineData("Vovô estava cochilando.", "Grandpa was dozing.", "O Avô estava a dormitar.")]
    [InlineData("Você cochilou e perdeu a chance.", "You missed the opportunity.", "Você cochilou e perdeu a hipótese.")]
    public void ReferenceAndComplementsBoundSurveillanceAndStalling(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Fact]
    public void EarInflectionsKeepUnstressedPluralForms()
    {
        Assert.True(VerbConjugator.TryConjugate("grampear", out var forms));
        foreach (var form in new[] { "grampeio", "grampeias", "grampeia", "grampeiam", "grampeie", "grampeies", "grampeiem", "grampeamos", "grampeemos", "grampeou" })
            Assert.Contains(form, forms);
        foreach (var form in new[] { "grampeo", "grampea", "grampee", "grampeiamos", "grampeiemos" })
            Assert.DoesNotContain(form, forms);
    }

    [Fact]
    public void NewVocabularyRespectsOptionsAndLocalEntries()
    {
        Assert.Equal("Ele monitorou.", new PortugueseConverter(new ConversionOptions(Vocabulary: false)).Convert("Ele monitorou."));
        var local = new PortugueseLocalWordList();
        local.Ignored.Add("monitorar");
        Assert.Equal("Ele monitorou.", new PortugueseConverter(localWordList: local).Convert("Ele monitorou."));
        Assert.Equal("Ele grampeou o telefone.", new PortugueseConverter(new ConversionOptions(DisabledRules: "reviewed-verb-senses")).Convert("Ele grampeou o telefone."));
        local.Ignored.Add("grampeou o telefone");
        Assert.Equal("Ele grampeou o telefone.", new PortugueseConverter(localWordList: local).Convert("Ele grampeou o telefone."));
        var infinitiveIgnored = new PortugueseLocalWordList();
        infinitiveIgnored.Ignored.Add("grampear");
        Assert.Equal("Ele grampeou o telefone.", new PortugueseConverter(localWordList: infinitiveIgnored).Convert("Ele grampeou o telefone."));
        var overridden = new PortugueseLocalWordList();
        overridden.Words.Add(("grampeou", "vigiou"));
        Assert.Equal("Ele vigiou o telefone.", new PortugueseConverter(localWordList: overridden).Convert("Ele grampeou o telefone."));
    }

    [Theory]
    [InlineData("Só queria te parabenizar.", "Só queria felicitar-te.")]
    [InlineData("Ele parabenizou você.", "Ele felicitou-te.")]
    [InlineData("Ele monitorou você.", "Ele monitorizou-te.")]
    [InlineData("Alguém aqui considera ele sensível?", "Alguém aqui o considera sensível?")]
    [InlineData("Ninguém bateu você.", "Ninguém te bateu.")]
    public void ReviewedVerbsKeepPersonalObjectsAcrossVocabularyPasses(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Theory]
    [InlineData("Olha só, é uma honra\nvocê nos dar o ar da graça.", "Look here, it's a thrill that you've graced us.", "Olha só, é uma honra\nvocê dar-nos o ar da graça.")]
    [InlineData("É tipo um cochilo depressivo?", "Is that a depression nap?", "É tipo um cochilo depressivo?")]
    [InlineData("Posso tirar um breve cochilo.", "I can take a brief nap.", "Posso tirar um breve cochilo.")]
    public void ReviewedHomographsAreNotReplacedAsVerbs(string source, string english, string expected)
    {
        var result = new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english));
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Tudo bem. Traga ela junto.\nMas tenha cuidado.", "Fine. Fine. Bring her. But be careful.", "Tudo bem. Trá-la junto.\nMas tem cuidado.")]
    [InlineData("Mas as pessoas que mataram eles\nmataram o meu amigo.", "But the people who killed those kids killed my friend.", "Mas as pessoas que os mataram\nmataram o meu amigo.")]
    [InlineData("Deixa ele vivo na minha ausência.", "Try and keep him alive while I'm gone.", "Deixa-o vivo na minha ausência.")]
    [InlineData("Deixa ela viva na minha ausência.", "Try and keep her alive while I'm gone.", "Deixa-a viva na minha ausência.")]
    public void FollowingAdjectivesAdverbsAndPredicatesDoNotVetoPersonalObjects(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source, reference: ReferenceHint.FromText(english)));
        Assert.Equal(expected, converter.Convert(expected, reference: ReferenceHint.FromText(english)));
    }

    [Theory]
    [InlineData("A surra você mereceu.", "A tareia mereceste.")]
    [InlineData("Ele honra você.", "Ele honra-te.")]
    [InlineData("Ele surra você.", "Ele surra-te.")]
    public void NominalObjectsAndFollowingSubjectsKeepTheirRoles(string source, string expected)
    {
        Assert.Equal(expected, new PortugueseConverter().Convert(source));
    }

    [Fact]
    public void StallingWithADirectObjectUsesATransitiveTarget()
    {
        var reference = ReferenceHint.FromText("He stalled the process.");
        Assert.Equal("Ele protelou o processo.", new PortugueseConverter().Convert("Ele embromou o processo.", reference: reference));
        Assert.Equal("Ele embromou você.", ConversionConstructions.ReviewedVerbSenses("Ele embromou você.",
            ReferenceHint.FromText("He stalled you.")));
    }
}
