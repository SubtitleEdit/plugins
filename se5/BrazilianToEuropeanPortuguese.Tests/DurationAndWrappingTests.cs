using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class DurationAndWrappingTests
{
    [Theory]
    [InlineData("Bem, talvez passou\nmuitas longas noites", "Well, maybe she spent a lot of long nights",
        "Bem, talvez tenha passado\nmuitas noites longas")]
    [InlineData("Passei muitas longas noites.", null, "Passei muitas noites longas.")]
    [InlineData("Passei muitas longas noites.", "I spent very long nights.", "Passei noites muito longas.")]
    [InlineData("Passei muitas longas noites.", "I spent many very long nights.", "Passei muitas noites longas.")]
    [InlineData("Passei muitas longas noites.", "I spent a lot of long nights, not very long nights.", "Passei muitas noites longas.")]
    [InlineData("Passei muitas longas noites.", "They were not very long nights.", "Passei muitas noites longas.")]
    [InlineData("Passei muito longas noites.", null, "Passei noites muito longas.")]
    [InlineData("Foram muitos longos dias.", null, "Foram muitos dias longos.")]
    [InlineData("Foram muitas difíceis semanas.", null, "Foram muitas semanas difíceis.")]
    [InlineData("Foi uma das muitas curtas horas.", null, "Foi uma das muitas horas curtas.")]
    [InlineData("PASSEI MUITAS LONGAS NOITES.", null, "PASSEI MUITAS NOITES LONGAS.")]
    public void DurationOrderPreservesQuantityUnlessDegreeIsExplicit(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("Passei muitas noites longas.")]
    [InlineData("Passei noites muito longas.")]
    [InlineData("Passei muitas boas noites.")]
    [InlineData("Muitos grandes homens.")]
    [InlineData("Passei muitas longos noites.")]
    public void OtherMeaningsAndUnresolvedAgreementStayProtected(string text) =>
        Assert.Equal(text, new PortugueseConverter().Convert(text));

    [Fact]
    public void DurationPreferenceUsesExistingStyleSwitches()
    {
        const string text = "Passei muitas longas noites.";
        Assert.Equal(text, new PortugueseConverter(new ConversionOptions(Colloquial: false)).Convert(text));
        Assert.Equal(text, new PortugueseConverter(new ConversionOptions(DisabledRules: "natural-word-order")).Convert(text));
    }

    [Fact]
    public void ProtectedWhitespaceAndIgnoredPhrasesStayVerbatim()
    {
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("muitas longas noites");
        Assert.Equal("Passei muitas\nlongas noites.", new PortugueseConverter(localWordList: words)
            .Convert("Passei muitas\nlongas noites."));
        Assert.Equal("É um autocarro.\u2028", new PortugueseConverter().Convert("É um ônibus.\u2028"));
        Assert.Equal("Texto sem\nalterações.", new PortugueseConverter().Convert("Texto sem\nalterações."));
    }

    [Theory]
    [InlineData("Espero que ambos precisam se conhecer de novo.")]
    [InlineData("A perna esquerda do meu padrinho foi lesionada.")]
    [InlineData("porque você sempre sabe quando eu tô mentindo.")]
    [InlineData("Só lembra-te quem enfrentas.")]
    [InlineData("Eu nunca vou te esquecer.")]
    [InlineData("Ele virou médico.")]
    [InlineData("Bem, talvez passou muitas longas noites.")]
    [InlineData("O telefone está tocando.")]
    [InlineData("Ela precisa se lembrar disso.")]
    [InlineData("Você está certo que pode fazer isso.")]
    [InlineData("Pergunto-me por que ela fez isso.")]
    [InlineData("Ele falou algo.")]
    [InlineData("Eu gabaritei as questões.")]
    [InlineData("Vá até o fim.")]
    [InlineData("É melhor você não contar.")]
    [InlineData("Ele gostou que eu dirigi.")]
    public void EveryWordBoundaryGetsTheSameCorrectionAcrossStages(string text)
    {
        var converter = new PortugueseConverter();
        var expected = converter.Convert(text);
        for (var index = 0; index < text.Length; index++)
        {
            if (text[index] != ' ') continue;
            foreach (var newline in new[] { "\n", "\r\n" })
            {
                var wrapped = text[..index] + newline + text[(index + 1)..];
                var converted = converter.Convert(wrapped);
                Assert.Equal(expected, converted.Replace(newline, " "));
                Assert.Equal(converted, converter.Convert(converted));
                Assert.Equal(1, converted.Split('\n').Length - 1);
            }
        }
    }

    [Theory]
    [InlineData("Esqueceu seu café.", "You forgot your coffee.", "Esqueceste-te do teu café.")]
    [InlineData("Eu não tinha esquecido o nome.", null, "Eu não me tinha esquecido do nome.")]
    [InlineData("Você tinha esquecido o nome.", null, "Tinhas-te esquecido do nome.")]
    [InlineData("Não esquece o café.", null, "Não te esqueças do café.")]
    [InlineData("Ela esqueceu um documento.", null, "Ela esqueceu-se de um documento.")]
    [InlineData("Ele está jogando verde.", "He's fishing.", "Ele está à pesca.")]
    [InlineData("Ele está a jogar um verde.", "He's fishing.", "Ele está à pesca.")]
    [InlineData("Ela estava jogando um verde.", "She was fishing.", "Ela estava à pesca.")]
    [InlineData("Ele jogou verde.", "He fished for information.", "Ele pescou.")]
    [InlineData("Ele joga um verde para colher maduro.", "He fishes for information.", "Ele pesca.")]
    [InlineData("Espero que ele pegue a isca.", "I hope he takes the bait.", "Espero que ele morda o isco.")]
    [InlineData("Ele pegou na isca.", "He took the bait.", "Ele mordeu o isco.")]
    [InlineData("Uma xícara de Joe para viagem.", "A cup of joe to go.", "Uma chávena de café para levar.")]
    [InlineData("Há algo que não soma.", "Something's not adding up.", "Há algo que não bate certo.")]
    [InlineData("E não somos jogadores.", "And we ain't no fishermen.", "E não somos pescadores.")]
    [InlineData("Ele está mentindo.", "He's lying.", "Ele está a mentir.")]
    [InlineData("Ele mente.", "He's lying.", "Ele está a mentir.")]
    [InlineData("Ele não mente.", "He isn't lying.", "Ele não está a mentir.")]
    [InlineData("Ela estava falando.", "She was talking.", "Ela estava a falar.")]
    [InlineData("Ele veio ao esquadrão.", "He came by the squad.", "Ele veio à esquadra.")]
    [InlineData("Estou na sala do esquadrão.", "I'm in the squad room.", "Estou na sala da esquadra.")]
    [InlineData("Vou para o distrito policial.", "I'm going to the precinct.", "Vou para a esquadra.")]
    [InlineData("Então decidiu revidar.", "So you decided to get back at him.", "Então decidiste vingar-te.")]
    [InlineData("Ela não revidou.", "She did not get back at him.", "Ela não se vingou.")]
    [InlineData("Ela retrucou.", "She replied.", "Ela respondeu.")]
    [InlineData("Ele está revidando.", "He is getting back at her.", "Ele está a vingar-se.")]
    [InlineData("Num trote de celular?", null, "Numa chamada de brincadeira?")]
    [InlineData("O trote estudantil.", null, "A praxe académica.")]
    [InlineData("Era um trote.", "It was a prank.", "Era uma brincadeira.")]
    [InlineData("Ele passou um trote.", null, "Ele pregou uma partida.")]
    [InlineData("Como divertisse você?", "Like I amuse you?", "Como se eu te divertisse?")]
    [InlineData("Como eu fosse um palhaço?", "Like I'm a clown?", "Como se eu fosse um palhaço?")]
    [InlineData("Como ele não visse você?", "Like he didn't see you?", "Como se ele não te visse?")]
    [InlineData("Ele pisou no meu rosto com sua bota.", "He stepped on my face with his boot.", "Ele pisou-me o rosto com a bota.")]
    [InlineData("Você tinha uma arma na sua mão.", "You did have a gun in your hand.", "Tinhas uma arma na mão.")]
    [InlineData("Ela bateu na minha cara.", null, "Ela bateu-me na cara.")]
    [InlineData("Parti o meu braço.", null, "Parti o braço.")]
    [InlineData("Não, mas não pensou", "No, but you didn't think...", "Não, mas não pensaste")]
    [InlineData("e contou sobre isso.", "and told her about it.", "e contou-lhe sobre isso.")]
    public void RecentRulesWorkAcrossEveryWordBoundary(string text, string? english, string expected)
    {
    var converter = new PortugueseConverter();
    var reference = ReferenceHint.FromText(english);
    Assert.Equal(expected, converter.Convert(text, reference: reference));
    for (var index = 0; index < text.Length; index++)
        {
        if (text[index] != ' ') continue;
        foreach (var newline in new[] { "\n", "\r\n" })
            {
            var wrapped = text[..index] + newline + text[(index + 1)..];
            var converted = converter.Convert(wrapped, reference: reference);
            Assert.True(expected == converted.Replace(newline, " "),
                $"Input: {wrapped.Replace("\r", "\\r").Replace("\n", "\\n")}; output: {converted.Replace("\r", "\\r").Replace("\n", "\\n")}; expected: {expected}");
            Assert.Equal(1, converted.Count(c => c == '\n'));
            Assert.Equal(converted, converter.Convert(converted, reference: reference));
        }
    }
    }

    [Theory]
    [InlineData("- Ele está jogando\n- Verde.", "He's fishing.")]
    [InlineData("Ele está jogando\n\nverde.", "He's fishing.")]
    [InlineData("- Ele pegou\n- A isca.", "He took the bait.")]
    [InlineData("Ele pegou a\n\nisca.", "He took the bait.")]
    [InlineData("- Uma xícara\n- De Joe.", "A cup of joe.")]
    [InlineData("Uma xícara de\n\nJoe.", "A cup of joe.")]
    [InlineData("- Ele\n- mente.", "He's lying.")]
    [InlineData("Ele\n\nmente.", "He's lying.")]
    [InlineData("- na sala\n- do esquadrão.", "In the squad room.")]
    [InlineData("na sala\n\ndo esquadrão.", "In the squad room.")]
    [InlineData("- Ela\n- revidou.", "She got back at him.")]
    [InlineData("Ela\n\nrevidou.", "She got back at him.")]
    public void RecentIdiomsDoNotCrossSpeakersOrBlankLines(string source, string english)
    {
    var converter = new PortugueseConverter();
    Assert.Equal(converter.Convert(source), converter.Convert(source, reference: ReferenceHint.FromText(english)));
    }

    [Theory]
    [InlineData("Ele está jogando\nverde.", "He's\nfishing.", "Ele está\nà pesca.")]
    [InlineData("Ele pegou a\nisca.", "He took\nthe bait.", "Ele mordeu o\nisco.")]
    [InlineData("Há algo que não\nsoma.", "Something's not\nadding up.", "Há algo que não\nbate certo.")]
    [InlineData("Uma xícara de\nJoe.", "A cup of\njoe.", "Uma chávena de\ncafé.")]
    [InlineData("Ele\nmente.", "He's\nlying.", "Ele\nestá a mentir.")]
    [InlineData("Ele veio\nao esquadrão.", "He came by\nthe squad.", "Ele veio\nà esquadra.")]
    [InlineData("Então decidiu\nrevidar.", "So you decided to get\nback at him.", "Então decidiste\nvingar-te.")]
    [InlineData("Como divertisse\nvocê?", "Like I\namuse you?", "Como se eu\nte divertisse?")]
    [InlineData("Ele pisou no meu\nrosto com sua bota.", "He stepped on my face\nwith his boot.", "Ele pisou-me\no rosto com a bota.")]
    public void ReferenceAndPortugueseMayBothWrap(string source, string english, string expected)
    {
    var converter = new PortugueseConverter();
    Assert.Equal(expected, converter.Convert(source, reference: ReferenceHint.FromText(english)));
    }

    [Theory]
    [InlineData("- Não\n- esquece o café.")]
    [InlineData("Não\n\nesquece o café.")]
    public void NegativeCommandDoesNotCrossSpeakersOrBlankLines(string source)
    {
    var rules = new List<RuleChange>();
    new PortugueseConverter().Convert(source, trace: rules);
    Assert.DoesNotContain(rules, r => r.Id == "address");
    }

    [Theory]
    [InlineData("- Espero que ele sabe.\n- Eu também.", "- Espero que ele saiba.\n- Eu também.")]
    [InlineData("-Talvez\n-Passou muitas noites.", "-Talvez\n-Passou muitas noites.")]
    [InlineData("Talvez.\nPassou muitas noites.", "Talvez.\nPassou muitas noites.")]
    [InlineData("Ele\n\nvirou médico.", "Ele\n\ntornou-se médico.")]
    [InlineData("<i>Ele virou\nmédico.</i>", "<i>Ele tornou-se\nmédico.</i>")]
    [InlineData("Ele virou \n  médico.", "Ele tornou-se \n  médico.")]
    public void SpeakerBoundariesBlankLinesAndTagsSurvive(string text, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(text));

    [Theory]
    [InlineData("Ele gostou que\neu dirigi.", "Ele gostou de ser\neu a conduzir.")]
    [InlineData("Ele gostou de ser\neu a conduzir.", "Ele gostou de ser\neu a conduzir.")]
    [InlineData("Ela gostou de ser\nela a pagar.", "Ela gostou de ser\nela a pagar.")]
    [InlineData("Eu a vi.", "Eu vi-a.")]
    public void RestoredBreakDoesNotTurnInfinitivalPrepositionIntoClitic(string text, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(text));
        Assert.Equal(expected, converter.Convert(expected));
    }
}
