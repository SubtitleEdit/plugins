using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class StudyGuideTests
{
    private static readonly PortugueseConverter Converter = new();

    public static TheoryData<string, string> Vocabulary => new()
    {
        { "O varal de roupa.", "O estendal de roupa." },
        { "Os varais de roupa.", "Os estendais de roupa." },
        { "Um pregador de roupa.", "Uma mola da roupa." },
        { "Os pregadores de roupa.", "As molas da roupa." },
        { "A marcha à ré.", "A marcha-atrás." },
        { "Uma internação.", "Um internamento." },
        { "As internações.", "Os internamentos." },
        { "Um exame de sangue.", "Uma análise ao sangue." },
        { "Os exames de sangue.", "As análises ao sangue." },
        { "A coleta de sangue.", "A colheita de sangue." },
        { "As coletas de sangue.", "As colheitas de sangue." },
        { "O efeito colateral.", "O efeito secundário." },
        { "Os efeitos colaterais.", "Os efeitos secundários." },
        { "O clareamento dental.", "O branqueamento dentário." },
        { "O shampoo.", "O champô." },
        { "Os shampoos.", "Os champôs." },
        { "O desodorante.", "O desodorizante." },
        { "Os desodorantes.", "Os desodorizantes." },
        { "O demaquilante.", "O desmaquilhante." },
        { "Os demaquilantes.", "Os desmaquilhantes." },
        { "Os cílios postiços.", "As pestanas postiças." },
        { "A máscara de cílios.", "A máscara de pestanas." },
        { "Uma pochete.", "Uma bolsa de cintura." },
        { "As pochetes.", "As bolsas de cintura." },
        { "O cortador de grama.", "O corta-relva." },
        { "Os cortadores de grama.", "Os corta-relvas." },
        { "Um carrapato.", "Uma carraça." },
        { "Os carrapatos.", "As carraças." },
        { "O pernilongo.", "O mosquito." },
        { "Os pernilongos.", "Os mosquitos." },
        { "A salsinha.", "A salsa." },
        { "O papel-toalha.", "O papel de cozinha." },
        { "O papel alumínio.", "O papel de alumínio." },
        { "Um filme plástico.", "Uma película aderente." },
        { "Um mixer de mão.", "Uma varinha mágica." },
        { "A secadora de roupa.", "A máquina de secar roupa." },
        { "As secadoras de roupa.", "As máquinas de secar roupa." },
        { "Um pendrive.", "Uma pen USB." },
        { "Os pendrives.", "As pens USB." },
        { "O roteador.", "O router." },
        { "Os roteadores.", "Os routers." },
        { "O arquivo SRT.", "O ficheiro SRT." },
        { "Os arquivos SRT.", "Os ficheiros SRT." },
        { "O roteirista.", "O argumentista." },
        { "As roteiristas.", "As argumentistas." },
        { "A pré-estreia.", "A ante-estreia." },
        { "As pré-estreias.", "As ante-estreias." },
        { "O doutorado.", "O doutoramento." },
        { "Os doutorados.", "Os doutoramentos." },
        { "A volta às aulas.", "O regresso às aulas." },
        { "Uma kitnet.", "Um estúdio." },
        { "As kitnets.", "Os estúdios." },
        { "Com frete grátis.", "Com portes grátis." },
        { "O contracheque.", "O recibo de vencimento." },
        { "Os contracheques.", "Os recibos de vencimento." },
        { "O interfone.", "O intercomunicador." },
        { "Os interfones.", "Os intercomunicadores." },
        { "Um plugue.", "Uma ficha." },
        { "Os plugues.", "As fichas." },
        { "A esmerilhadeira.", "A rebarbadora." },
        { "As esmerilhadeiras.", "As rebarbadoras." },
        { "A lixadeira.", "A lixadora." },
        { "As lixadeiras.", "As lixadoras." },
        { "A parafusadeira.", "A aparafusadora." },
        { "As parafusadeiras.", "As aparafusadoras." },
        { "A chave catraca.", "A chave de roquete." },
        { "As chaves catraca.", "As chaves de roquete." },
        { "A massa corrida.", "A massa de barramento." },
        { "A calha de chuva.", "A caleira." },
        { "As calhas de chuva.", "As caleiras." },
        { "O zoneamento.", "O zonamento." },
        { "Os zoneamentos.", "Os zonamentos." },
        { "O laudo técnico.", "O relatório técnico." },
        { "Os laudos técnicos.", "Os relatórios técnicos." },
        { "Um memorial descritivo.", "Uma memória descritiva." },
        { "Os memoriais descritivos.", "As memórias descritivas." },
        { "A chupeta.", "A chucha." },
        { "As chupetas.", "As chuchas." },
        { "O babador.", "O babete." },
        { "Os babadores.", "Os babetes." },
        { "O lenço umedecido.", "O toalhete húmido." },
        { "Os lenços umedecidos.", "Os toalhetes húmidos." },
        { "O prontuário médico.", "O processo clínico." },
        { "Os prontuários médicos.", "Os processos clínicos." },
        { "A consulta de retorno.", "A consulta de seguimento." },
        { "As consultas de retorno.", "As consultas de seguimento." },
        { "A superdosagem.", "A sobredosagem." },
        { "As superdosagens.", "As sobredosagens." },
        { "Os triglicerídeos.", "Os triglicéridos." },
        { "A superdotação.", "A sobredotação." },
        { "A avaliação somativa.", "A avaliação sumativa." },
        { "As avaliações somativas.", "As avaliações sumativas." },
        { "A evasão escolar.", "O abandono escolar." },
        { "O centroavante.", "O ponta de lança." },
        { "Os centroavantes.", "Os pontas de lança." },
        { "As oitavas de final.", "Os oitavos de final." },
        { "As quartas de final.", "Os quartos de final." },
        { "A semifinal.", "A meia-final." },
        { "As semifinais.", "As meias-finais." },
        { "O jogo de ida.", "O jogo da primeira mão." },
        { "Os jogos de ida.", "Os jogos da primeira mão." },
        { "O jogo de volta.", "O jogo da segunda mão." },
        { "Os jogos de volta.", "Os jogos da segunda mão." },
        { "O banco de reservas.", "O banco de suplentes." },
        { "Os bancos de reservas.", "Os bancos de suplentes." },
        { "A pré-temporada.", "A pré-época." },
        { "As pré-temporadas.", "As pré-épocas." },
        { "O handebol.", "O andebol." },
        { "A bicicleta ergométrica.", "A bicicleta estática." },
        { "As bicicletas ergométricas.", "As bicicletas estáticas." },
        { "O carboidrato.", "O hidrato de carbono." },
        { "Os carboidratos.", "Os hidratos de carbono." },
        { "O alho-poró.", "O alho-francês." },
        { "Os alhos-porós.", "Os alhos-franceses." },
        { "Os brócolis.", "Os brócolos." },
        { "O açúcar mascavo.", "O açúcar mascavado." },
        { "O fermento biológico.", "O fermento de padeiro." },
        { "O fermento químico.", "O fermento em pó." },
        { "O cupom de desconto.", "O cupão de desconto." },
        { "Os cupons de desconto.", "Os cupões de desconto." },
        { "O marca-texto.", "O marcador fluorescente." },
        { "Os marca-textos.", "Os marcadores fluorescentes." },
        { "A canetinha.", "A caneta de feltro." },
        { "As canetinhas.", "As canetas de feltro." },
        { "O giz de cera.", "O lápis de cera." },
        { "<i>Um exame\nde sangue.</i>", "<i>Uma análise\nao sangue.</i>" },
        { "OS EFEITOS COLATERAIS.", "OS EFEITOS SECUNDÁRIOS." },
    };

    [Theory]
    [MemberData(nameof(Vocabulary))]
    public void ReviewedVocabulary(string source, string expected)
    {
        Assert.Equal(expected, Converter.Convert(source));
        Assert.Equal(expected, Converter.Convert(expected));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Vocabulary: false)).Convert(source));
    }

    [Theory]
    [InlineData("Eu tiro uma dúvida.", "Eu esclareço uma dúvida.")]
    [InlineData("Ela tirou as dúvidas.", "Ela esclareceu as dúvidas.")]
    [InlineData("Tu tiras dúvidas.", "Tu esclareces dúvidas.")]
    [InlineData("Vamos tirar uma dúvida.", "Vamos esclarecer uma dúvida.")]
    [InlineData("Não tirei a dúvida.", "Não esclareci a dúvida.")]
    [InlineData("Tirar-lhe uma dúvida.", "Esclarecer-lhe uma dúvida.")]
    [InlineData("TIREI UMA DÚVIDA.", "ESCLARECI UMA DÚVIDA.")]
    [InlineData("Ela tirou\numa dúvida.", "Ela esclareceu\numa dúvida.")]
    [InlineData("Se pá, ele chega.", "Se calhar, ele chega.")]
    [InlineData("SE PÁ, ELE CHEGA.", "SE CALHAR, ELE CHEGA.")]
    [InlineData("Se\npá, ele chega.", "Se\ncalhar, ele chega.")]
    [InlineData("Vamos nessa!", "Vamos a isso!")]
    [InlineData("- Vamos nessa.", "- Vamos a isso.")]
    public void ReviewedExpressions(string source, string expected)
    {
        Assert.Equal(expected, Converter.Convert(source));
        Assert.Equal(expected, Converter.Convert(expected));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Colloquial: false)).Convert(source));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Vocabulary: false)).Convert(source));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(DisabledRules: "reference-idioms")).Convert(source));
    }

    [Theory]
    [InlineData("O facto.")]
    [InlineData("O género.")]
    [InlineData("O bebé.")]
    [InlineData("A câmara.")]
    [InlineData("A humidade.")]
    [InlineData("O contacto.")]
    [InlineData("A secção.")]
    [InlineData("O concelho.")]
    [InlineData("Que fixe!")]
    [InlineData("É porreiro.")]
    [InlineData("O gajo.")]
    [InlineData("Bué de gente.")]
    [InlineData("A rapariga.")]
    [InlineData("Os cílios da célula.")]
    [InlineData("A lagartixa.")]
    [InlineData("A calha de cortina.")]
    [InlineData("Ele é doutorado.")]
    [InlineData("O copo americano.")]
    [InlineData("O SUS e o SNS.")]
    [InlineData("O SAMU e o INEM.")]
    [InlineData("Os Correios e os CTT.")]
    [InlineData("A folha sulfite.")]
    [InlineData("A graduação.")]
    [InlineData("A união estável.")]
    [InlineData("O carro avariado.")]
    [InlineData("O telemóvel partido.")]
    [InlineData("Vamos nessa carrinha.")]
    [InlineData("Ela tirou uma fotografia.")]
    [InlineData("Se a pá cair, espera.")]
    public void RejectsReversedEquivalencesSharedWordsAndDifferentReferents(string text) =>
        Assert.Equal(text, Converter.Convert(text));

    [Fact]
    public void BatchUsesSameMappingsAndKeepsCueTimes()
    {
        var source = "1\n00:00:01,000 --> 00:00:03,000\nUm exame de sangue.\n\n" +
                     "2\n00:00:04,000 --> 00:00:06,000\nEla tirou uma dúvida.\n\n" +
                     "3\n00:00:07,000 --> 00:00:09,000\nO concelho.\n";
        var request = new PluginRequest
        {
            SettingsVersion = ConversionOptions.Version,
            Settings = new ConversionOptions(UseReference: false).ToJson(),
            Subtitle = new PluginSubtitle { Format = "SubRip", SubRip = source },
        };
        var response = BatchMode.BuildBatchResponse(request,
            Path.Combine(Path.GetTempPath(), $"ptpt-study-guide-{Guid.NewGuid():N}", PortugueseLocalWordList.FileName),
            new EmbeddingSettings(CheckMeaning: false));
        var before = SubRipParser.Parse(source);
        var after = SubRipParser.Parse(response.Subtitle!.Native);
        Assert.Equal(new[] { "Uma análise ao sangue.", "Ela esclareceu uma dúvida.", "O concelho." },
            after.Select(cue => cue.Text));
        Assert.Equal(before.Select(cue => (cue.StartMs, cue.EndMs)), after.Select(cue => (cue.StartMs, cue.EndMs)));
    }
}
