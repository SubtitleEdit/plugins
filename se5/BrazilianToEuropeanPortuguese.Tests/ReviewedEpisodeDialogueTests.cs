using System.Text.RegularExpressions;
using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class ReviewedEpisodeDialogueTests
{
    public static TheoryData<string, string?, string> Examples => new()
    {
        { "Não. Trabalhei com ele antes,\nele é pé no saco.", "No. I've worked with him before, he's a pain in the ass.",
            "Não. Trabalhei com ele antes, ele é um chato." },
        { "Ela é pé no saco.", null, "Ela é uma chata." },
        { "ELE É PÉ NO SACO.", null, "ELE É UM CHATO." },
        { "Ele será pé no saco.", null, "Ele será um chato." },
        { "Elas são pés no saco.", null, "Elas são umas chatas." },
        { "Isso é pé no saco.", null, "Isso é uma chatice." },
        { "-Deve ser divertido.\n-Só fique longe de mim.", "This should be fun. Just stay out of my way.",
            "-Deve ser divertido. -Simplesmente fica longe de mim." },
        { "Ela não vai se safar por\nmatar um juiz em exercício.", "She is not getting away with killing a sitting judge.",
            "Ela não se vai safar por matar um juiz em exercício." },
        { "Sem sinal de crime, mas\nsó faz algumas horas.", "No sign of foul play, but it's only been a few hours.",
            "Sem sinal de crime, mas só passaram algumas horas." },
        { "Faz uma hora.", "It's been an hour.", "Passou uma hora." },
        { "Faz um mês.", "It's been a month.", "Passou um mês." },
        { "Faz dois dias.", "It's been 2 days.", "Passaram dois dias." },
        { "Fazia algumas horas.", "It had been a few hours.", "Tinham passado algumas horas." },
        { "Fique falando assim,\nvai cair do lado", "Keep running that mouth, you're gonna go off the side",
            "Continua a falar assim, vais cair do lado" },
        { "Fique fazendo isso.", "Keep doing that.", "Continua a fazer isso." },
        { "Tem um bocão,\ne não se cala!", "You got a big mouth, and you don't shut up!",
            "Tens uma boca grande e não te calas!" },
        { "Sim, mas era um safado\nantes disso.", "Yeah, but he was a prick way before that.",
            "Sim, mas era um idiota antes disso." },
        { "Ela era uma safada.", "She was a jerk.", "Ela era uma idiota." },
        { "São muitas malditas janelas.", "That's a lot of damn windows.", "São muitas janelas." },
        { "São muitos malditos carros.", "That's a lot of damn cars.", "São muitos carros." },
        { "Todas os acordos\nnão valem.", "All deals are off.", "Todos os acordos são nulos." },
        { "Todas os acordos são nulos.", null, "Todos os acordos são nulos." },
        { "Todos as bocas.", null, "Todas as bocas." },
        { "Tens um boca grande.", null, "Tens uma boca grande." },
    };

    [Theory]
    [MemberData(nameof(Examples))]
    public void ReviewedMeaningsAndAgreementConverge(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        var converted = converter.Convert(source, reference: reference);
        Assert.Equal(expected, Flat(converted));
        Assert.Equal(converted, converter.Convert(converted, reference: reference));
        Assert.Equal(source.Count(c => c == '\n'), converted.Count(c => c == '\n'));
    }

    [Theory]
    [InlineData("Ele pôs pé no saco de dormir.", null)]
    [InlineData("Ela está com pé no saco.", null)]
    [InlineData("Ele faz algumas horas.", "He works a few hours.")]
    [InlineData("Faz duas horas que ela chegou.", "It's been 2 hours since she arrived.")]
    [InlineData("São muitas janelas.", "That's a lot of windows.")]
    [InlineData("São demasiadas janelas.", "That's too many windows.")]
    [InlineData("São muitas malditas janelas.", null)]
    [InlineData("Ela era uma safada.", "She was promiscuous.")]
    [InlineData("Ela era uma safada.", null)]
    [InlineData("Ele era um safado.", "He felt a prick from the needle.")]
    [InlineData("Todos os acordos não valem nada.", "All agreements are worthless.")]
    [InlineData("Só fica longe de mim.", "He just stays away from me.")]
    [InlineData("Ele fica falando assim.", "He keeps talking like that.")]
    public void OtherSensesAreNotReinterpreted(string source, string? english)
    {
        var converted = new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english));
        Assert.DoesNotContain("simplesmente", converted, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("passaram", converted, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("são nulos", converted, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("idiota", converted, StringComparison.OrdinalIgnoreCase);
        if (!source.Contains("falando")) Assert.Equal(source, converted);
        else Assert.Equal("Ele fica a falar assim.", converted);
    }

    [Theory]
    [InlineData("Ela não vai esquecer-se.", "Ela não se vai esquecer.")]
    [InlineData("Nunca vamos encontrar-nos.", "Nunca nos vamos encontrar.")]
    [InlineData("Ela vai esquecer-se.", "Ela vai esquecer-se.")]
    public void NegativeIrPlacementIsConsistent(string source, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source));

    [Theory]
    [MemberData(nameof(Examples))]
    public void OrdinaryWrapsDoNotBlockTheCorrection(string source, string? english, string expected)
    {
        if (source.Contains('-')) return;
        var flat = Flat(source);
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        for (var index = 0; index < flat.Length; index++)
        {
            if (flat[index] != ' ') continue;
            foreach (var newline in new[] { "\n", "\r\n" })
            {
                var result = converter.Convert(flat[..index] + newline + flat[(index + 1)..], reference: reference);
                Assert.Equal(expected, Flat(result));
                Assert.Equal(1, result.Count(c => c == '\n'));
            }
        }
    }

    [Fact]
    public void FormalAddressAndOptionsRemainAuthoritative()
    {
        var formal = new PortugueseConverter();
        ReferenceHint Formal(string english) => ReferenceHint.FromText(english)! with { FormalAddress = true };
        Assert.Equal("Tem uma boca grande e não se cala!", formal.Convert("Tem um bocão, e não se cala!",
            reference: Formal("You got a big mouth, and you don't shut up!")));
        Assert.Equal("Simplesmente fique longe de mim.", formal.Convert("Só fique longe de mim.",
            reference: Formal("Just stay away from me.")));
        Assert.Equal("Continue a fazer isso.", formal.Convert("Fique fazendo isso.",
            reference: Formal("Keep doing that.")));
        const string quantity = "São muitas malditas janelas.";
        var reference = ReferenceHint.FromText("That's a lot of damn windows.");
        foreach (var options in new[] { new ConversionOptions(Colloquial: false),
            new ConversionOptions(Vocabulary: false),
            new ConversionOptions(DisabledRules: "dialogue-idioms") })
            Assert.Equal(quantity, new PortugueseConverter(options).Convert(quantity, reference: reference));
        var ignored = new PortugueseLocalWordList();
        ignored.Ignored.Add("malditas");
        Assert.Equal(quantity, new PortugueseConverter(localWordList: ignored).Convert(quantity, reference: reference));
        var custom = new PortugueseLocalWordList();
        custom.Words.Add(("safado", "patife"));
        Assert.Equal("Era um patife.", new PortugueseConverter(localWordList: custom).Convert("Era um safado.",
            reference: ReferenceHint.FromText("He was a prick.")));
    }

    [Fact]
    public void SharedPipelineRespectsFormalAddressAndMissingReference()
    {
        var cue = new SrtBlock { StartMs = 1000, EndMs = 3000, Text = "Tem um bocão,\ne não se cala!" };
        var english = new string?[] { "You got a big mouth, and you don't shut up!" };
        var formal = Assert.Single(SubtitleConversion.Convert(new[] { cue },
            new ConversionOptions(Address: AddressMode.Formal), english: english));
        Assert.Equal("Tem uma boca grande\ne não se cala!", formal.Text);
        Assert.Equal(1000, cue.StartMs);
        Assert.Equal(3000, cue.EndMs);
        cue.Text = "São muitas malditas janelas.";
        var withoutReference = Assert.Single(SubtitleConversion.Convert(new[] { cue },
            new ConversionOptions(UseReference: false)));
        Assert.Equal(cue.Text, withoutReference.Text);
    }

    [Fact]
    public void IgnoredDialogueAndDisabledAddressKeepTheirMeaning()
    {
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("bocão");
        words.Ignored.Add("safado");
        var converter = new PortugueseConverter(localWordList: words);
        Assert.Equal("Tens um bocão, e não se cala!", converter.Convert("Tem um bocão, e não se cala!",
            reference: ReferenceHint.FromText("You got a big mouth, and you don't shut up!")));
        Assert.Equal("Era um safado.", converter.Convert("Era um safado.",
            reference: ReferenceHint.FromText("He was a prick.")));
        foreach (var options in new[] { new ConversionOptions(YouToTu: false),
            new ConversionOptions(DisabledRules: "address") })
        {
            var result = new PortugueseConverter(options).Convert("Tem um bocão, e não se cala!",
                reference: ReferenceHint.FromText("You got a big mouth, and you don't shut up!"));
            Assert.Equal("Tem um bocão, e não se cala!", result);
        }
        Assert.Equal("<i>Tens uma boca grande e não te calas!</i>", new PortugueseConverter().Convert(
            "<i>Tem um bocão, e não se cala!</i>",
            reference: ReferenceHint.FromText("You got a big mouth, and you don't shut up!")));
        Assert.Equal("-Continua a falar assim.\n-Ele vai cair.", new PortugueseConverter().Convert(
            "-Fique falando assim.\n-Ele vai cair.",
            reference: ReferenceHint.FromText("Keep talking like that. He's going to fall.")));
    }

    private static string Flat(string text) => Regex.Replace(text, @"\s+", " ");
}
