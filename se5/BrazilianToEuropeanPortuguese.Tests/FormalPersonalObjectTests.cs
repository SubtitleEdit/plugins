using System.Text.RegularExpressions;
using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class FormalPersonalObjectTests
{
    [Theory]
    [InlineData("Vejo você no tribunal.", "Vejo-o no tribunal.", "Vejo-te no tribunal.")]
    [InlineData("Vejo você amanhã.", "Vejo-o amanhã.", "Vejo-te amanhã.")]
    [InlineData("Não vejo você no tribunal.", "Não o vejo no tribunal.", "Não te vejo no tribunal.")]
    [InlineData("Nunca vi você aqui.", "Nunca o vi aqui.", "Nunca te vi aqui.")]
    [InlineData("Sei que ele viu você.", "Sei que ele o viu.", "Sei que ele te viu.")]
    [InlineData("Eu vi você.", "Eu vi-o.", "Eu vi-te.")]
    [InlineData("Ela encontrou você.", "Ela encontrou-o.", "Ela encontrou-te.")]
    [InlineData("Eles encontraram você.", "Eles encontraram-no.", "Eles encontraram-te.")]
    [InlineData("Ele vê você.", "Ele vê-o.", "Ele vê-te.")]
    [InlineData("Ele fez você rir.", "Ele fê-lo rir.", "Ele fez-te rir.")]
    [InlineData("Quero ver você.", "Quero vê-lo.", "Quero ver-te.")]
    [InlineData("Manterei você informado.", "Mantê-lo-ei informado.", "Manter-te-ei informado.")]
    [InlineData("Não manterei você informado.", "Não o manterei informado.", "Não te manterei informado.")]
    [InlineData("Ele considerou você inocente.", "Ele considerou-o inocente.", "Ele considerou-te inocente.")]
    [InlineData("Acabei defendendo você.", "Acabei por defendê-lo.", "Acabei por defender-te.")]
    [InlineData("VEJO VOCÊ NO TRIBUNAL.", "VEJO-O NO TRIBUNAL.", "VEJO-TE NO TRIBUNAL.")]
    [InlineData("<i>Vejo você no tribunal.</i>", "<i>Vejo-o no tribunal.</i>", "<i>Vejo-te no tribunal.</i>")]
    public void TheSameObjectRuleSupportsFormalAndInformalAddress(string source, string formal, string informal)
    {
        foreach (var (mode, expected) in new[] { (AddressMode.Formal, formal), (AddressMode.Informal, informal) })
        {
            var options = new ConversionOptions(Address: mode);
            var block = new SrtBlock { StartMs = 1000, EndMs = 2000, Text = source };
            var result = SubtitleConversion.Convert([block], options)[0].Text;
            Assert.Equal(expected, result);
            Assert.Equal(result, SubtitleConversion.Convert(
                [new SrtBlock { StartMs = 1000, EndMs = 2000, Text = result }], options)[0].Text);
        }
    }

    [Theory]
    [InlineData("Doutora, vejo você no tribunal.", "Doutora, vejo-a no tribunal.")]
    [InlineData("Senhora, vi você aqui.", "Senhora, vi-a aqui.")]
    [InlineData("Vejo você no tribunal, doutora.", "Vejo-a no tribunal, doutora.")]
    [InlineData("Vejo você no tribunal, senhora Silva.", "Vejo-a no tribunal, senhora Silva.")]
    [InlineData("Doutor, vejo você no tribunal.", "Doutor, vejo-o no tribunal.")]
    [InlineData("A doutora viu você.", "A doutora viu-o.")]
    [InlineData("Falei com a senhora. Vejo você amanhã.", "Falei com a senhora. Vejo-o amanhã.")]
    [InlineData("-Doutora, ouviu?\n-Vejo você no tribunal.", "-Doutora, ouviu?\n-Vejo-o no tribunal.")]
    public void OnlyExplicitLocalFeminineAddressSelectsA(string source, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source,
            reference: new ReferenceHint(false, false, false, false) { FormalAddress = true }));

    [Theory]
    [InlineData("É você.")]
    [InlineData("Parece você.")]
    [InlineData("Vejo você mesma no tribunal.")]
    [InlineData("A honra você.")]
    [InlineData("O contacto você.")]
    [InlineData("Esperei você pedir ajuda.")]
    public void PredicatesEmphasisNominalHomographsAndInfinitiveSubjectsAreProtected(string source) =>
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Colloquial: false)).Convert(source,
            reference: new ReferenceHint(false, false, false, false) { FormalAddress = true }));

    [Theory]
    [InlineData("Agora, por que ajudaria você?", "Agora, porque o ajudaria?")]
    [InlineData("Então não foi a sandes que realmente trouxe você aqui.", "Então não foi a sandes que realmente o trouxe aqui.")]
    [InlineData("Também sei que a presidente da câmara confia e respeita você,", "Também sei que a presidente da câmara confia e o respeita,")]
    [InlineData("Ele confia e respeita você.", "Ele confia e respeita-o.")]
    [InlineData("- Espera lá!\n- Não, espera você!", "- Espera lá!\n- Não, espera você!")]
    [InlineData("Manda você.", "Manda você.")]
    [InlineData("Acabei defendendo-o.", "Acabei por defendê-lo.")]
    [InlineData("Acabei defendendo-te.", "Acabei por defender-te.")]
    public void CorpusInteractionsPreserveAttractionEmphaticSubjectsAndProgressiveConstructions(string source, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source,
            reference: new ReferenceHint(false, false, false, false) { FormalAddress = true }));

    [Theory]
    [InlineData("Vejo você no tribunal.", "Vejo-o no tribunal.")]
    [InlineData("Não vejo você no tribunal.", "Não o vejo no tribunal.")]
    [InlineData("Doutora, vejo você no tribunal.", "Doutora, vejo-a no tribunal.")]
    [InlineData("Vejo você no tribunal, doutora.", "Vejo-a no tribunal, doutora.")]
    [InlineData("Então não foi a sandes que realmente trouxe você aqui.", "Então não foi a sandes que realmente o trouxe aqui.")]
    [InlineData("Sei que a presidente da câmara confia e respeita você.", "Sei que a presidente da câmara confia e o respeita.")]
    public void EveryWordBoundaryPreservesFormalPlacement(string source, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = new ReferenceHint(false, false, false, false) { FormalAddress = true };
        for (var index = 0; index < source.Length; index++)
        {
            if (source[index] != ' ') continue;
            foreach (var newline in new[] { "\n", "\r\n" })
            {
                var result = converter.Convert(source[..index] + newline + source[(index + 1)..], reference: reference);
                Assert.Equal(expected, Regex.Replace(result, @"\s+", " "));
                Assert.Equal(1, result.Count(c => c == '\n'));
                Assert.Equal(result, converter.Convert(result, reference: reference));
            }
        }
    }

    [Fact]
    public void FormalConversionRespectsSettingsIgnoredTextAndLocalWords()
    {
        const string source = "Vejo você no tribunal.";
        var reference = new ReferenceHint(false, false, false, false) { FormalAddress = true };
        foreach (var options in new[] { new ConversionOptions(Pronouns: false),
            new ConversionOptions(YouToTu: false), new ConversionOptions(DisabledRules: "address") })
            Assert.Equal(source, new PortugueseConverter(options).Convert(source, reference: reference));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("você");
        Assert.Equal(source, new PortugueseConverter(localWordList: words).Convert(source, reference: reference));
        words.Ignored.Clear();
        words.Words.Add(("vejo", "encontro"));
        Assert.Equal("Encontro-o no tribunal.", new PortugueseConverter(localWordList: words)
            .Convert(source, reference: reference));
        Assert.Equal("Acabei defendendo-o.", new PortugueseConverter(new ConversionOptions(Progressive: false))
            .Convert("Acabei defendendo você.", reference: reference));
        Assert.Equal("Acabei defendendo-o.", new PortugueseConverter(new ConversionOptions(DisabledRules: "progressive,progressive-command"))
            .Convert("Acabei defendendo você.", reference: reference));
    }

    [Fact]
    public void NearbyFormalAddressExplainsTheSharedScreenshotFailure()
    {
        var blocks = new[] {
            new SrtBlock { StartMs = 269000, EndMs = 271500, Text = "A reunião acabou, doutora." },
            new SrtBlock { StartMs = 271664, EndMs = 273426, Text = "Vejo você no tribunal." },
        };
        var english = new string?[] { "The meeting is over, ma'am.", "I'll see you in court." };
        var automatic = SubtitleConversion.Convert(blocks, ConversionOptions.Default, english: english);
        Assert.True(automatic[1].Hint?.FormalAddress);
        Assert.Equal("Vejo-o no tribunal.", automatic[1].Text);
        var informal = SubtitleConversion.Convert(blocks, new ConversionOptions(Address: AddressMode.Informal), english: english);
        Assert.Equal("Vejo-te no tribunal.", informal[1].Text);
        Assert.Equal("Vejo você no tribunal.", blocks[1].Text);
        Assert.Equal(271664, blocks[1].StartMs);
        Assert.Equal(273426, blocks[1].EndMs);
    }
}
