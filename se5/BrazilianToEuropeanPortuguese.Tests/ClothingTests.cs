using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class ClothingTests
{
    private static readonly PortugueseConverter Converter = new();

    [Theory]
    [InlineData("Tira a fantasia.", "Ditch the leotard.", "Tira o maillot.")]
    [InlineData("A fantasia vermelha.", "The red leotard.", "O maillot vermelho.")]
    [InlineData("As fantasias estão novas.", "The leotards are new.", "Os maillots estão novos.")]
    [InlineData("A minha fantasia está rasgada.", "My leotard is torn.", "O meu maillot está rasgado.")]
    [InlineData("<i>Tira a fantasia.</i>", "Ditch the leotard.", "<i>Tira o maillot.</i>")]
    [InlineData("TIRA A FANTASIA.", "Ditch the leotard.", "TIRA O MAILLOT.")]
    [InlineData("O collant azul.", "The blue leotard.", "O maillot azul.")]
    [InlineData("Os colants pretos.", "The black leotards.", "Os maillots pretos.")]
    [InlineData("Veste o agasalho.", "Put on the tracksuit.", "Veste o fato de treino.")]
    [InlineData("Os agasalhos novos.", "The new tracksuits.", "Os fatos de treino novos.")]
    [InlineData("O macacão preto.", "The black wetsuit.", "O fato de mergulho preto.")]
    [InlineData("Os macacões pretos.", "The black wetsuits.", "Os fatos de mergulho pretos.")]
    [InlineData("A camisa branca.", "The white golf shirt.", "O polo branco.")]
    [InlineData("As camisas novas.", "The new polo shirts.", "Os polos novos.")]
    [InlineData("A camiseta branca.", "The white golf shirt.", "O polo branco.")]
    [InlineData("A camisa vermelha.", "The red cycling jersey.", "A camisola de ciclismo vermelha.")]
    [InlineData("A camiseta vermelha.", "The red cycling jersey.", "A camisola de ciclismo vermelha.")]
    [InlineData("Os culotes brancos.", "The white riding breeches.", "As calças de equitação brancas.")]
    [InlineData("Os culotes brancos.", "The white jodhpurs.", "As calças de equitação brancas.")]
    public void EnglishSportswearResolvesAmbiguousBrazilianGarments(string source, string english, string expected)
    {
        var reference = ReferenceHint.FromText(english);
        var trace = new List<RuleChange>();
        Assert.Equal(expected, Converter.Convert(source, null, reference, trace));
        Assert.Contains(trace, r => r.Id == "sports-clothing");
        Assert.Equal(expected, Converter.Convert(expected, null, reference));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Vocabulary: false, YouToTu: false))
            .Convert(source, null, reference));
        var disabledTrace = new List<RuleChange>();
        new PortugueseConverter(new ConversionOptions(DisabledRules: "sports-clothing"))
            .Convert(source, null, reference, disabledTrace);
        Assert.DoesNotContain(disabledTrace, r => r.Id == "sports-clothing");
    }

    [Theory]
    [InlineData("Tira a fantasia.", null)]
    [InlineData("Tira a fantasia.", "Take off the costume.")]
    [InlineData("Tira a fantasia.", "Take off the dress.")]
    [InlineData("Tira a fantasia.", "Take off the leotard and the costume.")]
    [InlineData("Tira a fantasia.", "Take off the leotards.")]
    [InlineData("As fantasias.", "The leotard.")]
    [InlineData("Uma fantasia e outra fantasia.", "A leotard.")]
    [InlineData("Os collants de ballet.", "The ballet tights.")]
    [InlineData("Os collants de ballet.", null)]
    [InlineData("O agasalho.", "The warm coat.")]
    [InlineData("O macacão.", "The mechanic's overalls.")]
    [InlineData("Os culotes.", null)]
    [InlineData("Os culotes.", "His historical breeches.")]
    [InlineData("Tira a fantasia.", "Not the leotard, the swimsuit.")]
    [InlineData("As polainas.", "The riding gaiters.")]
    [InlineData("As chuteiras e as caneleiras.", "The football boots and shin guards.")]
    public void AmbiguousSportswearAndSharedTermsArePreserved(string source, string? english) =>
        Assert.Equal(source, Converter.Convert(source, null, ReferenceHint.FromText(english)));

    [Theory]
    [InlineData("Um leotard preto.", "Um maillot preto.")]
    [InlineData("Os leotards novos.", "Os maillots novos.")]
    [InlineData("O maiô de ginástica.", "O maillot de ginástica.")]
    [InlineData("Os maiôs de ginástica.", "Os maillots de ginástica.")]
    [InlineData("O maiô de balé.", "O maillot de ballet.")]
    [InlineData("Os maiôs de balé.", "Os maillots de ballet.")]
    [InlineData("A camisa polo branca.", "O polo branco.")]
    [InlineData("As camisas polo brancas.", "Os polos brancos.")]
    [InlineData("A camiseta polo nova.", "O polo novo.")]
    [InlineData("As camisetas polo novas.", "Os polos novos.")]
    [InlineData("A camisa de golfe.", "O polo de golfe.")]
    [InlineData("As camisas de golfe.", "Os polos de golfe.")]
    [InlineData("As calças de hipismo.", "As calças de equitação.")]
    [InlineData("As botas de hipismo.", "As botas de equitação.")]
    [InlineData("O capacete de hipismo.", "O toque de equitação.")]
    [InlineData("Os capacetes de hipismo.", "Os toques de equitação.")]
    [InlineData("A casaca de hipismo.", "A casaca de equitação.")]
    [InlineData("As casacas de hipismo.", "As casacas de equitação.")]
    [InlineData("A camisa de ciclismo.", "A camisola de ciclismo.")]
    [InlineData("As camisas de ciclismo.", "As camisolas de ciclismo.")]
    [InlineData("As bermudas de ciclismo pretas.", "Os calções de ciclismo pretos.")]
    [InlineData("O macacão de mergulho.", "O fato de mergulho.")]
    [InlineData("Os macacões de mergulho.", "Os fatos de mergulho.")]
    [InlineData("O sutiã esportivo.", "O soutien desportivo.")]
    [InlineData("Os sutiãs esportivos.", "Os soutiens desportivos.")]
    public void QualifiedSportswearWorksWithoutEnglish(string source, string expected)
    {
        Converts(source, expected);
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Vocabulary: false)).Convert(source));
    }

    [Theory]
    [InlineData("a calcinha", "as cuecas")]
    [InlineData("a cueca", "as cuecas")]
    [InlineData("na minha cueca", "nas minhas cuecas")]
    [InlineData("uma cueca limpa", "umas cuecas limpas")]
    [InlineData("Arrancou a calcinha.", "Arrancou as cuecas.")]
    [InlineData("A sua calcinha branca.", "As tuas cuecas brancas.")]
    [InlineData("As cuecas estão limpas.", "As cuecas estão limpas.")]
    [InlineData("Cueca boxer.", "Boxers.")]
    [InlineData("A cueca está limpa.", "As cuecas estão limpas.")]
    [InlineData("A cueca estava suja.", "As cuecas estavam sujas.")]
    [InlineData("A cueca é branca.", "As cuecas são brancas.")]
    [InlineData("A cueca ficou molhada.", "As cuecas ficaram molhadas.")]
    public void UnderwearUsesPluralWithAgreement(string source, string expected) => Converts(source, expected);

    [Theory]
    [InlineData("Ele jogou ela no chão.", "Ele mandou-a para o chão.")]
    [InlineData("Ela jogou ele no chão.", "Ela mandou-o para o chão.")]
    [InlineData("Eles jogaram elas no chão.", "Eles mandaram-nas para o chão.")]
    [InlineData("Não jogou ela no chão.", "Não a mandou para o chão.")]
    [InlineData("Tirou a camisa dela.", "Tirou-lhe a camisa.")]
    [InlineData("Arrancou o casaco dele.", "Arrancou-lhe o casaco.")]
    [InlineData("Tirou os sapatos deles.", "Tirou-lhes os sapatos.")]
    [InlineData("Tirou minha camisa.", "Tirou-me a camisa.")]
    [InlineData("Não tirou a camisa dela.", "Não lhe tirou a camisa.")]
    public void PersonAndExplicitClothingOwner(string source, string expected) => Converts(source, expected);

    [Fact]
    public void SportswearRespectsLocalOverridesAndIgnores()
    {
        var local = new PortugueseLocalWordList();
        local.Words.Add(("fantasia", "roupa"));
        local.Words.Add(("camiseta", "t-shirt"));
        local.Ignored.Add("agasalho");
        var converter = new PortugueseConverter(localWordList: local);
        Assert.Equal("Tira a roupa.", converter.Convert("Tira a fantasia.", null, ReferenceHint.FromText("Ditch the leotard.")));
        Assert.Equal("Veste o agasalho.", converter.Convert("Veste o agasalho.", null, ReferenceHint.FromText("Put on the tracksuit.")));
        Assert.Equal("A t-shirt.", converter.Convert("A camiseta.", null, ReferenceHint.FromText("The golf shirt.")));
    }

    [Fact]
    public void EpisodeSentenceUsesReferenceConfirmedOwner()
    {
        var reference = ReferenceHint.FromText("She was on her way home from the library. Next thing she knows, perp's got her on the ground, yanks her jeans, rips her underwear.");
        const string source = "Depois, ele a jogou no chão tirou\nseu jeans e arrancou a calcinha.";
        const string expected = "Depois, ele mandou-a para o chão, tirou-lhe\nos jeans e arrancou-lhe as cuecas.";
        Assert.Equal(expected, Converter.Convert(source, null, reference));
        Assert.Equal(expected, Converter.Convert(expected, null, reference));
    }

    [Theory]
    [InlineData("Tirou o seu casaco.")]
    [InlineData("Ele mandou-a para o chão.")]
    [InlineData("Tirou-lhe a camisa.")]
    [InlineData("Arrancou a porta.")]
    [InlineData("Ele jogou-o no chão.")]
    public void AmbiguousOwnersAndValidOutputsStayUnchanged(string source)
    {
        var formal = new ReferenceHint(false, false, true, true) { FormalAddress = true };
        Assert.Equal(source, Converter.Convert(source, null, formal));
    }

    [Fact]
    public void ThrowsObjectsWithoutChangingTheirMeaning()
    {
        Assert.Equal("Ele jogou a bola no chão.", Converter.Convert("Ele jogou a bola no chão."));
    }

    [Fact]
    public void VocabularyAndPronounGroupsRemainIndependent()
    {
        Assert.Equal("Arrancou a calcinha.", new PortugueseConverter(new ConversionOptions(Vocabulary: false)).Convert("Arrancou a calcinha."));
        Assert.Equal("Tirou as cuecas dela.", new PortugueseConverter(new ConversionOptions(Pronouns: false)).Convert("Tirou a calcinha dela."));
    }

    private static void Converts(string source, string expected)
    {
        Assert.Equal(expected, Converter.Convert(source));
        Assert.Equal(expected, Converter.Convert(expected));
    }
}
