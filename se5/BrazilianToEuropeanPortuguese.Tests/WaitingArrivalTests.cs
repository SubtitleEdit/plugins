using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class WaitingArrivalTests
{
    [Theory]
    [InlineData("temos de esperar o Jamie\nantes de comer.", "temos de esperar pelo Jamie\nantes de comer.")]
    [InlineData("Esperamos a Ana.", "Esperamos pela Ana.")]
    [InlineData("Esperava o João.", "Esperava pelo João.")]
    [InlineData("Esperava João.", "Esperava pelo João.")]
    [InlineData("Esperava Maria.", "Esperava pela Maria.")]
    [InlineData("Vou esperar Jamie.", "Vou esperar por Jamie.")]
    [InlineData("Vou esperar os meus amigos.", "Vou esperar pelos meus amigos.")]
    [InlineData("Vou esperar as minhas amigas.", "Vou esperar pelas minhas amigas.")]
    [InlineData("Temos de esperar o médico.", "Temos de esperar pelo médico.")]
    [InlineData("Precisamos esperar a Maria.", "Precisamos de esperar pela Maria.")]
    [InlineData("Não vou esperar o Jamie.", "Não vou esperar pelo Jamie.")]
    [InlineData("TEMOS DE ESPERAR O JAMIE.", "TEMOS DE ESPERAR PELO JAMIE.")]
    [InlineData("<i>Temos de esperar o Jamie.</i>", "<i>Temos de esperar pelo Jamie.</i>")]
    public void ReviewedPersonalObjectsUsePor(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Theory]
    [InlineData("o", "pelo")]
    [InlineData("a", "pela")]
    [InlineData("none", "por")]
    public void BareNamesRespectTheSelectedArticle(string article, string preposition) =>
        Assert.Equal($"Vamos esperar {preposition} Jamie.", new PortugueseConverter(
            new ConversionOptions(NameArticles: $"Jamie = {article}")).Convert("Vamos esperar Jamie."));

    [Theory]
    [InlineData("Espero que o Jamie chegue.")]
    [InlineData("Vou esperar o resultado.")]
    [InlineData("Temos de esperar pelo Jamie.")]
    [InlineData("Temos de esperar o Jamie chegar.")]
    [InlineData("Vou esperar por alguém.")]
    public void OtherComplementsStayUnchanged(string source) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source));

    [Theory]
    [InlineData("Jamie", "JAMIE", false)]
    [InlineData("Maria", "MARIA", false)]
    [InlineData("Jamie", "JAMIE", true)]
    public void NamedArrivalApologizesToTheWaitingGroup(string name, string speaker, bool sentenceContext)
    {
        var blocks = Scene(name);
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(SentenceContext: sentenceContext),
            english: English(name, speaker));
        Assert.Equal($"temos de esperar {(name == "Maria" ? "pela" : "pelo")} {name}\nantes de comer.", result[0].Text);
        Assert.Equal("- Não.\n- Ei! Desculpem!", result[2].Text);
        Assert.Contains(result[2].Rules, r => r.Id == "arrival-group-apology");
        Assert.Contains("verify the recipients", result[2].ContextNote);
        Assert.Equal(2265703, blocks[2].StartMs);
        Assert.Equal(2267695, blocks[2].EndMs);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("No. Hey! Sorry!")]
    [InlineData("No. JOHN: Hey! Sorry!")]
    public void AlguemAloneDoesNotEstablishPluralRecipients(string? arrival)
    {
        var english = English("Jamie", "JAMIE");
        english[2] = arrival;
        var result = SubtitleConversion.Convert(Scene("Jamie"), ConversionOptions.Default, english: english);
        Assert.Equal("- Não.\n- Ei! Desculpa!", result[2].Text);
        Assert.DoesNotContain(result[2].Rules, r => r.Id == "arrival-group-apology");
    }

    [Theory]
    [InlineData("temos que esperar o Jamie\nantes de comer.", "we have to wait\ntill Jamie gets home to eat.", true)]
    [InlineData("esperamos o Jamie.", "We are waiting for Jamie.", true)]
    [InlineData("Não temos que esperar o Jamie.", "We don't have to wait for Jamie.", false)]
    [InlineData("temos que esperar o Jamie.", "We don't have to wait for Jamie.", false)]
    [InlineData("Tenho que esperar o Jamie.", "I have to wait for Jamie.", false)]
    public void ActualMarkupModalVariantsAndNegation(string waiting, string reference, bool plural)
    {
        var blocks = Scene("Jamie");
        blocks[0].Text = waiting;
        blocks[2].Text = "- Não.\n<i>- Ei! Desculpe!</i>";
        var english = English("Jamie", "JAMIE");
        english[0] = reference;
        english[2] = "No.JAMIE: Hey! Sorry!";
        var result = SubtitleConversion.Convert(blocks, ConversionOptions.Default, english: english);
        Assert.Equal($"- Não.\n<i>- Ei! {(plural ? "Desculpem" : "Desculpa")}!</i>", result[2].Text);
    }

    [Fact]
    public void SingularVocativeDistantSceneAndOptionsPreventInference()
    {
        var blocks = Scene("Jamie");
        var english = English("Jamie", "JAMIE");
        blocks[2].Text = "- Não.\n- Ei! Desculpa, pai!";
        Assert.Equal("- Não.\n- Ei! Desculpa, pai!",
            SubtitleConversion.Convert(blocks, ConversionOptions.Default, english: english)[2].Text);
        blocks = Scene("Jamie");
        blocks[2].StartMs += 20000;
        blocks[2].EndMs += 20000;
        Assert.DoesNotContain(SubtitleConversion.Convert(blocks, ConversionOptions.Default, english: english)[2].Rules,
            r => r.Id == "arrival-group-apology");
        foreach (var options in new[]
        {
            new ConversionOptions(Colloquial: false),
            new ConversionOptions(Pronouns: false),
            new ConversionOptions(DisabledRules: "arrival-group-apology"),
        })
            Assert.Equal("- Não.\n- Ei! Desculpa!",
                SubtitleConversion.Convert(Scene("Jamie"), options, english: english)[2].Text);
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("Desculpa");
        words.Ignored.Add("esperar");
        var result = SubtitleConversion.Convert(Scene("Jamie"), ConversionOptions.Default, words, english);
        Assert.Equal("- Não.\n- Ei! Desculpa!", result[2].Text);
        Assert.Equal("temos de esperar o Jamie\nantes de comer.", result[0].Text);
        Assert.Equal("Temos de esperar o Jamie.", new PortugueseConverter(
            new ConversionOptions(DisabledRules: "waiting-for-people")).Convert("Temos de esperar o Jamie."));
    }

    private static SrtBlock[] Scene(string name) =>
    [
        new() { StartMs = 2260257, EndMs = 2262581, Text = $"temos de esperar {(name == "Maria" ? "a" : "o")} {name}\nantes de comer." },
        new() { StartMs = 2262583, EndMs = 2265571, Text = "- Não vai mudar nada.\n- Alguém sabe dele?" },
        new() { StartMs = 2265703, EndMs = 2267695, Text = "- Não.\n- Ei! Desculpa!" },
    ];

    private static string?[] English(string name, string speaker) =>
    [
        $"we have to wait till {name} gets home to eat.",
        "No, you're not. Anybody heard from him?",
        $"No. {speaker}: Hey! Sorry!",
    ];
}
