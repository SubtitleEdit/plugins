using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class EmergencyReferenceTests
{
    [Theory]
    [InlineData("Bem, quando atenderam\nno clube, tiveram informação",
        "Well, when you responded to the club, you got some information",
        "Bem, quando responderam\nno clube, tiveram informação")]
    [InlineData("Nós atendemos na escola.", "We responded to the school.", "Nós respondemos na escola.")]
    [InlineData("Ele atendeu no acidente.", "He responded to the accident.", "Ele respondeu no acidente.")]
    [InlineData("Estavam a atender no local.", "They were responding to the scene.",
        "Estavam a responder no local.")]
    [InlineData("de um rapaz cuja namorada\nteve uma parada.",
        "from a kid whose girlfriend went into arrest.",
        "de um rapaz cuja namorada\nteve uma paragem cardíaca.")]
    [InlineData("Ela sofreu uma parada.", "She suffered a cardiac arrest.",
        "Ela sofreu uma paragem cardíaca.")]
    [InlineData("Ele teve uma paragem.", "He had a cardiorespiratory arrest.",
        "Ele teve uma paragem cardiorrespiratória.")]
    [InlineData("Ela teve uma parada.", "She went into cardiac arrest.",
        "Ela teve uma paragem cardíaca.")]
    [InlineData("ELA TEVE UMA PARADA.", "She went into arrest.", "ELA TEVE UMA PARAGEM CARDÍACA.")]
    [InlineData("Ele tinha uma parada.", "He was going into arrest.", "Ele tinha uma paragem cardíaca.")]
    [InlineData("Sim, isto funcionaria?", "Yeah, would that work?", "Sim, isso funcionaria?")]
    [InlineData("Isto é ótimo.", "That's great.", "Isso é ótimo.")]
    [InlineData("O que isto tem a ver connosco?", "What's that got to do with us?",
        "O que isso tem a ver connosco?")]
    [InlineData("Não sabia disto.", "I didn't know about that.", "Não sabia disso.")]
    [InlineData("Pensei nisto.", "I thought of that.", "Pensei nisso.")]
    public void ConfirmedMeaningsAreGeneralizedAndStable(string source, string english, string expected)
    {
        var hint = ReferenceHint.FromText(english);
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source, reference: hint));
        Assert.Equal(expected, converter.Convert(expected, reference: hint));
    }

    [Theory]
    [InlineData("Ela atendeu no clube.", "She answered the phone at the club.", "Ela atendeu no clube.")]
    [InlineData("Ele atendeu no hotel.", "He responded to the letter at the hotel.", "Ele atendeu no hotel.")]
    [InlineData("Ela atendeu no hospital.", null, "Ela atendeu no hospital.")]
    [InlineData("Ela teve uma parada.", null, "Ela teve uma parada.")]
    [InlineData("Ela teve uma parada.", "She had a stop.", "Ela teve uma paragem.")]
    [InlineData("Ela teve uma parada.", "She was under arrest.", "Ela teve uma parada.")]
    [InlineData("Isto funcionaria?", null, "Isto funcionaria?")]
    [InlineData("Isto funcionaria?", "Would this work?", "Isto funcionaria?")]
    [InlineData("Isto é o que ele disse.", "This is what he said that day.", "Isto é o que ele disse.")]
    [InlineData("Ele disse isto.", "He said that he was tired.", "Ele disse isto.")]
    [InlineData("Isto é bom.", "This is good, that is bad.", "Isto é bom.")]
    [InlineData("Este plano funcionaria?", "Would that plan work?", "Este plano funcionaria?")]
    [InlineData("Ele compara isto ao plano.", "He compares it to that plan.", "Ele compara isto ao plano.")]
    [InlineData("Isto faz aquele plano funcionar.", "It makes that plan work.", "Isto faz aquele plano funcionar.")]
    public void OtherReadingsStayProtected(string source, string? english, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Fact]
    public void OptionsAndIgnoredPhrasesAreRespected()
    {
        var arrest = ReferenceHint.FromText("She went into arrest.");
        Assert.Equal("Ela teve uma parada.", new PortugueseConverter(new ConversionOptions(Vocabulary: false))
            .Convert("Ela teve uma parada.", reference: arrest));
        Assert.Equal("Ela teve uma parada.", new PortugueseConverter(new ConversionOptions(DisabledRules: "emergency-constructions"))
            .Convert("Ela teve uma parada.", reference: arrest));
        Assert.Equal("Isto funcionaria?", new PortugueseConverter(new ConversionOptions(DisabledRules: "reference-clitics"))
            .Convert("Isto funcionaria?", reference: ReferenceHint.FromText("Would that work?")));
        var list = new PortugueseLocalWordList();
        list.Ignored.Add("teve uma parada");
        list.Ignored.Add("isto");
        var converter = new PortugueseConverter(localWordList: list);
        Assert.Equal("Ela teve uma parada.", converter.Convert("Ela teve uma parada.", reference: arrest));
        Assert.Equal("Isto funcionaria?", converter.Convert("Isto funcionaria?",
            reference: ReferenceHint.FromText("Would that work?")));
    }

    [Fact]
    public void SubtitlePipelineUsesMedicalSenseWithoutCrossCueGuessing()
    {
        var blocks = SubRipParser.Parse(
            "1\n00:00:01,000 --> 00:00:03,000\nBem, quando atenderam\nno clube, tiveram informação\n\n" +
            "2\n00:00:03,010 --> 00:00:05,000\nde um rapaz cuja namorada\nteve uma parada.\n\n" +
            "3\n00:01:01,000 --> 00:01:03,000\nSim, isto funcionaria?\n\n");
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(), english: new string?[] {
            "Well, when you responded to the club, you got some information",
            "from a kid whose girlfriend went into arrest.", "Yeah, would that work?" });
        Assert.Equal("Bem, quando responderam\nno clube, tiveram informação", result[0].Text);
        Assert.Equal("de um rapaz cuja namorada\nteve uma paragem cardíaca.", result[1].Text);
        Assert.Equal("Sim, isso funcionaria?", result[2].Text);
    }
}
