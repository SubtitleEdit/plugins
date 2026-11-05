using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;
using System.Text.Json;

namespace BrazilianToEuropeanPortuguese.Tests;

/// <summary>Subtitle Edit Batch convert: "batch" converts every line headless, "configure" returns the settings.</summary>
public sealed class BatchModeTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "BrazilianToEuropeanPortugueseTests-" + Guid.NewGuid().ToString("N"));

    public BatchModeTests() => Directory.CreateDirectory(_folder);

    public void Dispose() => Directory.Delete(_folder, true);

    private string WordListPath => Path.Combine(_folder, PortugueseLocalWordList.FileName);

    private static PluginRequest Request(params string[] texts)
    {
        var blocks = texts.Select((t, i) => new SrtBlock { StartMs = i * 1000, EndMs = i * 1000 + 900, Text = t }).ToList();
        return new PluginRequest
        {
            RequestType = PluginRequestType.Batch,
            Subtitle = new PluginSubtitle { Format = "SubRip", SubRip = SubRipParser.Serialize(blocks) },
        };
    }

    private static List<string> Texts(PluginResponse response) =>
        SubRipParser.Parse(response.Subtitle!.Native).Select(b => b.Text).ToList();

    [Fact]
    public void Batch_EnglishSportswearPreservesCueTimingAndReferenceSwitch()
    {
        var request = EpisodeRequest("Tira a fantasia.", true, "Ditch the leotard.");
        var response = BatchMode.BuildBatchResponse(request, WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Equal(new[] { "Tira o maillot." }, Texts(response));
        Assert.Equal(SubRipParser.Parse(request.Subtitle.SubRip).Select(b => (b.StartMs, b.EndMs)),
            SubRipParser.Parse(response.Subtitle!.Native).Select(b => (b.StartMs, b.EndMs)));
        request.SettingsVersion = ConversionOptions.Version;
        request.Settings = new ConversionOptions(UseReference: false).ToJson();
        response = BatchMode.BuildBatchResponse(request, WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Null(response.Subtitle);
        Assert.Contains("nothing to convert", response.Message);
    }

    [Fact]
    public void Batch_SharedResidenceUsesEnglishWithoutModels()
    {
        var request = EpisodeRequest("Você é meu vizinho de andar.", true, "Hey, we live on the same floor.");
        var response = BatchMode.BuildBatchResponse(request, WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Equal(new[] { "Vivemos no mesmo andar." }, Texts(response));
        Assert.Equal(SubRipParser.Parse(request.Subtitle.SubRip).Select(b => (b.StartMs, b.EndMs)),
            SubRipParser.Parse(response.Subtitle!.Native).Select(b => (b.StartMs, b.EndMs)));
        request.SettingsVersion = ConversionOptions.Version;
        request.Settings = new ConversionOptions(UseReference: false).ToJson();
        response = BatchMode.BuildBatchResponse(request, WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Equal(new[] { "És meu vizinho de andar." }, Texts(response));
    }

    [Fact]
    public void Batch_PhysicalDescriptionsPreserveMeasurementsAndCueTimings()
    {
        var request = Request("Suspeito de cerca de 1,80m, porte médio.", "Uma mulher de 60 kg.",
            "Um muro de cerca de dois metros.", "Um suspeito de cerca de 30 anos.");
        var response = BatchMode.BuildBatchResponse(request, WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Equal(new[] { "Suspeito com cerca de 1,80m, porte médio.", "Uma mulher com 60 kg.",
            "Um muro de cerca de dois metros.", "Um suspeito de cerca de 30 anos." }, Texts(response));
        Assert.Equal(SubRipParser.Parse(request.Subtitle.SubRip).Select(b => (b.StartMs, b.EndMs)),
            SubRipParser.Parse(response.Subtitle!.Native).Select(b => (b.StartMs, b.EndMs)));
    }

    [Fact]
    public void Batch_VocabularyVerbWorkbookPreservesCuesAndQualifiedSenses()
    {
        var request = Request("Uma furadeira nova.", "As sacolas vazias.", "Ele rosqueou a tampa.",
            "Ela subiu um arquivo.", "Ele engraxou os sapatos.", "Eu digitei o código.");
        var response = BatchMode.BuildBatchResponse(request, WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Equal(new[] { "Um berbequim novo.", "Os sacos vazios.", "Ele enroscou a tampa.",
            "Ela carregou um ficheiro.", "Ele engraxou os sapatos.", "Eu introduzi o código." }, Texts(response));
        Assert.Equal(SubRipParser.Parse(request.Subtitle.SubRip).Select(b => (b.StartMs, b.EndMs)),
            SubRipParser.Parse(response.Subtitle!.Native).Select(b => (b.StartMs, b.EndMs)));
    }

    [Fact]
    public void Batch_ExtractedQuestionsPreserveSubjectAndDistinguishObjects()
    {
        var request = EpisodeRequest("Ned, quem você acha que vê isso?", true,
            "Ned, who do you think is actually watching this?");
        var response = BatchMode.BuildBatchResponse(request, WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Equal(new[] { "Ned, quem achas que vê isto?" }, Texts(response));
        Assert.Equal(SubRipParser.Parse(request.Subtitle.SubRip).Select(b => (b.StartMs, b.EndMs)),
            SubRipParser.Parse(response.Subtitle!.Native).Select(b => (b.StartMs, b.EndMs)));
        request = EpisodeRequest("Quem você acha que viu?", true, "Who do you think you saw?");
        response = BatchMode.BuildBatchResponse(request, WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Equal(new[] { "Quem achas que viste?" }, Texts(response));
    }

    [Fact]
    public void Batch_HabitualWordOrderAndProgressiveSubjectKeepCueTimings()
    {
        var request = EpisodeRequest("porque você sempre sabe\nquando eu tô mentindo.", true,
            "Because you always know when I'm lying.");
        var response = BatchMode.BuildBatchResponse(request, WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Equal(new[] { "porque sabes sempre\nquando estou a mentir." }, Texts(response));
        Assert.Equal(SubRipParser.Parse(request.Subtitle.SubRip).Select(b => (b.StartMs, b.EndMs)),
            SubRipParser.Parse(response.Subtitle!.Native).Select(b => (b.StartMs, b.EndMs)));
    }

    [Fact]
    public void Batch_EnglishSlangResolvesPortugueseOnlyWithReference()
    {
        var request = EpisodeRequest("Estou quebrada.", true, "I'm skint.");
        var response = BatchMode.BuildBatchResponse(request, WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Equal(new[] { "Estou sem dinheiro." }, Texts(response));
        Assert.Equal(SubRipParser.Parse(request.Subtitle.SubRip).Select(b => (b.StartMs, b.EndMs)),
            SubRipParser.Parse(response.Subtitle!.Native).Select(b => (b.StartMs, b.EndMs)));
        request.Settings = (ConversionOptions.Default with { UseReference = false }).ToJson();
        request.SettingsVersion = ConversionOptions.Version;
        response = BatchMode.BuildBatchResponse(request, WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Null(response.Subtitle);
        Assert.Contains("nothing to convert.", response.Message);
    }

    [Fact]
    public void Batch_NewEnglishIdiomsProduceReviewHintsWithoutModels()
    {
        var request = EpisodeRequest("Ele estava no seu Tod.", true, "He was on his tod.");
        var response = BatchMode.BuildBatchResponse(request, WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Contains("1 line(s) with an English idiom", response.Message);
        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Batch_AdditionalWorkbookRulesPreserveCueTimings()
    {
        var request = Request("Recebi o holerite.", "Baixei um arquivo.", "Dei uma força.",
            "Ela puxou\nassunto.", "Elas ficaram de saco cheio.", "Foi um escanteio.", "Contou uma parada cardíaca.");
        var response = BatchMode.BuildBatchResponse(request, WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Equal(new[] { "Recebi o recibo de vencimento.", "Descarreguei um ficheiro.", "Dei uma ajuda.",
            "Ela meteu\nconversa.", "Elas ficaram fartas.", "Foi um canto.", "Contou uma paragem cardíaca." }, Texts(response));
        Assert.Equal(SubRipParser.Parse(request.Subtitle.SubRip).Select(b => (b.StartMs, b.EndMs)),
            SubRipParser.Parse(response.Subtitle!.Native).Select(b => (b.StartMs, b.EndMs)));
    }

    [Fact]
    public void Batch_AdditionalWorkbookHelpOutNeedsEnglishConfirmation()
    {
        var request = EpisodeRequest("Ele quebrou um galho.", true, "He did me a favor.");
        var response = BatchMode.BuildBatchResponse(request, WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Equal(new[] { "Ele desenrascou." }, Texts(response));
        request.Settings = (ConversionOptions.Default with { UseReference = false }).ToJson();
        request.SettingsVersion = ConversionOptions.Version;
        response = BatchMode.BuildBatchResponse(request, WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Equal(new[] { "Ele partiu um galho." }, Texts(response));
    }

    [Fact]
    public void Batch_VerifiedSpreadsheetExpressionsPreserveCueTimings()
    {
        var request = Request("Vou pegar um busão.", "Paguei mico.", "Dei um rolê.", "Tomei uma breja.",
            "A meia-calça está rasgada.", "A mensalidade da faculdade.");
        var response = BatchMode.BuildBatchResponse(request, WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Equal(new[] { "Vou apanhar um autocarro.", "Passei vergonha.", "Dei uma volta.", "Bebi uma cerveja.",
            "Os collants estão rasgados.", "A propina." }, Texts(response));
        Assert.Equal(SubRipParser.Parse(request.Subtitle.SubRip).Select(b => (b.StartMs, b.EndMs)),
            SubRipParser.Parse(response.Subtitle!.Native).Select(b => (b.StartMs, b.EndMs)));
    }

    [Fact]
    public void Batch_VerifiedSpreadsheetSlangUsesEnglishConfirmation()
    {
        var response = BatchMode.BuildBatchResponse(EpisodeRequest("Ele é pão-duro.", true, "He's a cheapskate."),
            WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Equal(new[] { "Ele é forreta." }, Texts(response));
    }

    [Fact]
    public void Batch_InclusiveContinuationUsesOriginalPreviousCue()
    {
        var response = BatchMode.BuildBatchResponse(Request("A gente sempre escuta,", "mas nunca acha que vai acontecer contigo."),
            WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Equal(new[] { "Escutamos sempre,", "mas nunca achamos que vá acontecer connosco." }, Texts(response));
    }

    [Fact]
    public void Batch_FormalDialogueContextUsesEarlierAndLaterAddress()
    {
        var response = BatchMode.BuildBatchResponse(Request("Sr. diretor, um momento.", "Não sei.", "Certo, veja...", "Não fale comigo.", "O estoque acabou."),
            WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Equal(new[] { "Sr. diretor, um momento.", "Não sei.", "Certo, veja...", "Não fale comigo.", "O stock acabou." }, Texts(response));
        response = BatchMode.BuildBatchResponse(Request("Certo, veja...", "Não sei.", "Não concordo, detetive.", "O estoque acabou."),
            WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Contains("Certo, veja...", Texts(response));
    }

    [Fact]
    public void Batch_FormalAddressInDashedReplyKeepsCommandFormal()
    {
        var response = BatchMode.BuildBatchResponse(
            Request("Aparentemente não.", "Certo, veja...", "Uma mulher foi espancada\ne violentada.", "E Sebastian dá com os ombros,",
                "- e isso é \"aparentemente não\"?\n- Não gosto de seu tom, detetive."),
            WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Equal("Certo, veja...", Texts(response)[1]);
    }

    [Fact]
    public void Batch_FormalAddressInLaterDashedCueKeepsCommandFormal()
    {
        var response = BatchMode.BuildBatchResponse(
            Request("Bem, olhe um veículo parecido,", "em marca e modelo,\ncom uma placa semelhante", "envolveu-se numa série de\nestupros ontem à noite.",
                "- Não seja ridículo.\n- Alguém mais teria", "- acesso ao veículo, madame?\n- Já chega disto."),
            WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Equal("Bem, olhe um veículo parecido,", Texts(response)[0]);
    }

    private PluginRequest EpisodeRequest(string text, bool withEnglish, string english = "So we got prints and his picture now.")
    {
        var request = Request(text);
        request.Subtitle.FileName = Path.Combine(_folder, "Show.S01E01.720p.pt-br.srt");
        File.WriteAllText(request.Subtitle.FileName, request.Subtitle.SubRip);
        if (withEnglish)
            File.WriteAllText(Path.Combine(_folder, "Show.S01E01.720p.en.srt"), $"1\n00:00:00,000 --> 00:00:00,900\n{english}\n");
        return request;
    }

    [Fact]
    public void Batch_UsesTheEnglishSubtitleNextToTheFile()
    {
        var response = BatchMode.BuildBatchResponse(EpisodeRequest("Já temos digitais e sua foto.", true), WordListPath, new EmbeddingSettings());
        Assert.Equal(new[] { "Já temos impressões digitais e a sua foto." }, Texts(response));
        Assert.Contains("English reference Show.S01E01.720p.en.srt: Reference matched 1 of 1", response.Message);
    }

    [Fact]
    public void Batch_CausalQuestionUsesAlignedSubjectAndNegativeObject()
    {
        var response = BatchMode.BuildBatchResponse(EpisodeRequest("Por isso que nunca visitou nenhuma ilha?", true,
            "Is that why you never visited any islands?"), WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Equal(new[] { "Por isso é que nunca visitaste uma ilha?" }, Texts(response));
    }

    [Fact]
    public void Batch_HearingStatementUsesEnglishMeaningAndPluralOrder()
    {
        var response = BatchMode.BuildBatchResponse(EpisodeRequest("A gente sempre escuta,", true,
            "You just hear about this stuff."), WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Equal(new[] { "Ouvimos sempre," }, Texts(response));
        response = BatchMode.BuildBatchResponse(Request("Vamos pegar este cara."), WordListPath, new EmbeddingSettings(CheckMeaning: false));
        Assert.Equal(new[] { "Vamos apanhar este tipo." }, Texts(response));
    }

    [Fact]
    public void Batch_CountsLinesToReview()
    {
        var response = BatchMode.BuildBatchResponse(EpisodeRequest("Precisamos falar com seu marido. Ele está em casa?", true,
            "We need to talk to your husband. Is he home?"), WordListPath, new EmbeddingSettings());
        Assert.Equal(new[] { "Precisamos de falar com o seu marido. Ele está em casa?" }, Texts(response));
        Assert.Contains("1 line(s) to review", response.Message);
    }

    [Fact]
    public void Batch_ReferenceCanBeSwitchedOffAndIsOptional()
    {
        var off = EpisodeRequest("Já temos digitais e sua foto.", true);
        off.Settings = (ConversionOptions.Default with { UseReference = false }).ToJson();
        off.SettingsVersion = ConversionOptions.Version;
        var response = BatchMode.BuildBatchResponse(off, WordListPath, new EmbeddingSettings());
        Assert.Equal(new[] { "Já temos impressões digitais e a tua foto." }, Texts(response));
        Assert.DoesNotContain("English reference", response.Message);

        File.Delete(Path.Combine(_folder, "Show.S01E01.720p.en.srt"));
        response = BatchMode.BuildBatchResponse(EpisodeRequest("Já temos digitais e sua foto.", false), WordListPath, new EmbeddingSettings());
        Assert.Equal(new[] { "Já temos impressões digitais e a tua foto." }, Texts(response));
        Assert.DoesNotContain("English reference", response.Message);
    }

    [Fact]
    public void Configure_RemembersTheReferenceOption()
    {
        var json = (ConversionOptions.Default with { UseReference = false }).ToJson();
        var request = new PluginRequest { Settings = json, SettingsVersion = ConversionOptions.Version };
        Assert.False(ConversionOptions.Load(request).UseReference);
        Assert.True(ConversionOptions.Load(new PluginRequest()).UseReference);
    }

    [OptInFact("PTPT_TEST_VULKAN", "real MiniLM on a Vulkan GPU in Batch convert")]
    public void Batch_RealEpisodeWithMiniLmOnVulkan()
    {
        // PTPT_SOURCE_SRT: a real PT-BR subtitle (its English .srt next to it is not needed here).
        var source = Environment.GetEnvironmentVariable("PTPT_SOURCE_SRT")
                     ?? throw new InvalidOperationException("Set PTPT_SOURCE_SRT to a PT-BR .srt file.");
        var folder = Environment.GetEnvironmentVariable("PTPT_EMBEDDING_DIR") ?? EmbeddingAssets.DirectoryPath;
        var settings = new EmbeddingSettings(Directory.GetFiles(folder, "llama-server", SearchOption.AllDirectories).First(),
            Path.Combine(folder, EmbeddingAssets.ModelFile), true, true, Environment.GetEnvironmentVariable("PTPT_VULKAN_DEVICE") ?? "Vulkan0");
        var request = new PluginRequest
        {
            RequestType = PluginRequestType.Batch,
            Subtitle = new PluginSubtitle { Format = "SubRip", FileName = source, SubRip = ReferenceAligner.Decode(File.ReadAllBytes(source)) },
        };
        var response = BatchMode.BuildBatchResponse(request, WordListPath, settings);
        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Contains("MiniLM ran on Vulkan", response.Message);
        Assert.Contains("line(s) to review", response.Message);
        Assert.Contains(Texts(response), t => t.Contains("o seu rapaz"));
    }

    [Fact]
    public void Batch_UsesOriginalPreviousCueForPossessives()
    {
        var response = BatchMode.BuildBatchResponse(Request(
            "E me divorciando do cretino.",
            "- Já até mandei suas coisas.\n- Para onde?",
            "Precisamos falar com\nseu marido. Ele está em casa?"), WordListPath);
        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Equal(new[]
        {
            "E a divorciar-me do cretino.",
            "- Até, já lhe mandei as suas coisas.\n- Para onde?",
            "Precisamos de falar com\no seu marido. Ele está em casa?",
        }, Texts(response));
    }

    [Fact]
    public void Batch_ConvertsAllLines()
    {
        var response = BatchMode.BuildBatchResponse(Request("O trem chegou.", "Bom dia.", "Estou fazendo o jantar."), WordListPath);

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Equal(new[] { "O comboio chegou.", "Bom dia.", "Estou a fazer o jantar." }, Texts(response));
        Assert.Equal(ConversionOptions.Version, response.SettingsVersion);
    }

    [Fact]
    public void Batch_KeepsTimingsAndMultiLineText()
    {
        var response = BatchMode.BuildBatchResponse(Request("Bom dia.", "- Oi!\n- Tá bom."), WordListPath);

        var blocks = SubRipParser.Parse(response.Subtitle!.Native);
        Assert.Equal(new long[] { 0, 1000 }, blocks.Select(b => b.StartMs));
        Assert.Equal(new long[] { 900, 1900 }, blocks.Select(b => b.EndMs));
        Assert.Equal("- Olá!\n- Está bem.", blocks[1].Text);
    }

    [Fact]
    public void Batch_NothingToConvert_ReturnsOkWithoutSubtitle()
    {
        var response = BatchMode.BuildBatchResponse(Request("Bom dia.", "Estou a ver."), WordListPath);

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Batch_UsesSavedSettings()
    {
        var request = Request("Estou fazendo um fato no trem.");
        request.Settings = new ConversionOptions(Vocabulary: false, Progressive: false).ToJson();
        request.SettingsVersion = ConversionOptions.Version;

        var response = BatchMode.BuildBatchResponse(request, WordListPath);

        Assert.Equal(new[] { "Estou fazendo um facto no trem." }, Texts(response));
        Assert.False(response.Settings!.Value.GetProperty("vocabulary").GetBoolean());
    }

    [Fact]
    public void Settings_WrongVersionFallsBackToDefaults()
    {
        var request = new PluginRequest
        {
            Settings = JsonSerializer.SerializeToElement(new { spelling = false }),
            SettingsVersion = 99,
        };

        Assert.Equal(ConversionOptions.Default, ConversionOptions.Load(request));
    }

    [Fact]
    public void Configure_ReturnsSettings()
    {
        var response = BatchMode.BuildConfigureResponse(new ConversionOptions(YouToTu: false));

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Equal(ConversionOptions.Version, response.SettingsVersion);
        var request = new PluginRequest { Settings = response.Settings, SettingsVersion = response.SettingsVersion };
        Assert.Equal(new ConversionOptions(YouToTu: false), ConversionOptions.Load(request));
    }

    [Fact]
    public void Batch_UsesLocalWordList()
    {
        File.WriteAllText(WordListPath, """
            <?xml version="1.0" encoding="utf-8"?>
            <Words>
              <Word br="balada" pt="discoteca" />
              <Ignore br="trem" />
            </Words>
            """);

        var response = BatchMode.BuildBatchResponse(Request("Vamos à balada de trem."), WordListPath);

        Assert.Equal(new[] { "Vamos à discoteca de trem." }, Texts(response));
    }

    [Fact]
    public void Batch_UnreadableLocalWordList_IsError()
    {
        File.WriteAllText(WordListPath, "<Words><Word");

        var response = BatchMode.BuildBatchResponse(Request("O trem chegou."), WordListPath);

        Assert.Equal(PluginStatus.Error, response.Status);
    }

    [Fact]
    public void LocalWordList_SaveAndLoadRoundTrip()
    {
        var list = new PortugueseLocalWordList();
        list.Words.Add(("geladeira", "frigorífico"));
        list.Ignored.Add("garota");
        list.Save(WordListPath);

        var loaded = PortugueseLocalWordList.Load(WordListPath);

        Assert.Equal(list.Words, loaded.Words);
        Assert.Equal(list.Ignored, loaded.Ignored);
    }
}
