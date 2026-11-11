using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class VehiclePlateTests
{
    [Theory]
    [InlineData("Anotei a placa do carro.", null, "Anotei a matrícula do carro.")]
    [InlineData("Anotei as placas dos carros.", null, "Anotei as matrículas dos carros.")]
    [InlineData("A placa da carrinha.", null, "A matrícula da carrinha.")]
    [InlineData("As placas de um caminhão.", null, "As matrículas de um camião.")]
    [InlineData("A placa da moto.", null, "A matrícula da moto.")]
    [InlineData("O veículo estava sem placa.", null, "O veículo estava sem matrícula.")]
    [InlineData("O carro tinha placas falsas.", null, "O carro tinha matrículas falsas.")]
    [InlineData("Anotei o número da placa.", null, "Anotei o número da matrícula.")]
    [InlineData("Anotei os números das placas.", null, "Anotei os números das matrículas.")]
    [InlineData("com placas de fora.", null, "com matrículas de fora.")]
    [InlineData("Ele usa placas clonadas.", null, "Ele usa matrículas clonadas.")]
    [InlineData("com placas de Jersey.", "with Jersey plates.", "com matrículas de Jersey.")]
    [InlineData("com placas de Lisboa.", "with Lisbon license plates.", "com matrículas de Lisboa.")]
    [InlineData("Duas das placas.", "Two of the plates we could ID, both stolen.", "Duas das matrículas.")]
    [InlineData("- Sobre a placa", "Plates came back registered to him.", "- Sobre a matrícula")]
    [InlineData("Verifique essa placa?", "Call in these plates?", "Verifique esta matrícula?")]
    [InlineData("A placa não foi confirmada.", "Registration not confirmed.", "A matrícula não foi confirmada.")]
    [InlineData("Anotou a placa?", "Did he get the license plate number?", "Anotou a matrícula?")]
    [InlineData("Anotei a placa.", "I wrote down the licence plate.", "Anotei a matrícula.")]
    [InlineData("A placa.", "The number plate.", "A matrícula.")]
    [InlineData("A placa.", "The registration plate.", "A matrícula.")]
    [InlineData("A placa.", "Run the plate.", "A matrícula.")]
    [InlineData("As placas.", "The car has plates.", "As matrículas.")]
    [InlineData("ANOTEI AS PLACAS.", "I wrote down the license plates.", "ANOTEI AS MATRÍCULAS.")]
    [InlineData("<i>com placas de Nova Jersey</i>", "with New Jersey registration", "<i>com matrículas de Nova Jersey</i>")]
    [InlineData("Anotei a placa\ndo carro.", null, "Anotei a matrícula\ndo carro.")]
    public void VehicleRegistrationUsesMatricula(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("A placa de rua.", "The street sign.")]
    [InlineData("A placa na parede.", null)]
    [InlineData("As placas tectónicas.", null)]
    [InlineData("A placa bacteriana.", null)]
    [InlineData("A placa dentária.", null)]
    [InlineData("A placa de rede.", null)]
    [InlineData("A placa de som.", null)]
    [InlineData("A placa gráfica.", null)]
    [InlineData("A placa de indução.", null)]
    [InlineData("A placa de Petri.", null)]
    [InlineData("A placa de metal.", null)]
    [InlineData("O número da placa de rede.", null)]
    [InlineData("O edifício está sem placa.", null)]
    [InlineData("Está sem placa.", null)]
    [InlineData("A placa.", "The dinner plate.")]
    [InlineData("As placas.", null)]
    [InlineData("A placa comemorativa.", "The car has license plates. This is a memorial plaque.")]
    [InlineData("O carro atingiu a placa de sinalização.", null)]
    public void NonVehicleAndUnresolvedUsesRemainUnchanged(string source, string? english) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Fact]
    public void DistinctPlateSensesInOneCueRemainSeparate()
    {
        Assert.Equal("Anotei a matrícula do carro e vi a placa de rua.", new PortugueseConverter()
            .Convert("Anotei a placa do carro e vi a placa de rua.",
                reference: ReferenceHint.FromText("I noted the license plate and saw the street sign.")));
        Assert.Equal("Uma chapa de matrícula.", new PortugueseConverter()
            .Convert("Uma placa de matrícula."));
        Assert.Equal("Uma chapa de matrícula.", new PortugueseConverter()
            .Convert("Uma placa de matrícula.", reference: ReferenceHint.FromText("A license plate.")));
    }

    [Fact]
    public void VocabularyStageAndLocalChoicesWin()
    {
        const string source = "A placa do carro.";
        foreach (var options in new[]
        {
            new ConversionOptions(Vocabulary: false),
            new ConversionOptions(DisabledRules: "vehicle-plates"),
        })
            Assert.Equal(source, new PortugueseConverter(options).Convert(source));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("placa");
        Assert.Equal(source, new PortugueseConverter(localWordList: words).Convert(source));
        var custom = new PortugueseLocalWordList();
        custom.Words.Add(("placa do carro", "chapa do carro"));
        Assert.Equal("A chapa do carro.", new PortugueseConverter(localWordList: custom).Convert(source));
        custom.Words.Clear();
        custom.Words.Add(("placa", "identificação"));
        Assert.Equal("A identificação do carro.", new PortugueseConverter(localWordList: custom).Convert(source));
    }

    [Fact]
    public void VehicleContextCanContinueAcrossCuesWithoutChangingTimings()
    {
        var blocks = new[]
        {
            new SrtBlock { StartMs = 1000, EndMs = 2000, Text = "Era um veículo parecido," },
            new SrtBlock { StartMs = 2010, EndMs = 3000, Text = "em marca e modelo,\ncom uma placa semelhante." },
        };
        var result = SubtitleConversion.Convert(blocks, ConversionOptions.Default);
        Assert.Equal("em marca e modelo,\ncom uma matrícula semelhante.", result[1].Text);
        Assert.Equal(2010, blocks[1].StartMs);
        Assert.Equal(3000, blocks[1].EndMs);
        Assert.Equal("a placa de rua.", new PortugueseConverter().Convert("a placa de rua.",
            previousText: "Era um veículo parecido,"));
    }
}
