using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class ExplanationTests
{
    [Theory]
    [InlineData("Sim, mas acompanhe,\nporque esse é o pulo.", "Yeah, but stay with me, 'cause this is the leap.",
        "Sim, mas acompanhe-me,\nporque este é o salto.")]
    [InlineData("Sim, mas acompanhe-me,\nporque esse é o pulo.", "Yeah, but stay with me, 'cause this is the leap.",
        "Sim, mas acompanhe-me,\nporque este é o salto.")]
    [InlineData("Acompanha, este é o pulo.", "Follow me, this is the leap.", "Acompanha-me, este é o salto.")]
    [InlineData("Acompanhem, este é o pulo.", "Stay with me, this is the leap.", "Acompanhem-me, este é o salto.")]
    [InlineData("Acompanhe, vou explicar.", "Follow my reasoning, I will explain.", "Acompanha-me, vou explicar.")]
    [InlineData("Isto é um grande pulo.", "This is a big leap in reasoning.", "Isto é um grande salto.")]
    [InlineData("<i>Acompanhe, este é o pulo.</i>", "Stay with me, this is the leap.", "<i>Acompanha-me, este é o salto.</i>")]
    [InlineData("ACOMPANHE, ESTE É O PULO.", "Stay with me, this is the leap.", "ACOMPANHA-ME, ESTE É O SALTO.")]
    public void FollowAnExplanationAndReasoningLeap(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: hint));
        Assert.Equal(expected, converter.Convert(expected, reference: hint));
    }

    [Theory]
    [InlineData("Sim, mas acompanhe, porque esse é o pulo.", null)]
    [InlineData("Acompanhe, este é o pulo.", "Stay with me on the roof before the leap.")]
    [InlineData("Acompanhe a cantora, este é o pulo.", "Accompany the singer, this is the leap.")]
    [InlineData("Não acompanhe, este é o pulo.", "Stay with me, this is the leap.")]
    [InlineData("Acompanhe, é um pulo de fé.", "Stay with me, it is a leap of faith.")]
    [InlineData("Acompanhe, este é o pulo.", "Stay with me, this is a physical leap.")]
    public void UnconfirmedPhysicalAndCompleteComplementsDoNotGainAnObject(string source, string? english)
    {
        var converted = new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english));
        Assert.DoesNotContain("-me", converted);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AddressContinuesAcrossCuesWithoutRequiringSentenceContext(bool sentenceContext)
    {
        var blocks = Scene();
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(SentenceContext: sentenceContext),
            english: new string?[] { "What if you got it in your head", "to seek revenge in kind?" });
        Assert.Equal("E se meteste\nna tua cabeça", result[0].Text);
        Assert.Equal("que terias a tua vingança?", result[1].Text);
        Assert.Contains(result[1].Rules, r => r.Id == "address");
        Assert.Equal(1195438, blocks[1].StartMs);
    }

    [Theory]
    [InlineData("Você pensa", "que precisaria de ajuda.", "que precisarias de ajuda.")]
    [InlineData("Tu disseste", "que teria sua resposta.", "que terias a tua resposta.")]
    [InlineData("Você acredita", "que conseguiria vencer.", "que conseguirias vencer.")]
    [InlineData("Tu imaginas", "que estaria pronto.", "que estarias pronto.")]
    [InlineData("Você pensa", "que ela teria razão.", "que ela teria razão.")]
    [InlineData("Você disse ao João", "que teria razão.", "que teria razão.")]
    [InlineData("Ela pensa", "que teria razão.", "que teria razão.")]
    [InlineData("Você disse.", "que teria razão.", "que teria razão.")]
    public void ReviewedGoverningClausesPropagateOnlyTheSameSubject(string previous, string current, string expected)
    {
        var blocks = Scene();
        blocks[0].Text = previous;
        blocks[1].Text = current;
        Assert.Equal(expected, SubtitleConversion.Convert(blocks, ConversionOptions.Default,
            english: new string?[] { null, expected != current ? "that you would continue." : null })[1].Text);
    }

    [Theory]
    [InlineData("Você pensa que teria sua resposta.", "You think you would have your answer.", "Pensas que terias a tua resposta.")]
    [InlineData("Tu acreditas que conseguiria vencer.", "You believe you would manage to win.", "Tu acreditas que conseguirias vencer.")]
    public void SameSentenceKeepsSubjectAcrossQue(string source, string english, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Fact]
    public void AnUnresolvedReportedSubjectIsNotAssumedToBeTheAddressee()
    {
        var blocks = Scene();
        blocks[0].Text = "Você pensa";
        blocks[1].Text = "que precisaria de ajuda.";
        Assert.Equal(blocks[1].Text, SubtitleConversion.Convert(blocks, ConversionOptions.Default)[1].Text);
    }

    [Theory]
    [InlineData("que teria ele?")]
    [InlineData("que chegaria a polícia.")]
    [InlineData("que se faria justiça.")]
    public void PostposedAndImpersonalSubjectsAreNotInherited(string current)
    {
        var blocks = Scene();
        blocks[1].Text = current;
        var result = SubtitleConversion.Convert(blocks, ConversionOptions.Default)[1];
        Assert.Equal(current, result.Text);
    }

    [Fact]
    public void SceneSpeakerRegisterAndOptionBoundariesAreRespected()
    {
        foreach (var options in new[]
        {
            new ConversionOptions(Address: AddressMode.Formal),
            new ConversionOptions(YouToTu: false),
            new ConversionOptions(DisabledRules: "address"),
        })
            Assert.StartsWith("que teria ", SubtitleConversion.Convert(Scene(), options)[1].Text);
        Assert.StartsWith("que teria ", SubtitleConversion.Convert(Scene(), ConversionOptions.Default,
            speakers: new string?[] { "Frank", "Erin" })[1].Text);
        var distant = Scene();
        distant[1].StartMs += 10000;
        distant[1].EndMs += 10000;
        Assert.StartsWith("que teria ", SubtitleConversion.Convert(distant, ConversionOptions.Default)[1].Text);
        Assert.Equal("que teria a sua vingança?", SubtitleConversion.Convert(Scene(), ConversionOptions.Default,
            english: new string?[] { "What if you got it in your head", "that he would have his revenge?" })[1].Text);
        var formal = ReferenceHint.FromText("Stay with me, this is the leap.")! with { FormalAddress = true };
        Assert.Equal("Acompanhe-me, este é o salto.", new PortugueseConverter().Convert(
            "Acompanhe, este é o pulo.", reference: formal));
    }

    [Fact]
    public void ExplanationOptionsAndLocalOverridesAreRespected()
    {
        const string text = "Acompanhe, este é o pulo.";
        var hint = ReferenceHint.FromText("Stay with me, this is the leap.");
        foreach (var options in new[]
        {
            new ConversionOptions(Colloquial: false),
            new ConversionOptions(DisabledRules: "explanation-constructions"),
        })
        {
            var rules = new List<RuleChange>();
            new PortugueseConverter(options).Convert(text, reference: hint, trace: rules);
            Assert.DoesNotContain(rules, r => r.Id == "explanation-constructions");
        }
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("pulo");
        words.Ignored.Add("acompanhar");
        Assert.DoesNotContain("-me", new PortugueseConverter(localWordList: words).Convert(text, reference: hint));
        Assert.Contains("pulo", new PortugueseConverter(localWordList: words).Convert(text, reference: hint));
    }

    private static SrtBlock[] Scene() =>
    [
        new() { StartMs = 1192973, EndMs = 1195265, Text = "E se você meteu\nna sua cabeça" },
        new() { StartMs = 1195438, EndMs = 1197064, Text = "que teria sua vingança?" },
    ];
}
