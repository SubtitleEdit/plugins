using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class InjuryStateTests
{
    [Theory]
    [InlineData("a perna esquerda do\nmeu padrinho foi lesionada.", "a perna esquerda do\nmeu padrinho ficou lesionada.")]
    [InlineData("A perna esquerda do meu padrinho foi lesionada.", "A perna esquerda do meu padrinho ficou lesionada.")]
    [InlineData("O braço direito foi magoado.", "O braço direito ficou magoado.")]
    [InlineData("As pernas foram lesionadas.", "As pernas ficaram lesionadas.")]
    [InlineData("Os joelhos do meu pai foram feridos.", "Os joelhos do meu pai ficaram feridos.")]
    [InlineData("A mão foi gravemente lesionada.", "A mão ficou gravemente lesionada.")]
    [InlineData("O tornozelo foi ligeiramente magoado.", "O tornozelo ficou ligeiramente magoado.")]
    [InlineData("As costas foram lesionadas no acidente.", "As costas ficaram lesionadas no acidente.")]
    [InlineData("O pescoço foi lesionado.", "O pescoço ficou lesionado.")]
    [InlineData("O tendão foi lesionado.", "O tendão ficou lesionado.")]
    [InlineData("Os tendões foram lesionados.", "Os tendões ficaram lesionados.")]
    public void AnatomicalInjuriesUseResultState(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Theory]
    [InlineData("A perna foi lesionada pelo agressor.")]
    [InlineData("A perna foi lesionada por uma bala.")]
    [InlineData("A perna foi lesionada de propósito.")]
    [InlineData("A perna foi lesionada intencionalmente.")]
    [InlineData("O suspeito foi ferido.")]
    [InlineData("A perna não foi lesionada.")]
    [InlineData("A perna já estava lesionada.")]
    [InlineData("As pernas foi lesionada.")]
    [InlineData("A perna que foi lesionada.")]
    [InlineData("A perna foi examinada.")]
    [InlineData("A perna e o braço foram lesionados.")]
    public void EventivePassivesNegationAndOtherMeaningsAreNotRewritten(string source) =>
        Assert.Equal(source, AutomaticGrammar.Apply(source, ConversionOptions.Default));

    [Theory]
    [InlineData("a perna do\nmeu padrinho foi lesionada.", "a perna do\nmeu padrinho ficou lesionada.")]
    [InlineData("a casa da\nminha irmã ficou bonita.", "a casa da\nminha irmã ficou bonita.")]
    [InlineData("o carro dos\nmeus amigos chegou.", "o carro dos\nmeus amigos chegou.")]
    [InlineData("estou no\nmeu quarto.", "estou no\nmeu quarto.")]
    [InlineData("ela está na\nsua casa.", "ela está na\nsua casa.")]
    [InlineData("olha para o\nmeu carro.", "olha para o\nmeu carro.")]
    [InlineData("Ele falou do\nmeu bebé!", "Ele falou do\nmeu bebé!")]
    public void WrappedPossessivesDoNotDuplicateExistingArticles(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Fact]
    public void ColloquialOptionControlsResultativePreference()
    {
        var converter = new PortugueseConverter(new ConversionOptions(Colloquial: false));
        Assert.Equal("A perna foi lesionada.", converter.Convert("A perna foi lesionada."));
    }
}
