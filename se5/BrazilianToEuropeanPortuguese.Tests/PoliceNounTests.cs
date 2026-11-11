using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class PoliceNounTests
{
    [Theory]
    [InlineData("Sem isso, haveria muito\nmais policiais", "Sem isso, haveria muito\nmais polícias")]
    [InlineData("Precisamos de mais policiais.", "Precisamos de mais polícias.")]
    [InlineData("Há menos policiais.", "Há menos polícias.")]
    [InlineData("Porque há policiais?", "Porque há polícias?")]
    [InlineData("Havia policiais.", "Havia polícias.")]
    [InlineData("Houve policiais.", "Houve polícias.")]
    [InlineData("Haverá policiais.", "Haverá polícias.")]
    [InlineData("Haveria policiais.", "Haveria polícias.")]
    [InlineData("Se houver policiais.", "Se houver polícias.")]
    [InlineData("Há ainda policiais.", "Há ainda polícias.")]
    [InlineData("HÁ POLICIAIS.", "HÁ POLÍCIAS.")]
    [InlineData("<i>Há policiais.</i>", "<i>Há polícias.</i>")]
    [InlineData("Chegaram 20 policiais.", "Chegaram 20 polícias.")]
    [InlineData("MAIS POLICIAIS.", "MAIS POLÍCIAS.")]
    [InlineData("As operações policiais.", "As operações policiais.")]
    [InlineData("Mais operações policiais.", "Mais operações policiais.")]
    [InlineData("Há muitos carros policiais.", "Há muitos carros policiais.")]
    [InlineData("Há operações policiais.", "Há operações policiais.")]
    [InlineData("Havia investigações policiais.", "Havia investigações policiais.")]
    [InlineData("Não para policiais.\nPara policiais, ele é responsável",
        "Não para polícias.\nPara polícias, ele é responsável")]
    [InlineData("pelas bombas apontadas\npara policiais, fim da história.",
        "pelas bombas apontadas\npara polícias, fim da história.")]
    [InlineData("Falei com policiais.", "Falei com polícias.")]
    [InlineData("Saí sem policiais.", "Saí sem polícias.")]
    [InlineData("Estava entre policiais corruptos.", "Estava entre polícias corruptos.")]
    [InlineData("Perante policiais, ficou calado.", "Perante polícias, ficou calado.")]
    [InlineData("Foi feito por policiais.", "Foi feito por polícias.")]
    [InlineData("Entregaram isto a policiais.", "Entregaram isto a polícias.")]
    [InlineData("Confio em policiais honestos.", "Confio em polícias honestos.")]
    [InlineData("Falei com policiais que conheço.", "Falei com polícias que conheço.")]
    [InlineData("Falei com policiais e médicos.", "Falei com polícias e médicos.")]
    [InlineData("Para policiais do bairro.", "Para polícias do bairro.")]
    [InlineData("PARA POLICIAIS.", "PARA POLÍCIAS.")]
    [InlineData("<i>Para policiais.</i>", "<i>Para polícias.</i>")]
    [InlineData("Para operações policiais.", "Para operações policiais.")]
    [InlineData("Com carros policiais.", "Com carros policiais.")]
    [InlineData("Sem provas policiais.", "Sem provas policiais.")]
    [InlineData("Em romances policiais.", "Em romances policiais.")]
    [InlineData("As séries policiais.", "As séries policiais.")]
    [InlineData("Falou com a policiais,", "Falou com a policiais,")]
    [InlineData("Para a policiais.", "Para a policiais.")]
    public void QuantifiersIdentifyPersonnelWithoutChangingAdjectives(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Fact]
    public void ReferenceAndUserOptionsRemainEffective()
    {
        const string source = "Há mais policiais.";
        Assert.Equal("Há mais polícias.", new PortugueseConverter().Convert(source,
            reference: ReferenceHint.FromText("There are more cops.")));
        Assert.Equal("Há mais agentes.", new PortugueseConverter().Convert(source,
            reference: ReferenceHint.FromText("There are more officers.")));
        Assert.Equal("Há polícias.", new PortugueseConverter().Convert("Há policiais.",
            reference: ReferenceHint.FromText("There are cops.")));
        Assert.Equal("Há agentes.", new PortugueseConverter().Convert("Há policiais.",
            reference: ReferenceHint.FromText("There are officers.")));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Vocabulary: false)).Convert(source));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(DisabledRules: "police-nouns")).Convert(source));
        Assert.Equal("Há policiais.", new PortugueseConverter(new ConversionOptions(DisabledRules: "police-nouns"))
            .Convert("Há policiais."));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("mais policiais");
        Assert.Equal(source, new PortugueseConverter(localWordList: words).Convert(source));
        words.Ignored.Add("policiais");
        Assert.Equal("Há policiais.", new PortugueseConverter(localWordList: words).Convert("Há policiais."));
        Assert.Equal("Para policiais.", new PortugueseConverter(localWordList: words).Convert("Para policiais."));
        Assert.Equal("Para policiais.", new PortugueseConverter(new ConversionOptions(DisabledRules: "police-nouns"))
            .Convert("Para policiais."));
        Assert.Equal("Para policiais.", new PortugueseConverter(new ConversionOptions(Vocabulary: false))
            .Convert("Para policiais."));
        Assert.Equal("Para polícias.", new PortugueseConverter().Convert("Para policiais.",
            reference: ReferenceHint.FromText("To cops.")));
        Assert.Equal("Para agentes.", new PortugueseConverter().Convert("Para policiais.",
            reference: ReferenceHint.FromText("To officers.")));
    }
}
