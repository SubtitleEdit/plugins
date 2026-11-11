using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class SpokenIdiomTests
{
    [Theory]
    [InlineData("ele sempre fala bobagens\ntoda vez que fica doidão.",
        "'cause he always talking crazy every time he gets high.",
        "ele diz sempre tolices\nquando fica pedrado.")]
    [InlineData("Ele fala bobagens.", null, "Ele diz tolices.")]
    [InlineData("Ela fala tolices.", null, "Ela diz tolices.")]
    [InlineData("Eles falam disparates.", null, "Eles dizem disparates.")]
    [InlineData("Eu falo bobagens.", null, "Eu digo tolices.")]
    [InlineData("Tu falas bobagens.", null, "Tu dizes tolices.")]
    [InlineData("Eu falei bobagens.", null, "Eu disse tolices.")]
    [InlineData("Tu falaste bobagens.", null, "Tu disseste tolices.")]
    [InlineData("Ela falou bobagens.", null, "Ela disse tolices.")]
    [InlineData("Eles falaram bobagens.", null, "Eles disseram tolices.")]
    [InlineData("Eu falava bobagens.", null, "Eu dizia tolices.")]
    [InlineData("Nós falávamos bobagens.", null, "Nós dizíamos tolices.")]
    [InlineData("Eu falarei bobagens.", null, "Eu direi tolices.")]
    [InlineData("Ele falaria bobagens.", null, "Ele diria tolices.")]
    [InlineData("Não fale bobagens.", null, "Não digas tolices.")]
    [InlineData("Se falassem bobagens.", null, "Se dissessem tolices.")]
    [InlineData("Se eles falarem bobagens.", null, "Se eles disserem tolices.")]
    [InlineData("Se eles não falarem bobagens.", null, "Se eles não disserem tolices.")]
    [InlineData("Se não falar bobagens.", null, "Se não disser tolices.")]
    [InlineData("Quando falar bobagens.", null, "Quando disser tolices.")]
    [InlineData("É melhor falar bobagens.", null, "É melhor dizer tolices.")]
    [InlineData("Não quero falar bobagens.", null, "Não quero dizer tolices.")]
    [InlineData("Nós falamos bobagens.", "We talk nonsense.", "Nós dizemos tolices.")]
    [InlineData("Nós falamos bobagens.", "We talked nonsense.", "Nós dissemos tolices.")]
    [InlineData("Ele fala muitas bobagens.", null, "Ele diz muitas tolices.")]
    [InlineData("Ele fala muitos disparates.", null, "Ele diz muitos disparates.")]
    [InlineData("Ele fala besteiras.", null, "Ele diz asneiras.")]
    [InlineData("Ele só fala bobagens.", null, "Ele só diz tolices.")]
    [InlineData("ELE SEMPRE FALA BOBAGENS.", null, "ELE DIZ SEMPRE TOLICES.")]
    [InlineData("<i>Ele sempre fala bobagens.</i>", null, "<i>Ele diz sempre tolices.</i>")]
    public void NonsenseUsesDizerWithMatchingPersonAndTense(string source, string? english, string expected) =>
        AssertConversion(source, english, expected);

    [Theory]
    [InlineData("Toda vez que chove, fico em casa.", "Sempre que chove, fico em casa.")]
    [InlineData("Toda a vez que chove, fico em casa.", "Sempre que chove, fico em casa.")]
    [InlineData("Ele sorri toda vez que chove.", "Ele sorri sempre que chove.")]
    [InlineData("Ele sempre sorri toda vez que chove.", "Ele sempre sorri quando chove.")]
    [InlineData("Ele não sorri sempre toda vez que chove.", "Ele não sorri sempre de cada vez que chove.")]
    [InlineData("Ele sempre diz que toda vez que chove, fica em casa.", "Ele diz sempre que sempre que chove, fica em casa.")]
    [InlineData("TODA VEZ QUE CHOVE.", "SEMPRE QUE CHOVE.")]
    [InlineData("Todas as vezes que chove.", "Todas as vezes que chove.")]
    [InlineData("De cada vez que chove.", "De cada vez que chove.")]
    public void HabitualClausesKeepUniversalMeaning(string source, string expected) =>
        AssertConversion(source, null, expected);

    [Theory]
    [InlineData("Ele fica doidão.", "He gets high.", "Ele fica pedrado.")]
    [InlineData("Ela ficou doidona.", "She got high.", "Ela ficou pedrada.")]
    [InlineData("Eles ficaram doidões.", "They got stoned.", "Eles ficaram pedrados.")]
    [InlineData("Elas estão doidonas.", "They are high.", "Elas estão pedradas.")]
    [InlineData("Eu estou muito doidão.", "I'm really high.", "Eu estou muito pedrado.")]
    [InlineData("Ele estava doidão.", "He was stoned.", "Ele estava pedrado.")]
    [InlineData("Ele vai ficar doidão.", "He will get high.", "Ele vai ficar pedrado.")]
    [InlineData("Ele ficou doidão de cocaína.", null, "Ele ficou pedrado de cocaína.")]
    [InlineData("Ele ficou doidão de muita cocaína.", null, "Ele ficou pedrado de muita cocaína.")]
    [InlineData("Ele ficou doidão de erva.", null, "Ele ficou pedrado de erva.")]
    [InlineData("ELE FICA DOIDÃO.", "He gets high.", "ELE FICA PEDRADO.")]
    [InlineData("<i>Ela ficou doidona.</i>", "She got high.", "<i>Ela ficou pedrada.</i>")]
    public void DrugEvidenceSelectsGenderedIntoxicationSlang(string source, string? english, string expected) =>
        AssertConversion(source, english, expected);

    [Theory]
    [InlineData("Ele fica doidão.", null, "Ele fica doidão.")]
    [InlineData("Ele fica doidão.", "He goes crazy.", "Ele fica doidão.")]
    [InlineData("Ele fica doidão.", "He gets high scores.", "Ele fica doidão.")]
    [InlineData("Ele fica doidão.", "He gets high up in the mountains.", "Ele fica doidão.")]
    [InlineData("Ele fica doidão.", "He is high on life.", "Ele fica doidão.")]
    [InlineData("Ele fica doidão de alegria.", "He gets high on life.", "Ele fica doidão de alegria.")]
    [InlineData("Ele combate as drogas e fica doidão.", "He fights drugs and goes crazy.", "Ele combate as drogas e fica doidão.")]
    [InlineData("Ele fica doidão por ela.", "He gets high. He's crazy about her.", "Ele fica doidão por ela.")]
    [InlineData("Ele fala português.", "He speaks Portuguese.", "Ele fala português.")]
    [InlineData("Ele fala de bobagens.", null, "Ele fala de tolices.")]
    [InlineData("Nós falamos bobagens.", null, "Nós falamos tolices.")]
    [InlineData("Toda a vez foi emocionante.", null, "Toda a vez foi emocionante.")]
    public void UnrelatedAndAmbiguousSensesAreProtected(string source, string? english, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Fact]
    public void OptionsAndLocalChoicesWin()
    {
        var high = ReferenceHint.FromText("He gets high.");
        foreach (var options in new[]
        {
            new ConversionOptions(Colloquial: false),
            new ConversionOptions(DisabledRules: "spoken-idioms"),
        })
        {
            Assert.Equal("Ele fica doidão.", new PortugueseConverter(options).Convert("Ele fica doidão.", reference: high));
            Assert.Equal("Toda vez que chove.", new PortugueseConverter(options).Convert("Toda vez que chove."));
        }
        var noVocabulary = new PortugueseConverter(new ConversionOptions(Vocabulary: false));
        Assert.Equal("Ele fica doidão.", noVocabulary.Convert("Ele fica doidão.", reference: high));
        Assert.Equal("Ele fala bobagens.", noVocabulary.Convert("Ele fala bobagens."));
        Assert.Equal("Sempre que chove.", noVocabulary.Convert("Toda vez que chove."));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("fala bobagens");
        words.Ignored.Add("toda vez que");
        words.Words.Add(("doidão", "ganzado"));
        var converter = new PortugueseConverter(localWordList: words);
        Assert.Equal("Ele fala bobagens.", converter.Convert("Ele fala bobagens."));
        Assert.Equal("toda vez que chove.", converter.Convert("toda vez que chove."));
        Assert.Equal("Ele fica ganzado.", converter.Convert("Ele fica doidão.", reference: high));
        var speechWords = new PortugueseLocalWordList();
        speechWords.Words.Add(("falar", "conversar"));
        Assert.Equal("Ele fala tolices.", new PortugueseConverter(localWordList: speechWords).Convert("Ele fala bobagens."));
    }

    [Fact]
    public void ExactCuePreservesTimingAndLineCount()
    {
        var cue = new SrtBlock { StartMs = 1667747, EndMs = 1670344,
            Text = "ele sempre fala bobagens\ntoda vez que fica doidão." };
        var result = Assert.Single(SubtitleConversion.Convert(new[] { cue }, ConversionOptions.Default,
            english: new string?[] { "'cause he always talking crazy every time he gets high." }));
        Assert.Equal("ele diz sempre tolices\nquando fica pedrado.", result.Text);
        Assert.Equal(1667747, cue.StartMs);
        Assert.Equal(1670344, cue.EndMs);
    }

    private static void AssertConversion(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter();
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: hint));
        Assert.Equal(expected, converter.Convert(expected, reference: hint));
    }
}
