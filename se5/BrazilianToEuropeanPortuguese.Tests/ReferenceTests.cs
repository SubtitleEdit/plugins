using System.Text;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;
using Xunit.Abstractions;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class OptInFactAttribute : FactAttribute
{
    public OptInFactAttribute(string variable, string reason)
    {
        if (Environment.GetEnvironmentVariable(variable) != "1") Skip = $"Opt-in ({variable}=1): {reason}";
    }
}

[Collection("XdgDataHome")]
public sealed class ReferenceTests(ITestOutputHelper output)
{
    private static readonly PortugueseConverter Converter = new(ConversionOptions.Default);

    private static SrtBlock Cue(long start, long end, string text) => new() { StartMs = start, EndMs = end, Text = text };

    [Fact]
    public void EmptiedLineIsRemovedFromTheSubtitle()
    {
        var blocks = new List<SrtBlock> { Cue(0, 1000, "Um."), Cue(1000, 2000, "Dois."), Cue(2000, 3000, "Três.") };
        var response = BatchMode.BuildOkResponse(blocks, new Dictionary<int, string> { [1] = "  ", [2] = "Três!" }, ConversionOptions.Default);
        var texts = SubRipParser.Parse(response.Subtitle!.Native!).Select(b => b.Text).ToArray();
        Assert.Equal(new[] { "Um.", "Três!" }, texts);
    }

    [Theory]
    [InlineData("O policial chegou.", "The officer arrived.", "O agente chegou.")]
    [InlineData("Os policiais chegaram.", "The officers arrived.", "Os agentes chegaram.")]
    [InlineData("Um policial foi baleado.", "An officer was shot.", "Um agente foi baleado.")]
    [InlineData("O policial chegou.", "He arrived.", "O polícia chegou.")]
    [InlineData("Chame a polícia.", "Call the police.", "Chama a polícia.")]
    public void OfficerInTheEnglishMeansAgente(string br, string english, string expected) =>
        Assert.Equal(expected, Converter.Convert(br, null, ReferenceHint.FromText(english)));

    [Fact]
    public void ManualEditReplacesTheProposalAndIsSelected()
    {
        var proposal = new ChangeProposal(0, "Tem uma ligação\ncom o chefe.", "Há uma chamada\ncom o chefe.");
        proposal.Include = false;
        proposal.ConvertedText = "Tens uma chamada\r\ncom o chefe.";
        Assert.Equal("Tens uma chamada\ncom o chefe.", proposal.ConvertedText);
        Assert.True(proposal.Include);
        Assert.Equal(proposal.ConvertedText, proposal.ExpectedText);
        proposal.ConvertedText = "Há uma chamada\ncom o chefe.";
        Assert.Equal("Há uma chamada\ncom o chefe.", proposal.ConvertedText);
    }

    [Theory]
    [InlineData("Tem uma ligação\ncom o chefe da PBA às 13h.", "You have a call with the PBA chief at 1:00,", "Tens uma chamada\ncom o chefe da PBA às 13h.")]
    [InlineData("Tem uma reunião\ncom TARU às 15h,", "you have a briefing with TARU at 3:00 p.m.,", "Tens uma reunião\ncom TARU às 15h,")]
    [InlineData("Tem uma consulta marcada.", "You have an appointment.", "Tens uma consulta marcada.")]
    public void HavingAnAppointmentIsNotExistential(string br, string english, string expected) =>
        Assert.Equal(expected, Converter.Convert(br, null, ReferenceHint.FromText(english)));

    [Theory]
    [InlineData("acha", "achamos")]
    [InlineData("pensa", "pensamos")]
    [InlineData("imagina", "imaginamos")]
    public void InclusiveWeContinuesAcrossAdjacentCues(string verb, string plural)
    {
        var source = $"mas nunca {verb} que vai acontecer contigo.";
        var blocks = new[] { Cue(0, 1500, "A gente sempre escuta,"), Cue(1501, 3000, source) };
        var english = new string?[] { "You just hear about this stuff.", "Never think it's going to happen to you." };
        var hint = ReferenceHint.FromContext(blocks, 1, english);
        Assert.True(hint!.InclusiveContinuation);
        Assert.Equal($"mas nunca {plural} que vá acontecer connosco.", Converter.Convert(source, blocks[0].Text, hint));
        blocks[1].StartMs = 7000;
        Assert.False(ReferenceHint.FromContext(blocks, 1, english)!.InclusiveContinuation);
    }

    [Theory]
    [InlineData("Ela sempre escuta,", "Never think it's going to happen to you.")]
    [InlineData("A gente sempre escuta,", "He never thinks it's going to happen to you.")]
    public void InclusiveContinuationDoesNotGuessAnotherSubject(string previous, string english)
    {
        var blocks = new[] { Cue(0, 1500, previous), Cue(1501, 3000, "mas nunca acha que vai acontecer contigo.") };
        Assert.False(ReferenceHint.FromContext(blocks, 1, new string?[] { null, english })!.InclusiveContinuation);
    }

    [Theory]
    [InlineData("You just hear about this stuff.", true)]
    [InlineData("We always heard about it.", true)]
    [InlineData("She hears about these things.", true)]
    [InlineData("Listen carefully.", false)]
    [InlineData("We listen to music.", false)]
    [InlineData("Listen, I heard about it.", false)]
    [InlineData("Did you hear that sound?", false)]
    public void HearAboutReferenceHint(string english, bool expected) =>
        Assert.Equal(expected, ReferenceHint.FromText(english)!.HearAbout);

    [Theory]
    [InlineData("Is that why you never dated any officers?", true, true)]
    [InlineData("Is that why you already arrived?", true, false)]
    [InlineData("Is that why he never dated any officers?", false, true)]
    [InlineData("What happened to you?", false, false)]
    [InlineData("I never stayed any longer.", false, false)]
    [InlineData("I never wanted any more.", false, false)]
    public void ReferenceSubjectAdverbsAndNeverAnyHints(string english, bool subject, bool any)
    {
        var hint = ReferenceHint.FromText(english)!;
        Assert.Equal(subject, hint.SecondPersonSubject);
        Assert.Equal(any, hint.NeverAnyObject);
    }

    [Theory]
    [InlineData("Bem, Sr. vice-cônsul,")]
    [InlineData("Bom dia, professora.")]
    [InlineData("Senhor Silva, posso entrar?")]
    [InlineData("Não concordo, detetive.")]
    [InlineData("- Muito obrigado, doutora.")]
    [InlineData("<i>Capitão, espere.</i>")]
    public void NearbyPortugueseAddressPreservesFormalCommands(string address)
    {
        var blocks = new[] { Cue(0, 1500, address), Cue(2000, 3500, "Não sei."), Cue(4000, 5500, "Certo, veja...") };
        var hint = ReferenceHint.FromContext(blocks, 2);
        Assert.True(hint!.FormalAddress);
        Assert.Equal("Certo, veja...", Converter.Convert(blocks[2].Text, blocks[1].Text, hint));
    }

    [Theory]
    [InlineData("I disagree, Detective.")]
    [InlineData("Well, Mr. Deputy Consul,")]
    [InlineData("Yes, sir.")]
    [InlineData("Doctor Smith, please listen.")]
    [InlineData("Thank you, Your Honor.")]
    public void NearbyEnglishAddressPreservesFormalCommands(string address)
    {
        var blocks = new[] { Cue(0, 1500, "Certo, veja..."), Cue(2000, 3500, "Não sei."), Cue(4000, 5500, "Não concordo.") };
        var english = new string?[] { "Okay, look--", "I don't know.", address };
        var hint = ReferenceHint.FromContext(blocks, 0, english);
        Assert.True(hint!.FormalAddress);
        Assert.Equal("Certo, veja...", Converter.Convert(blocks[0].Text, null, hint));
    }

    [Theory]
    [InlineData("O detetive chegou.", "The detective arrived.")]
    [InlineData("Falei com o professor.", "I spoke to the professor.")]
    [InlineData("O senhor saiu.", "The gentleman left.")]
    [InlineData("Foi uma decisão do juiz.", "It was the judge's decision.")]
    [InlineData("Você sabe.", "You know.")]
    public void MentionedTitlesAndEnglishYouDoNotEstablishFormalContext(string text, string english)
    {
        var blocks = new[] { Cue(0, 1500, text), Cue(2000, 3500, "Não sei."), Cue(4000, 5500, "Certo, veja...") };
        var reference = new string?[] { english, "I don't know.", "Okay, look--" };
        Assert.False(ReferenceHint.FromContext(blocks, 2, reference)!.FormalAddress);
        Assert.Equal("Certo, vê...", Converter.Convert(blocks[2].Text, blocks[1].Text, ReferenceHint.FromContext(blocks, 2, reference)));
    }

    [Fact]
    public void FormalContextStopsAtTimingAndInformalBoundaries()
    {
        var gap = new[] { Cue(0, 1000, "Senhor, espere."), Cue(6001, 7000, "Certo, veja...") };
        Assert.False(DialogueRegister.IsFormal(gap, 1));
        var continuous = Enumerable.Range(0, 12).Select(i => Cue(i * 3000, i * 3000 + 2500, i == 0 ? "Senhor, espere." : "Certo, veja...")).ToArray();
        Assert.True(DialogueRegister.IsFormal(continuous, 10));
        Assert.False(DialogueRegister.IsFormal(continuous, 11));
        var dense = Enumerable.Range(0, 14).Select(i => Cue(i * 1000, i * 1000 + 900, i == 0 ? "Senhor, espere." : "Certo, veja...")).ToArray();
        Assert.True(DialogueRegister.IsFormal(dense, 12));
        Assert.False(DialogueRegister.IsFormal(dense, 13));
        var informal = new[] { Cue(0, 1000, "Senhor, espere."), Cue(2000, 3000, "Tu sabes."), Cue(4000, 5000, "Certo, veja...") };
        Assert.False(DialogueRegister.IsFormal(informal, 2));
        Assert.False(DialogueRegister.IsFormal(informal, 1));
        var ahead = new[] { Cue(0, 1000, "Certo, veja..."), Cue(2000, 3000, "Tu sabes."), Cue(4000, 5000, "Senhor, espere.") };
        Assert.False(DialogueRegister.IsFormal(ahead, 0));
    }

    [Theory]
    [InlineData("Diga a sua mãe.", "Diga à sua mãe.")]
    [InlineData("Não fale comigo.", "Não fale comigo.")]
    [InlineData("Você tem razão.", "Tem razão.")]
    [InlineData("Posso falar com você?", "Posso falar consigo?")]
    [InlineData("Isto é para você.", "Isto é para si.")]
    [InlineData("Cale a boca.", "Cale-se.")]
    [InlineData("Lembre de fechar a porta.", "Lembre-se de fechar a porta.")]
    public void FormalContextIsSharedByAddressPronounsAndCommands(string source, string expected)
    {
        var blocks = new[] { Cue(0, 1500, "Doutora, um momento."), Cue(2000, 3500, "Não sei."), Cue(4000, 5500, source) };
        var hint = ReferenceHint.FromContext(blocks, 2);
        Assert.Equal(expected, Converter.Convert(source, blocks[1].Text, hint));
        Assert.Equal(expected, Converter.Convert(expected, blocks[1].Text, hint));
        Assert.Null(ReferenceReview.Note("Você tem a sua chave.", hint));
    }

    [Theory]
    [InlineData("Ninguém leva as tuas coisas.", false, false)]
    [InlineData("I already sent him his stuff.", false, true)]
    [InlineData("This was his last message to us,", false, true)]
    [InlineData("Now, your brother Tiago,", true, false)]
    [InlineData("I sent her your things.", true, true)]
    public void PossessiveHints(string english, bool second, bool third)
    {
        var hint = ReferenceHint.FromText(english)!;
        Assert.Equal(second, hint.SecondPossessive);
        Assert.Equal(third, hint.ThirdPossessive);
        Assert.Equal(third && !second, hint.KeepsThirdPersonPossessive);
    }

    [Theory]
    [InlineData("Is he okay?", true)]
    [InlineData("They're home.", true)]
    [InlineData("Are you okay?", false)]
    [InlineData("You're right, he is.", false)]
    [InlineData("Okay?", false)]
    public void DroppedYouHint(string english, bool excluded) =>
        Assert.Equal(excluded, ReferenceHint.FromText(english)!.ExcludesDroppedYou);

    [Fact]
    public void NoReferenceTextGivesNoHint()
    {
        Assert.Null(ReferenceHint.FromText(null));
        Assert.Null(ReferenceHint.FromText("  "));
    }

    [Theory]
    [InlineData("DEMARCUS (groans): Yeah, I'm seeing double.", "Yeah, I'm seeing double.")]
    [InlineData("TERESA: There he is!\n- Hey!", "There he is! Hey!")]
    [InlineData("<i>WOMAN: * Start spreading the news</i>", "Start spreading the news")]
    [InlineData("[door closes] It's 10:30.", "It's 10:30.")]
    [InlineData("{\\an8}Mr. Banse", "Mr. Banse")]
    public void CleanRemovesTagsAndHearingImpairedText(string text, string expected) =>
        Assert.Equal(expected, ReferenceAligner.Clean(text));

    [Fact]
    public void DecodesUtf8Bom16AndWindows1252()
    {
        Assert.Equal("ação", ReferenceAligner.Decode(new byte[] { 0xEF, 0xBB, 0xBF }.Concat(Encoding.UTF8.GetBytes("ação")).ToArray()));
        Assert.Equal("“his”", ReferenceAligner.Decode(new byte[] { 0x93, 0x68, 0x69, 0x73, 0x94 }));
        Assert.Equal("you", ReferenceAligner.Decode(new byte[] { 0xFF, 0xFE }.Concat(Encoding.Unicode.GetBytes("you")).ToArray()));
    }

    [Fact]
    public void LooksEnglishNeedsEnglishFunctionWords()
    {
        Assert.True(ReferenceAligner.LooksEnglish(new[] { Cue(0, 1, "What are you doing?"), Cue(1, 2, "Fine.") }));
        Assert.False(ReferenceAligner.LooksEnglish(new[] { Cue(0, 1, "¿Qué estás haciendo?"), Cue(1, 2, "Nada.") }));
    }

    [Fact]
    public void TimingJoinsOverlappingCuesAndSkipsGaps()
    {
        var source = new[] { Cue(1000, 4000, "Já temos digitais e sua foto."), Cue(10_000, 12_000, "Oi."), Cue(20_000, 21_000, "♪") };
        var reference = new[]
        {
            Cue(900, 2400, "So we got prints"), Cue(2500, 4100, "and his picture now."), Cue(30_000, 31_000, "Hi."),
        };
        var alignment = ReferenceAligner.AlignByTiming(source, reference);
        Assert.Equal("So we got prints\nand his picture now.", alignment.Texts[0]);
        Assert.Null(alignment.Texts[1]);
        Assert.Null(alignment.Texts[2]);
        Assert.Equal(2, alignment.Total);
        Assert.Equal(1, alignment.Matched);
    }

    [Fact]
    public void ShortOverlapAtCueBoundaryIsIgnored()
    {
        var alignment = ReferenceAligner.AlignByTiming(new[] { Cue(1000, 3000, "Ele sabe.") },
            new[] { Cue(0, 1100, "Your car."), Cue(1050, 3000, "He knows.") });
        Assert.Equal("He knows.", alignment.Texts[0]);
    }

    [Fact]
    public void FitTimeMapRecoversFrameRateAndOffsetDespiteOutliers()
    {
        var random = new Random(7);
        var matches = Enumerable.Range(0, 60).Select(i =>
        {
            var source = i * 40_000.0;
            var reference = source * 1.0427 + 4200 + random.Next(-300, 300);
            return (source, i % 6 == 0 ? reference + 25_000 : reference);
        }).ToList();
        var map = ReferenceAligner.FitTimeMap(matches)!;
        Assert.InRange(map.Scale, 1.040, 1.045);
        Assert.InRange(map.Map(1_000_000) - (1_000_000 * 1.0427 + 4200), -500, 500);
    }

    [Fact]
    public void FitTimeMapKeepsIdentityAndRejectsTooFew()
    {
        var aligned = Enumerable.Range(0, 30).Select(i => (i * 50_000.0, i * 50_000.0 + (i % 2 == 0 ? 120 : -90))).ToList();
        Assert.True(ReferenceAligner.FitTimeMap(aligned)!.IsIdentity);
        Assert.Null(ReferenceAligner.FitTimeMap(aligned.Take(5).ToList()));
    }

    [Fact]
    public void MutualMatchesRejectAmbiguousPairs()
    {
        float[] V(params float[] v) => v;
        var source = new[] { V(1, 0, 0), V(0, 1, 0), V(0, 0, 1) };
        var reference = new[] { V(1, 0, 0), V(0, 0.9f, 0.436f), V(0, 0.9f, 0.436f), V(0, 0, 1) };
        var matches = ReferenceAligner.MutualMatches(source, reference);
        Assert.Contains((0, 0, 1.0), matches.Select(m => (m.Source, m.Reference, Math.Round(m.Score, 3))));
        Assert.DoesNotContain(matches, m => m.Source == 1);
        Assert.Contains(matches, m => m.Source == 2 && m.Reference == 3);
    }

    /// <summary>A reference 4 s late and stretched, with one scene cut out, still pairs every line.</summary>
    [Fact]
    public void EmbeddingAlignmentFixesShiftStretchAndCut()
    {
        const int count = 120;
        var source = new List<SrtBlock>();
        var reference = new List<SrtBlock>();
        var sourceVectors = new List<float[]?>();
        var referenceVectors = new List<float[]?>();
        for (var i = 0; i < count; i++)
        {
            long start = i * 20_000L;
            source.Add(Cue(start, start + 2500, $"Linha portuguesa número {i} com bastante texto"));
            sourceVectors.Add(Basis(i, count));
            var cut = i >= 80 ? 7000 : 0;
            var mapped = (long)(start * 1.0427) + 4000 - cut;
            reference.Add(Cue(mapped, mapped + (long)(2500 * 1.0427), $"English line number {i} with plenty of text"));
            referenceVectors.Add(Basis(i, count));
        }

        var timingOnly = ReferenceAligner.AlignByTiming(source, reference);
        Assert.True(timingOnly.Coverage < 0.3);
        var alignment = ReferenceAligner.AlignWithEmbeddings(source, reference, sourceVectors, referenceVectors);
        for (var i = 0; i < count; i++)
        {
            Assert.Equal($"English line number {i} with plenty of text", alignment.Texts[i]);
        }
        Assert.InRange(alignment.Map.Scale, 1.04, 1.045);
    }

    [Fact]
    public void EmbeddingAlignmentDropsPairsThatAreNotTranslations()
    {
        var source = new[] { Cue(0, 2000, "Bom dia a todos vocês aqui presentes"), Cue(5000, 7000, "Bom.") };
        var reference = new[] { Cue(0, 2000, "Good morning to all of you here"), Cue(5000, 7000, "I hate keeping secrets.") };
        var alignment = ReferenceAligner.AlignWithEmbeddings(source, reference,
            new[] { Basis(0, 3), Basis(1, 3) }, new[] { Basis(0, 3), Basis(2, 3) });
        Assert.Equal("Good morning to all of you here", alignment.Texts[0]);
        Assert.Null(alignment.Texts[1]);
        Assert.Equal(1, alignment.Rejected);
    }

    private static float[] Basis(int index, int size)
    {
        var vector = new float[size];
        vector[index] = 1;
        return vector;
    }

    [Theory]
    [InlineData("Carteira de motorista da Flórida.\nJá temos digitais e sua foto.", "so we got prints and his picture now.",
        "Carta de condução da Flórida.\nJá temos impressões digitais e a sua foto.",
        "Carta de condução da Flórida.\nJá temos impressões digitais e a tua foto.")]
    [InlineData("Esta foi sua última mensagem\npara nós.", "This was his last message to us,",
        "Esta foi a sua última mensagem\npara nós.", "Esta foi a tua última mensagem\npara nós.")]
    [InlineData("Já até mandei suas coisas.", "I already sent him his stuff.",
        "Até, já lhe mandei as suas coisas.", "Já até mandei as tuas coisas.")]
    [InlineData("Está bem?", "Is he okay?", "Está bem?", "Estás bem?")]
    public void ThirdPersonReferenceKeepsSeuAndThirdPersonVerbs(string text, string english, string withReference, string without)
    {
        Assert.Equal(without, Converter.Convert(text));
        Assert.Equal(withReference, Converter.Convert(text, null, ReferenceHint.FromText(english)));
    }

    /// <summary>English "your" cannot tell "tu" from formal "o seu marido": it must never force "teu".</summary>
    [Theory]
    [InlineData("Precisamos falar com\nseu marido. Ele está em casa?", "We need to talk to your husband. Is he home?")]
    [InlineData("Agora, seu irmão Tiago,", "Now, your brother Tiago,")]
    [InlineData("Você está bem?", "Are you okay?")]
    [InlineData("Não fala assim.", "Don't talk like that.")]
    [InlineData("Se afaste!", "Step aside!")]
    public void SecondPersonOrNeutralReferenceChangesNothing(string text, string english) =>
        Assert.Equal(Converter.Convert(text), Converter.Convert(text, null, ReferenceHint.FromText(english)));

    [Fact]
    public void FamilySubjectArticleDoesNotOverrideThirdPersonPossessive()
    {
        const string source = "Mamãe tá brava com ele\npor quê ficou ao seu lado.";
        const string expected = "Mãe está zangada com ele\npor quê ficou ao seu lado.";
        Assert.Equal(expected, Converter.Convert(source));
        Assert.Equal("A " + expected, Converter.Convert(source, null,
            ReferenceHint.FromText("Mom's mad at him because he took your side.")));
    }

    [Fact]
    public void ParsesVulkanDevicesAndSkipsSoftwareRenderers()
    {
        var devices = EmbeddingRuntime.ParseDevices(
            "Available devices:\n  Vulkan0: AMD Radeon RX 560 Series (RADV POLARIS11) (4096 MiB, 2045 MiB free)\n  Vulkan1: llvmpipe (LLVM 17)\n");
        var device = Assert.Single(devices);
        Assert.Equal("Vulkan0", device.Id);
        Assert.StartsWith("AMD Radeon RX 560", device.Name);
    }

    [Theory]
    [InlineData("../evil")]
    [InlineData("/etc/passwd")]
    [InlineData("a\\b")]
    public void RuntimeArchivePathsCannotEscape(string entry) =>
        Assert.Throws<InvalidDataException>(() => EmbeddingAssets.SafePath(Path.GetTempPath(), entry));

    [Fact]
    public void ChangeProposalShowsReferenceOnOneLine()
    {
        Assert.Equal("EN: his\nstuff".Replace('\n', ' '), new ChangeProposal(0, "a", "b", "his\nstuff").ReferenceText);
        Assert.False(new ChangeProposal(0, "a", "b").HasReference);
    }

    // Real assets downloaded by the plugin, or PTPT_EMBEDDING_DIR with llama-server and the GGUF.
    private static EmbeddingSettings RealSettings(bool gpu)
    {
        var folder = Environment.GetEnvironmentVariable("PTPT_EMBEDDING_DIR") ?? EmbeddingAssets.DirectoryPath;
        var runtime = Directory.GetFiles(folder, "llama-server", SearchOption.AllDirectories).First();
        return new EmbeddingSettings(runtime, Path.Combine(folder, EmbeddingAssets.ModelFile), true, gpu,
            Environment.GetEnvironmentVariable("PTPT_VULKAN_DEVICE") ?? "Vulkan0");
    }

    private static (List<SrtBlock> Source, List<SrtBlock> Reference) RealPair()
    {
        // PTPT_SOURCE_SRT and PTPT_ENGLISH_SRT: a real PT-BR subtitle and the English subtitle of the same release.
        var source = Environment.GetEnvironmentVariable("PTPT_SOURCE_SRT")
                     ?? throw new InvalidOperationException("Set PTPT_SOURCE_SRT to a PT-BR .srt file.");
        var english = Environment.GetEnvironmentVariable("PTPT_ENGLISH_SRT")
                      ?? throw new InvalidOperationException("Set PTPT_ENGLISH_SRT to the English .srt of the same release.");
        return (ReferenceAligner.Load(source), ReferenceAligner.Load(english));
    }

    [OptInFact("PTPT_TEST_EMBEDDING", "real MiniLM on the CPU")]
    public Task RealEmbeddingAlignmentOnCpu() => RealEmbeddingAlignment(false);

    [OptInFact("PTPT_TEST_VULKAN", "real MiniLM on a Vulkan GPU")]
    public Task RealEmbeddingAlignmentOnVulkan() => RealEmbeddingAlignment(true);

    private async Task RealEmbeddingAlignment(bool gpu)
    {
        var (source, reference) = RealPair();
        var shifted = reference.Select(r => Cue((long)(r.StartMs * 25.0 / 23.976) + 4200, (long)(r.EndMs * 25.0 / 23.976) + 4200, r.Text)).ToList();
        await using var runtime = await EmbeddingRuntime.StartAsync(RealSettings(gpu), new Progress<string>(output.WriteLine), default);
        Assert.Equal(gpu, runtime.UsesGpu);
        var sourceVectors = await runtime.EmbedCuesAsync(source.Select(b => b.Text).ToList(), new Progress<string>(_ => { }), "pt", default);
        var referenceVectors = await runtime.EmbedCuesAsync(reference.Select(b => b.Text).ToList(), new Progress<string>(_ => { }), "en", default);

        var alignment = ReferenceAligner.AlignWithEmbeddings(source, reference, sourceVectors, referenceVectors);
        var moved = ReferenceAligner.AlignWithEmbeddings(source, shifted, sourceVectors, referenceVectors);
        output.WriteLine(alignment.Describe());
        output.WriteLine(moved.Describe());
        Assert.True(alignment.Coverage > 0.95);
        Assert.True(moved.Coverage > 0.95);
        Assert.True(ReferenceAligner.AlignByTiming(source, shifted).Coverage < 0.9);
        var same = Enumerable.Range(0, source.Count).Count(i => alignment.Texts[i] == moved.Texts[i]);
        Assert.True(same > source.Count * 0.93, $"{same} of {source.Count} identical");
        // Cues the user reported; 739 drifts in the original files and needs the local correction.
        Assert.Contains("his picture", alignment.Texts[331]);
        Assert.Contains("his last message", alignment.Texts[726]);
        Assert.Contains("Anybody find anything yet?", alignment.Texts[738]);
        Assert.Contains("your father", alignment.Texts[818]);
    }

    [OptInFact("PTPT_TEST_UI", "native Avalonia window (run under xvfb-run)")]
    public void MainWindowAppliesReferenceToProposals()
    {
        var data = Path.Combine(Path.GetTempPath(), "ptpt-ui-" + Guid.NewGuid().ToString("N"));
        Environment.SetEnvironmentVariable("XDG_DATA_HOME", data);
        try
        {
            var english = Path.Combine(data, "episode.en.srt");
            Directory.CreateDirectory(data);
            File.WriteAllText(english, "1\n00:00:01,000 --> 00:00:04,000\nSo we got prints and his picture now.\n\n" +
                                       "2\n00:00:05,000 --> 00:00:07,000\nAre you okay?\n\n" +
                                       "3\n00:00:08,000 --> 00:00:10,000\nWe need to talk to your husband. Is he home?\n\n" +
                                       "4\n00:00:11,000 --> 00:00:13,000\nI'm not going to talk to you anymore.\n");
            var subtitle = Path.Combine(data, "episode.pt-br.srt");
            File.WriteAllText(subtitle, "");
            PluginBootstrap.BuildAvaloniaApp<App>().SetupWithoutStarting();
            var grammarWindow = new MainWindow(new PluginRequest
            {
                SettingsVersion = ConversionOptions.Version,
                Settings = new ConversionOptions(GrammarCheck: true, GrammarServer: "http://languagetool:8010/").ToJson(),
                Subtitle = new PluginSubtitle { SubRip = "1\n00:00:01,000 --> 00:00:03,000\nOlá.\n" },
            });
            Assert.True(grammarWindow.FindControl<CheckBox>("GrammarCheck")!.IsChecked);
            Assert.False(grammarWindow.FindControl<CheckBox>("GrammarAutoFix")!.IsChecked);
            Assert.False(grammarWindow.FindControl<CheckBox>("GrammarExternalServer")!.IsChecked);
            Assert.False(grammarWindow.FindControl<TextBox>("GrammarServer")!.IsVisible);
            Assert.NotNull(grammarWindow.FindControl<Button>("DownloadGrammarButton"));
            Assert.NotNull(grammarWindow.FindControl<Button>("DownloadJavaButton"));
            grammarWindow.FindControl<CheckBox>("GrammarExternalServer")!.IsChecked = true;
            Assert.True(grammarWindow.FindControl<TextBox>("GrammarServer")!.IsVisible);
            Assert.Equal("http://languagetool:8010/", grammarWindow.FindControl<TextBox>("GrammarServer")!.Text);
            grammarWindow.ApplyCheckResults(new[] { new CheckLine(0, "Olá.", "Olá.", null, null) },
                new Dictionary<int, CheckResult> { [0] = new("Olá.", "LanguageTool [TEST]: review only.") });
            Assert.Contains("LanguageTool", Assert.Single(grammarWindow.Proposals).ReviewNote);
            grammarWindow.Show();
            grammarWindow.FindControl<Button>("GrammarRulesButton")!
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var grammarRules = Assert.IsType<GrammarRulesWindow>(Assert.Single(grammarWindow.OwnedWindows));
            Assert.True(grammarRules.FindControl<CheckBox>("GrammarRule_TEST")!.IsChecked);
            grammarRules.FindControl<Button>("GrammarOnlyPreset")!
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Assert.Equal("other,spelling,style,typography", grammarRules.DisabledGroups);
            grammarRules.FindControl<CheckBox>("GrammarRule_MORFOLOGIK_RULE_PT_PT")!.IsChecked = false;
            grammarRules.FindControl<TextBox>("ExtraGrammarDisabledRules")!.Text = "BAD ID";
            grammarRules.FindControl<Button>("SaveGrammarRules")!
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Assert.Contains("Invalid LanguageTool", grammarRules.FindControl<TextBlock>("GrammarRulesError")!.Text);
            Assert.Contains("LanguageTool", Assert.Single(grammarWindow.Proposals).ReviewNote);
            grammarRules.FindControl<TextBox>("ExtraGrammarDisabledRules")!.Text = "";
            grammarRules.FindControl<Button>("SaveGrammarRules")!
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Empty(grammarWindow.Proposals);
            Assert.Contains("Run checks again", grammarWindow.ChecksStatusText);
            grammarWindow.FindControl<CheckBox>("GrammarCheck")!.IsChecked = false;
            Assert.Empty(grammarWindow.Proposals);
            grammarWindow.FindControl<TextBox>("GrammarServer")!.Text = "http://127.0.0.1:8081/";
            Assert.True(grammarWindow.FindControl<Button>("ApplyButton")!.IsEnabled);
            var repeatedWindow = new MainWindow(new PluginRequest
            {
                Subtitle = new PluginSubtitle
                {
                    Format = "SubRip",
                    SubRip = "1\n00:00:01,000 --> 00:00:04,000\nO trem chegou.\n\n" +
                             "2\n00:00:05,000 --> 00:00:07,000\nO trem chegou.\n",
                },
            });
            repeatedWindow.ApplyCheckResults(new[]
            {
                new CheckLine(0, "O trem chegou.", "O comboio chegou.", null, null),
                new CheckLine(1, "O trem chegou.", "O comboio chegou.", "O comboio chegou.", null),
            }, new Dictionary<int, CheckResult>
            {
                [1] = new("O comboio chegou.", "Second cue only."),
            });
            Assert.DoesNotContain("Second cue only.", repeatedWindow.Proposals.Single(p => p.LineIndex == 0).ReviewNote ?? "");
            Assert.Contains("Second cue only.", repeatedWindow.Proposals.Single(p => p.LineIndex == 1).ReviewNote);
            repeatedWindow.FindControl<CheckBox>("MeaningCheck")!.IsChecked = false;
            Assert.All(repeatedWindow.Proposals, p => Assert.DoesNotContain("Second cue only.", p.ReviewNote ?? ""));
            Assert.Contains("cleared because settings changed", repeatedWindow.ChecksStatusText);
            repeatedWindow.ApplyCheckResults(new[]
            {
                new CheckLine(0, "O trem chegou.", "O comboio chegou.", null, null),
            }, new Dictionary<int, CheckResult> { [0] = new("O comboio chegou.", "Old reference result.") });
            repeatedWindow.LoadReferenceAsync(english).GetAwaiter().GetResult();
            Assert.All(repeatedWindow.Proposals, p => Assert.DoesNotContain("Old reference result.", p.ReviewNote ?? ""));

            var filterWindow = new MainWindow(new PluginRequest
            {
                SettingsVersion = ConversionOptions.Version,
                Settings = new ConversionOptions(UseReference: false).ToJson(),
                Subtitle = new PluginSubtitle
                {
                    Format = "SubRip",
                    SubRip = "1\n00:00:01,000 --> 00:00:04,000\nO trem chegou.\n\n" +
                             "2\n00:00:05,000 --> 00:00:07,000\nBom dia.\n\n" +
                             "3\n00:00:08,000 --> 00:00:10,000\nBoa noite.\n\n" +
                             "4\n00:00:11,000 --> 00:00:13,000\nAté amanhã.\n",
                },
            });
            filterWindow.ApplyCheckResults(new[]
            {
                new CheckLine(1, "Bom dia.", "Bom dia.", "O comboio chegou.", null),
                new CheckLine(2, "Boa noite.", "Boa noite.", "Bom dia.", null),
                new CheckLine(3, "Até amanhã.", "Até amanhã.", "Boa noite.", null),
            }, new Dictionary<int, CheckResult>
            {
                [1] = new("Bom dia.", "MiniLM: check the translation."),
                [2] = new("Boa noite!", "Gervásio chose an alternative."),
                [3] = new("Até amanhã.", "Review this wording."),
            });
            filterWindow.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var filterNames = new[] { "RuleBasedFilter", "MiniLmFilter", "JudgeFilter", "OtherReviewFilter", "ManualEditFilter" };
            Assert.All(filterNames, name => Assert.True(filterWindow.FindControl<ToggleButton>(name)!.IsChecked));
            Assert.Equal(4, filterWindow.Proposals.Count);
            Assert.All(filterWindow.Proposals, p => Assert.True(p.VisibleInReview));
            void SetFilter(string name, bool enabled)
            {
                var button = filterWindow.FindControl<ToggleButton>(name)!;
                button.IsChecked = enabled;
                button.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }
            foreach (var name in filterNames) SetFilter(name, false);
            Assert.All(filterWindow.Proposals, p => Assert.False(p.VisibleInReview));
            Assert.Contains("Showing 0 of 4", filterWindow.FindControl<TextBlock>("SummaryLabel")!.Text);
            Assert.True(filterWindow.FindControl<Button>("ApplyButton")!.IsEnabled);
            var selectionButtons = filterWindow.FindControl<Control>("SelectButtons")!.GetVisualDescendants().OfType<Button>();
            Assert.Single(selectionButtons, b => Equals(b.Content, "Select none"))
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Assert.All(filterWindow.Proposals, p => Assert.False(p.Include));
            Assert.Single(selectionButtons, b => Equals(b.Content, "Select all"))
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Assert.All(filterWindow.Proposals, p => Assert.Equal(p.IsChange, p.Include));
            Assert.All(filterWindow.Proposals, p => Assert.False(p.VisibleInReview));
            SetFilter("RuleBasedFilter", true);
            Assert.Equal(0, Assert.Single(filterWindow.Proposals, p => p.VisibleInReview).LineIndex);
            SetFilter("RuleBasedFilter", false);
            SetFilter("MiniLmFilter", true);
            var miniReview = Assert.Single(filterWindow.Proposals, p => p.VisibleInReview);
            Assert.Equal(1, miniReview.LineIndex);
            miniReview.ConvertedText = "Bom dia!";
            miniReview.Include = false;
            SetFilter("MiniLmFilter", false);
            SetFilter("ManualEditFilter", true);
            Assert.Same(miniReview, Assert.Single(filterWindow.Proposals, p => p.VisibleInReview));
            Assert.False(miniReview.Include);
            SetFilter("MiniLmFilter", true);
            SetFilter("ManualEditFilter", false);
            Assert.True(miniReview.VisibleInReview);
            Assert.Equal("Bom dia!", miniReview.ConvertedText);
            SetFilter("MiniLmFilter", false);
            SetFilter("JudgeFilter", true);
            Assert.Equal(2, Assert.Single(filterWindow.Proposals, p => p.VisibleInReview).LineIndex);
            SetFilter("JudgeFilter", false);
            SetFilter("OtherReviewFilter", true);
            Assert.Equal(3, Assert.Single(filterWindow.Proposals, p => p.VisibleInReview).LineIndex);
            var visibleBorders = filterWindow.FindControl<ItemsControl>("ChangesList")!.GetVisualDescendants()
                .OfType<Border>().Where(b => b.DataContext is ChangeProposal && b.Padding == new Avalonia.Thickness(8, 6));
            Assert.Equal(4, visibleBorders.Count());
            Assert.Single(visibleBorders, b => b.IsVisible);
            filterWindow.FindControl<CheckBox>("VocabularyCheck")!.IsChecked = false;
            Assert.All(filterNames, name => Assert.Equal(name == "OtherReviewFilter",
                filterWindow.FindControl<ToggleButton>(name)!.IsChecked == true));
            filterWindow.Close();

            var contributionsOwner = new MainWindow(new PluginRequest());
            contributionsOwner.Show();
            contributionsOwner.FindControl<Button>("ContributeVocabularyButton")!
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var contributions = Assert.IsType<ContributionsWindow>(Assert.Single(contributionsOwner.OwnedWindows));
            Assert.Equal("Contribute vocabulary / Local word list",
                contributionsOwner.FindControl<Button>("ContributeVocabularyButton")!.Content);
            Assert.Equal(new[] { "Contribute vocabulary", "Custom word list", "Ignore word list" },
                contributions.FindControl<TabControl>("VocabularyTabs")!.Items.OfType<TabItem>().Select(t => t.Header));
            void ContributionClick(string name)
            {
                contributions.FindControl<Button>(name)!.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }
            void ContributionText(string name, string text)
            {
                contributions.FindControl<TextBox>(name)!.Text = text;
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }
            ContributionClick("AddContributionButton");
            Assert.Contains("required", contributions.FindControl<TextBlock>("ContributionError")!.Text);
            ContributionText("ContributionBrazilian", "teste");
            ContributionText("ContributionEuropean", "ensaio");
            ContributionText("ContributionBrazilianExample", "um; \"teste\"\nna prática");
            ContributionClick("AddContributionButton");
            var contributionList = contributions.FindControl<ListBox>("ContributionsList")!;
            Assert.Single(contributionList.Items);
            Assert.Equal("", contributions.FindControl<TextBox>("ContributionBrazilian")!.Text);
            contributionList.SelectedIndex = 0;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Equal("ensaio", contributions.FindControl<TextBox>("ContributionEuropean")!.Text);
            ContributionText("ContributionEuropean", "verificação");
            ContributionClick("UpdateContributionButton");
            Assert.Equal("verificação", Assert.IsType<VocabularyContribution>(Assert.Single(contributionList.Items)).European);
            ContributionText("ContributionBrazilian", "teste");
            ContributionText("ContributionEuropean", "outro sentido");
            var csv = contributions.ExportCsv();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Equal(2, VocabularyContributions.Parse(csv).Count);
            Assert.Throws<FormatException>(() => contributions.LoadCsv("invalid"));
            Assert.Equal(2, contributionList.Items.Count);
            contributions.LoadCsv(csv);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Equal(2, contributionList.Items.Count);
            Assert.DoesNotContain("Unsaved", contributions.FindControl<TextBlock>("ContributionStatus")!.Text);
            contributionList.SelectedIndex = 0;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            ContributionText("ContributionEuropean", "");
            Assert.Throws<FormatException>(() => contributions.ExportCsv());
            Assert.Equal(2, contributionList.Items.Count);
            contributionList.SelectedIndex = 1;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Equal(0, contributionList.SelectedIndex);
            Assert.Equal("", contributions.FindControl<TextBox>("ContributionEuropean")!.Text);
            ContributionText("ContributionEuropean", "verificação revista");
            ContributionClick("UpdateContributionButton");
            Assert.Equal("verificação revista", Assert.IsType<VocabularyContribution>(contributionList.Items[0]).European);
            contributionList.SelectedIndex = 0;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            ContributionClick("RemoveEntryButton");
            Assert.Single(contributionList.Items);
            contributions.Close();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.True(contributions.IsVisible);
            var discardDialog = Assert.Single(contributions.OwnedWindows);
            Assert.Single(discardDialog.GetVisualDescendants().OfType<Button>(), b => Equals(b.Content, "Keep editing"))
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.True(contributions.IsVisible);
            contributions.Close();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            discardDialog = Assert.Single(contributions.OwnedWindows);
            Assert.Single(discardDialog.GetVisualDescendants().OfType<Button>(), b => Equals(b.Content, "Discard"))
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.False(contributions.IsVisible);
            contributionsOwner.Close();

            var localPath = Path.Combine(data, "local-vocabulary.xml");
            var seedList = new PortugueseLocalWordList();
            seedList.Words.Add(("teste", "ensaio"));
            seedList.Ignored.Add("preservar");
            seedList.Save(localPath);
            var localEditor = new ContributionsWindow(localPath);
            localEditor.Show();
            var tabs = localEditor.FindControl<TabControl>("VocabularyTabs")!;
            var wordsList = localEditor.FindControl<ListBox>("WordsList")!;
            var ignoredList = localEditor.FindControl<ListBox>("IgnoredList")!;
            void LocalClick(string name)
            {
                localEditor.FindControl<Button>(name)!.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }
            void LocalText(string name, string text)
            {
                localEditor.FindControl<TextBox>(name)!.Text = text;
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }
            localEditor.LoadCsv(csv);
            Assert.Equal(("teste", "ensaio"), Assert.Single(PortugueseLocalWordList.Load(localPath).Words));
            Assert.False(localEditor.LocalWordListSaved);
            tabs.SelectedIndex = 1;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Contains(wordsList, localEditor.GetVisualDescendants());
            Assert.DoesNotContain(ignoredList, localEditor.GetVisualDescendants());
            LocalClick("AddWordButton");
            Assert.Contains("required", localEditor.FindControl<TextBlock>("LocalErrorWords")!.Text);
            localEditor.ImportLocalCsv("pt-BR;pt-PT\nteste;verificação\nexpressão;primeiro sentido\nexpressão;último sentido\n", false);
            Assert.Equal(2, wordsList.Items.Count);
            Assert.Contains(new WordPair("expressão", "último sentido"), wordsList.Items.OfType<WordPair>());
            var beforeInvalidImport = localEditor.ExportLocalCsv(false);
            Assert.Throws<FormatException>(() => localEditor.ImportLocalCsv("pt-BR;pt-PT\nválido;correto\ninválido;\n", false));
            Assert.Equal(beforeInvalidImport, localEditor.ExportLocalCsv(false));
            wordsList.SelectedIndex = 0;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            LocalText("PortugueseBox", "revisão");
            wordsList.SelectedIndex = 1;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Equal(0, wordsList.SelectedIndex);
            Assert.Equal("revisão", localEditor.FindControl<TextBox>("PortugueseBox")!.Text);
            Assert.Throws<FormatException>(() => localEditor.SaveLocalWords());
            LocalClick("UpdateWordButton");
            Assert.Contains(new WordPair("teste", "revisão"), wordsList.Items.OfType<WordPair>());
            tabs.SelectedIndex = 2;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Contains(ignoredList, localEditor.GetVisualDescendants());
            LocalClick("AddIgnoredButton");
            Assert.Contains("Enter", localEditor.FindControl<TextBlock>("LocalErrorIgnored")!.Text);
            LocalText("IgnoreBox", "teste");
            LocalClick("AddIgnoredButton");
            Assert.DoesNotContain(wordsList.Items.OfType<WordPair>(), w => w.Brazilian == "teste");
            localEditor.ImportLocalCsv("pt-BR\nexpressão\nTá Bom Bar\n", true);
            Assert.Empty(wordsList.Items);
            Assert.Equal(4, ignoredList.Items.Count);
            Assert.Equal(2, VocabularyContributions.Parse(localEditor.ExportCsv()).Count);
            LocalClick("SaveLocalIgnoredButton");
            var savedLocal = PortugueseLocalWordList.Load(localPath);
            Assert.True(localEditor.LocalWordListSaved);
            Assert.Equal(4, savedLocal.Ignored.Count);
            Assert.Equal("teste", new PortugueseConverter(ConversionOptions.Default, savedLocal).Convert("teste"));
            localEditor.ImportLocalCsv("pt-BR;pt-PT\nteste;ensaio\n", false);
            Assert.DoesNotContain("teste", ignoredList.Items.OfType<string>());
            tabs.SelectedIndex = 1;
            LocalClick("SaveLocalWordsButton");
            savedLocal = PortugueseLocalWordList.Load(localPath);
            Assert.Equal(("teste", "ensaio"), Assert.Single(savedLocal.Words));
            Assert.Equal("ensaio", new PortugueseConverter(ConversionOptions.Default, savedLocal).Convert("teste"));
            Assert.Equal(3, savedLocal.Ignored.Count);
            wordsList.SelectedIndex = 0;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            LocalText("BrazilianBox", "teste revisto");
            LocalClick("UpdateWordButton");
            Assert.Equal("teste revisto", Assert.IsType<WordPair>(Assert.Single(wordsList.Items)).Brazilian);
            wordsList.SelectedIndex = 0;
            LocalClick("RemoveWordButton");
            Assert.Empty(wordsList.Items);
            LocalText("BrazilianBox", "teste");
            LocalText("PortugueseBox", "ensaio");
            LocalText("IgnoreBox", "rascunho paralelo");
            LocalClick("AddWordButton");
            Assert.Equal("rascunho paralelo", localEditor.FindControl<TextBox>("IgnoreBox")!.Text);
            LocalClick("AddIgnoredButton");
            tabs.SelectedIndex = 2;
            ignoredList.SelectedItem = "rascunho paralelo";
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            LocalText("IgnoreBox", "expressão protegida");
            LocalClick("UpdateIgnoredButton");
            Assert.DoesNotContain("rascunho paralelo", ignoredList.Items.OfType<string>());
            Assert.Contains("expressão protegida", ignoredList.Items.OfType<string>());
            ignoredList.SelectedItem = "expressão protegida";
            LocalClick("RemoveIgnoredButton");
            Assert.Equal(3, ignoredList.Items.Count);
            LocalClick("SaveLocalIgnoredButton");
            Assert.Empty(Directory.GetFiles(data, "local-vocabulary.xml.*.tmp"));
            localEditor.Close();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.False(localEditor.IsVisible);
            var reopenedEditor = new ContributionsWindow(localPath);
            reopenedEditor.Show();
            Assert.Single(reopenedEditor.FindControl<ListBox>("WordsList")!.Items);
            Assert.Equal(3, reopenedEditor.FindControl<ListBox>("IgnoredList")!.Items.Count);
            reopenedEditor.FindControl<TabControl>("VocabularyTabs")!.SelectedIndex = 2;
            reopenedEditor.FindControl<TextBox>("IgnoreBox")!.Text = "unsaved draft";
            reopenedEditor.Close();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.True(reopenedEditor.IsVisible);
            var localDiscard = Assert.Single(reopenedEditor.OwnedWindows);
            Assert.Single(localDiscard.GetVisualDescendants().OfType<Button>(), b => Equals(b.Content, "Discard"))
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.False(reopenedEditor.IsVisible);
            Assert.DoesNotContain("unsaved draft", PortugueseLocalWordList.Load(localPath).Ignored);
            var damagedPath = Path.Combine(data, "damaged-vocabulary.xml");
            File.WriteAllText(damagedPath, "<not valid");
            var damagedEditor = new ContributionsWindow(damagedPath);
            Assert.Contains("Saving is disabled", damagedEditor.FindControl<TextBlock>("LocalErrorWords")!.Text);
            Assert.Throws<IOException>(() => damagedEditor.SaveLocalWords());
            Assert.Equal("<not valid", File.ReadAllText(damagedPath));
            damagedEditor.Close();

            var refreshPath = Path.Combine(data, "refresh-vocabulary.xml");
            var refreshOwner = new MainWindow(new PluginRequest
            {
                Subtitle = new PluginSubtitle { Format = "SubRip", SubRip = "1\n00:00:01,000 --> 00:00:04,000\nteste\n" },
            }, refreshPath);
            refreshOwner.Show();
            Assert.Empty(refreshOwner.Proposals);
            refreshOwner.FindControl<Button>("ContributeVocabularyButton")!
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var refreshEditor = Assert.IsType<ContributionsWindow>(Assert.Single(refreshOwner.OwnedWindows));
            refreshEditor.ImportLocalCsv("pt-BR;pt-PT\nteste;ensaio\n", false);
            refreshEditor.SaveLocalWords();
            refreshEditor.Close();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Equal("ensaio", Assert.Single(refreshOwner.Proposals).ConvertedText);
            refreshOwner.Close();
            var batchConfig = new MainWindow(new PluginRequest { RequestType = PluginRequestType.Configure }, refreshPath);
            batchConfig.Show();
            batchConfig.FindControl<Button>("ContributeVocabularyButton")!
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var configEditor = Assert.IsType<ContributionsWindow>(Assert.Single(batchConfig.OwnedWindows));
            configEditor.ImportLocalCsv("pt-BR\nteste\n", true);
            configEditor.SaveLocalWords();
            configEditor.Close();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Empty(batchConfig.Proposals);
            Assert.Empty(PortugueseLocalWordList.Load(refreshPath).Words);
            Assert.Equal("teste", Assert.Single(PortugueseLocalWordList.Load(refreshPath).Ignored));
            batchConfig.Close();

            var rulesConfig = new MainWindow(new PluginRequest { RequestType = PluginRequestType.Configure }, refreshPath);
            rulesConfig.Show();
            rulesConfig.FindControl<Button>("GrammarRulesButton")!
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var rulesEditor = Assert.IsType<GrammarRulesWindow>(Assert.Single(rulesConfig.OwnedWindows));
            rulesEditor.FindControl<Button>("GrammarOnlyPreset")!
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            rulesEditor.FindControl<CheckBox>("GrammarRule_DASH_ENUMERATION_SPACE_RULE")!.IsChecked = false;
            rulesEditor.FindControl<Button>("SaveGrammarRules")!
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            rulesConfig.FindControl<Button>("ApplyButton")!
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            var rulesResponse = App.Response!;
            var storedRules = ConversionOptions.Load(new PluginRequest
            {
                Settings = rulesResponse.Settings, SettingsVersion = rulesResponse.SettingsVersion,
            });
            Assert.Equal("DASH_ENUMERATION_SPACE_RULE", storedRules.GrammarDisabledRules);
            Assert.Equal("other,spelling,style,typography", storedRules.GrammarDisabledGroups);
            var rulesReload = new GrammarRulesWindow(storedRules, Array.Empty<string>());
            Assert.False(rulesReload.FindControl<CheckBox>("GrammarGroup_style")!.IsChecked);
            Assert.False(rulesReload.FindControl<CheckBox>("GrammarRule_DASH_ENUMERATION_SPACE_RULE")!.IsChecked);
            rulesReload.FindControl<Button>("AllGrammarRules")!
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Assert.Empty(rulesReload.DisabledRules);
            Assert.Empty(rulesReload.DisabledGroups);
            rulesReload.Close();

            var namesConfig = new MainWindow(new PluginRequest { RequestType = PluginRequestType.Configure }, refreshPath);
            namesConfig.Show();
            namesConfig.FindControl<Button>("EditNameArticlesButton")!
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var namesEditor = Assert.IsType<NameArticlesWindow>(Assert.Single(namesConfig.OwnedWindows));
            namesEditor.Editor.Text = "Jamie = invalid";
            namesEditor.SaveButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Assert.Contains("article must be", namesEditor.Error.Text);
            namesEditor.Editor.Text = "Jamie = a";
            namesEditor.SaveButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            namesConfig.FindControl<Button>("ApplyButton")!
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            var namesResponse = App.Response!;
            var storedNames = ConversionOptions.Load(new PluginRequest
            {
                Settings = namesResponse.Settings,
                SettingsVersion = namesResponse.SettingsVersion,
            });
            Assert.Equal("Jamie = a", storedNames.NameArticles);
            var namesPreview = new MainWindow(new PluginRequest
            {
                Settings = namesResponse.Settings,
                SettingsVersion = namesResponse.SettingsVersion,
                Subtitle = new PluginSubtitle { Format = "SubRip", SubRip = "1\n00:00:01,000 --> 00:00:04,000\npensar em Jamie.\n" },
            }, refreshPath);
            Assert.Equal("pensar na Jamie.", Assert.Single(namesPreview.Proposals).ConvertedText);
            namesPreview.Show();
            namesPreview.FindControl<Button>("EditNameArticlesButton")!
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var previewNamesEditor = Assert.IsType<NameArticlesWindow>(Assert.Single(namesPreview.OwnedWindows));
            previewNamesEditor.Editor.Text = "Jamie = o";
            previewNamesEditor.SaveButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Equal("pensar no Jamie.", Assert.Single(namesPreview.Proposals).ConvertedText);
            namesPreview.Close();

            var automaticNames = new MainWindow(new PluginRequest
            {
                Subtitle = new PluginSubtitle { Format = "SubRip", SubRip =
                    "1\n00:00:01,000 --> 00:00:04,000\npensar em Jamie e em Alex.\n" },
            }, refreshPath);
            automaticNames.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var firstQuestion = Assert.IsType<NameArticleQuestionWindow>(Assert.Single(automaticNames.OwnedWindows));
            Assert.Equal("Jamie", firstQuestion.PersonalName);
            firstQuestion.MasculineButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var secondQuestion = Assert.IsType<NameArticleQuestionWindow>(Assert.Single(automaticNames.OwnedWindows));
            Assert.Equal("Alex", secondQuestion.PersonalName);
            secondQuestion.FeminineButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Empty(automaticNames.OwnedWindows);
            Assert.Equal("pensar no Jamie e na Alex.", Assert.Single(automaticNames.Proposals).ConvertedText);
            automaticNames.FindControl<Button>("ApplyButton")!
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Assert.Contains("Jamie = o", App.Response!.Settings!.Value.GetProperty("nameArticles").GetString());
            Assert.Contains("Alex = a", App.Response.Settings!.Value.GetProperty("nameArticles").GetString());

            var noArticleNames = new MainWindow(new PluginRequest
            {
                Subtitle = new PluginSubtitle { Format = "SubRip", SubRip =
                    "1\n00:00:01,000 --> 00:00:04,000\npensar em Jamie.\n" },
            }, refreshPath);
            noArticleNames.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var noArticleQuestion = Assert.IsType<NameArticleQuestionWindow>(Assert.Single(noArticleNames.OwnedWindows));
            noArticleQuestion.NoArticleButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Empty(noArticleNames.Proposals);
            Assert.True(noArticleNames.FindControl<Button>("ApplyButton")!.IsEnabled);
            noArticleNames.FindControl<Button>("ApplyButton")!
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            var noArticleResponse = App.Response!;
            Assert.Equal(PluginStatus.Ok, noArticleResponse.Status);
            Assert.Null(noArticleResponse.Subtitle);
            Assert.Equal("Jamie = none", noArticleResponse.Settings!.Value.GetProperty("nameArticles").GetString());

            var savedNoArticleNames = new MainWindow(new PluginRequest
            {
                Settings = noArticleResponse.Settings, SettingsVersion = noArticleResponse.SettingsVersion,
                Subtitle = new PluginSubtitle { Format = "SubRip", SubRip =
                    "1\n00:00:01,000 --> 00:00:04,000\npensar em Jamie.\n" },
            }, refreshPath);
            savedNoArticleNames.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Empty(savedNoArticleNames.OwnedWindows);
            savedNoArticleNames.Close();

            var skippedNames = new MainWindow(new PluginRequest
            {
                Subtitle = new PluginSubtitle { Format = "SubRip", SubRip =
                    "1\n00:00:01,000 --> 00:00:04,000\npensar em Jamie.\n" },
            }, refreshPath);
            skippedNames.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var skipQuestion = Assert.IsType<NameArticleQuestionWindow>(Assert.Single(skippedNames.OwnedWindows));
            skipQuestion.SkipButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            skippedNames.FindControl<CheckBox>("PronounsCheck")!.IsChecked = false;
            skippedNames.FindControl<CheckBox>("PronounsCheck")!.IsChecked = true;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Empty(skippedNames.OwnedWindows);
            skippedNames.Close();

            var optedOutNames = new MainWindow(new PluginRequest
            {
                SettingsVersion = ConversionOptions.Version,
                Settings = new ConversionOptions(AskNameArticles: false).ToJson(),
                Subtitle = new PluginSubtitle { Format = "SubRip", SubRip =
                    "1\n00:00:01,000 --> 00:00:04,000\npensar em Jamie.\n" },
            }, refreshPath);
            optedOutNames.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Empty(optedOutNames.OwnedWindows);
            optedOutNames.FindControl<CheckBox>("AskNameArticlesCheck")!.IsChecked = true;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.IsType<NameArticleQuestionWindow>(Assert.Single(optedOutNames.OwnedWindows));
            optedOutNames.Close();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            var telephoneEnglish = Path.Combine(data, "name-question.en.srt");
            File.WriteAllText(telephoneEnglish,
                "1\n00:00:01,000 --> 00:00:04,000\nShould I call Jamie?\n");
            var telephoneNames = new MainWindow(new PluginRequest
            {
                Subtitle = new PluginSubtitle
                {
                    Format = "SubRip", FileName = Path.Combine(data, "name-question.pt-br.srt"),
                    SubRip = "1\n00:00:01,000 --> 00:00:04,000\nLigue para Jamie?\n",
                },
            }, refreshPath);
            telephoneNames.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var telephoneQuestion = Assert.IsType<NameArticleQuestionWindow>(Assert.Single(telephoneNames.OwnedWindows));
            Assert.Equal("Jamie", telephoneQuestion.PersonalName);
            telephoneQuestion.MasculineButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.Equal("Ligo para o Jamie?", Assert.Single(telephoneNames.Proposals).ConvertedText);
            telephoneNames.Close();

            var screenshotWindow = new MainWindow(new PluginRequest
            {
                SettingsVersion = ConversionOptions.Version,
                Settings = new ConversionOptions(PostAgreement: false, ValidateSpelling: true).ToJson(),
                Subtitle = new PluginSubtitle
                {
                    Format = "SubRip",
                    SubRip = "1\n00:00:01,000 --> 00:00:04,000\nVocê não lembra de mim,\nmas vou te contar uma parada\n",
                },
            });
            Assert.Equal("Não te lembras de mim,\nmas vou contar-te uma coisa",
                Assert.Single(screenshotWindow.Proposals).ConvertedText);
            screenshotWindow.FindControl<CheckBox>("VocabularyCheck")!.IsChecked = false;
            Assert.Contains("uma parada", Assert.Single(screenshotWindow.Proposals).ConvertedText);
            screenshotWindow.FindControl<CheckBox>("VocabularyCheck")!.IsChecked = true;
            Assert.Contains("uma coisa", Assert.Single(screenshotWindow.Proposals).ConvertedText);
            screenshotWindow.FindControl<CheckBox>("PronounsCheck")!.IsChecked = false;
            Assert.DoesNotContain("contar-te", Assert.Single(screenshotWindow.Proposals).ConvertedText);
            screenshotWindow.FindControl<CheckBox>("PronounsCheck")!.IsChecked = true;
            Assert.Contains("contar-te uma coisa", Assert.Single(screenshotWindow.Proposals).ConvertedText);
            var settingsPanel = screenshotWindow.FindControl<StackPanel>("SettingsPanel")!;
            var toggleSettings = screenshotWindow.FindControl<Button>("SettingsToggleButton")!;
            var beforeCollapse = Assert.Single(screenshotWindow.Proposals);
            beforeCollapse.ConvertedText += " [manual edit]";
            beforeCollapse.Include = false;
            screenshotWindow.FindControl<Expander>("ReferencePanel")!.IsExpanded = true;
            screenshotWindow.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var screenshotContent = Assert.IsAssignableFrom<Control>(screenshotWindow.Content);
            void LayoutScreenshot()
            {
                screenshotContent.Measure(new Avalonia.Size(1080, 720));
                screenshotContent.Arrange(new Avalonia.Rect(0, 0, 1080, 720));
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }
            LayoutScreenshot();
            var panels = screenshotWindow.FindControl<Grid>("ReviewLayoutPanels")!;
            var layoutPanel = screenshotWindow.FindControl<Expander>("LayoutTools")!;
            var referencePanel = screenshotWindow.FindControl<Expander>("ReferencePanel")!;
            Assert.Same(panels, layoutPanel.Parent);
            Assert.Same(panels, referencePanel.Parent);
            Assert.Equal(1, Grid.GetColumn(layoutPanel));
            Assert.Equal(2, Grid.GetColumn(referencePanel));
            Assert.True(referencePanel.Bounds.X >= layoutPanel.Bounds.Right);
            Assert.True(referencePanel.Bounds.Width > 200);
            Assert.True(referencePanel.Bounds.Right <= panels.Bounds.Width + 0.1);
            if (Environment.GetEnvironmentVariable("PTPT_UI_LAYOUT_CAPTURE") is { Length: > 0 } capture)
            {
                layoutPanel.IsExpanded = true;
                LayoutScreenshot();
                using var bitmap = new Avalonia.Media.Imaging.RenderTargetBitmap(
                    new Avalonia.PixelSize((int)screenshotContent.Bounds.Width, (int)screenshotContent.Bounds.Height));
                bitmap.Render(screenshotContent);
                bitmap.Save(capture);
            }
            var settingsHeight = settingsPanel.Bounds.Height;
            var reviewHeight = screenshotWindow.FindControl<Border>("ChangesBorder")!.Bounds.Height;
            toggleSettings.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            LayoutScreenshot();
            Assert.False(settingsPanel.IsVisible);
            Assert.True(settingsHeight > 0);
            Assert.Equal(0, settingsPanel.DesiredSize.Height);
            Assert.True(screenshotWindow.FindControl<Border>("ChangesBorder")!.Bounds.Height > reviewHeight + 100);
            Assert.Equal("Expand settings", Avalonia.Automation.AutomationProperties.GetName(toggleSettings));
            Assert.Same(beforeCollapse, Assert.Single(screenshotWindow.Proposals));
            toggleSettings.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            LayoutScreenshot();
            Assert.True(settingsPanel.IsVisible);
            Assert.True(screenshotWindow.FindControl<Expander>("ReferencePanel")!.IsExpanded);
            Assert.Equal("Collapse settings", Avalonia.Automation.AutomationProperties.GetName(toggleSettings));
            Assert.Equal(reviewHeight, screenshotWindow.FindControl<Border>("ChangesBorder")!.Bounds.Height);
            Assert.EndsWith(" [manual edit]", Assert.Single(screenshotWindow.Proposals).ConvertedText);
            Assert.False(Assert.Single(screenshotWindow.Proposals).Include);
            screenshotWindow.Close();
            var window = new MainWindow(new PluginRequest
            {
                Subtitle = new PluginSubtitle
                {
                    Format = "SubRip",
                    FileName = subtitle,
                    SubRip = "1\n00:00:01,000 --> 00:00:04,000\nJá temos digitais e sua foto.\n\n2\n00:00:05,000 --> 00:00:07,000\nVocê está bem?\n\n" +
                             "3\n00:00:08,000 --> 00:00:10,000\nPrecisamos falar com seu marido. Ele está em casa?\n\n" +
                             "4\n00:00:11,000 --> 00:00:13,000\nNão vou fala mais com vocês.\n",
                },
            });
            Assert.Contains("a tua foto", window.Proposals[0].ConvertedText);
            Assert.Contains("MiniLM is not installed", window.ReferenceStatusText);
            Assert.Equal("Model: not downloaded\nllama-server: not downloaded\nGervásio: not downloaded\nLanguageTool pt-PT: not downloaded\nPortable Java: not downloaded", window.AssetStatusText);
            window.FindReferenceAutomatically().GetAwaiter().GetResult();
            Assert.Equal(4, window.Alignment!.Matched);
            Assert.Contains("a sua foto", window.Proposals[0].ConvertedText);
            Assert.Equal("EN: So we got prints and his picture now.", window.Proposals[0].ReferenceText);
            Assert.Equal("EN: Are you okay?", window.Proposals[1].ReferenceText);
            var review = window.Proposals.Single(p => p.LineIndex == 2);
            Assert.True(review.IsChange);
            Assert.Contains("o seu marido", review.ConvertedText);
            Assert.Contains("keeps seu/sua", review.ReviewNote);
            Assert.Equal("Não vou falar mais com vocês.", window.Proposals.Single(p => p.LineIndex == 3).ConvertedText);
            Assert.StartsWith("Reference matched 4 of 4", window.ReferenceStatusText);
            Assert.Contains("1 line(s) marked for review", window.ReferenceStatusText);
            var formalEnglish = Path.Combine(data, "formal.en.srt");
            File.WriteAllText(formalEnglish, "1\n00:00:01,000 --> 00:00:02,000\nOkay, look--\n\n" +
                                           "2\n00:00:03,000 --> 00:00:04,000\nI don't know.\n\n" +
                                           "3\n00:00:05,000 --> 00:00:06,000\nI disagree, Detective.\n");
            var formalWindow = new MainWindow(new PluginRequest
            {
                Subtitle = new PluginSubtitle
                {
                    Format = "SubRip",
                    FileName = Path.Combine(data, "formal.pt-br.srt"),
                    SubRip = "1\n00:00:01,000 --> 00:00:02,000\nCerto, veja...\n\n" +
                             "2\n00:00:03,000 --> 00:00:04,000\nNão sei.\n\n" +
                             "3\n00:00:05,000 --> 00:00:06,000\nNão concordo.\n",
                },
            });
            Assert.Equal("Certo, vê...", formalWindow.Proposals.Single(p => p.LineIndex == 0).ConvertedText);
            formalWindow.FindReferenceAutomatically().GetAwaiter().GetResult();
            Assert.Equal(3, formalWindow.Alignment!.Matched);
            Assert.DoesNotContain(formalWindow.Proposals, p => p.LineIndex == 0 && p.IsChange);

            var qualityOptions = new ConversionOptions(SentenceContext: true, Rewrap: true, LineLength: 30, YouToTu: false,
                UseReference: false);
            var qualityWindow = new MainWindow(new PluginRequest
            {
                Settings = qualityOptions.ToJson(), SettingsVersion = ConversionOptions.Version,
                Subtitle = new PluginSubtitle
                {
                    Format = "SubRip",
                    SubRip = "1\n00:00:01,000 --> 00:00:03,000\nEstou no trem e não vou\n\n" +
                             "2\n00:00:03,100 --> 00:00:05,000\nfala mais.\n\n" +
                             "3\n00:00:08,000 --> 00:00:12,000\nVou falar com o diretor amanhã de manhã.\n",
                },
            });
            var first = qualityWindow.Proposals.Single(p => p.LineIndex == 0);
            var second = qualityWindow.Proposals.Single(p => p.LineIndex == 1);
            Assert.Equal("falar mais.", second.ConvertedText);
            first.Include = false;
            Assert.False(second.Include);
            second.Include = true;
            Assert.True(first.Include);
            var layout = qualityWindow.Proposals.Single(p => p.LineIndex == 2);
            Assert.True(layout.HasLayoutChange);
            layout.IncludeLayout = false;
            Assert.Equal("Vou falar com o diretor amanhã de manhã.", layout.ConvertedText);
            var choice = qualityWindow.FindControl<Avalonia.Controls.ComboBox>("DisableRuleChoice")!;
            choice.SelectedItem = "word-list";
            qualityWindow.FindControl<Avalonia.Controls.Button>("DisableRuleButton")!
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
            Assert.Contains("trem", qualityWindow.Proposals.Single(p => p.LineIndex == 0).ConvertedText);
            Assert.DoesNotContain(qualityWindow.Proposals.SelectMany(p => p.Rules), r => r.Id == "word-list");
            qualityWindow.FindControl<Avalonia.Controls.Button>("RestoreRulesButton")!
                .RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
            Assert.Contains("comboio", qualityWindow.Proposals.Single(p => p.LineIndex == 0).ConvertedText);

            var styleWindow = new MainWindow(new PluginRequest
            {
                SettingsVersion = ConversionOptions.Version,
                Settings = new ConversionOptions(UseReference: false).ToJson(),
                Subtitle = new PluginSubtitle
                {
                    Format = "SubRip",
                    SubRip = "1\n00:00:01,000 --> 00:00:04,000\nEu estou pronto.\n",
                },
            });
            var style = styleWindow.FindControl<Avalonia.Controls.CheckBox>("ConversationalCheck")!;
            Assert.False(style.IsChecked == true);
            Assert.Empty(styleWindow.Proposals);
            style.IsChecked = true;
            Assert.Equal("Estou pronto.", Assert.Single(styleWindow.Proposals).ConvertedText);
            styleWindow.FindControl<Expander>("ReviewTools")!.IsExpanded = true;
            styleWindow.FindControl<Expander>("LayoutTools")!.IsExpanded = true;
            styleWindow.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            foreach (var width in new[] { styleWindow.MinWidth, 1120d })
            {
                styleWindow.Width = width;
                var content = Assert.IsAssignableFrom<Control>(styleWindow.Content);
                content.Measure(new Avalonia.Size(width - 40, 720));
                content.Arrange(new Avalonia.Rect(0, 0, width - 40, 720));
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
                var reviewTools = styleWindow.FindControl<Expander>("ReviewTools")!;
                var layoutTools = styleWindow.FindControl<Expander>("LayoutTools")!;
                Assert.Same(reviewTools.Parent, layoutTools.Parent);
                Assert.True(layoutTools.Bounds.Left >= reviewTools.Bounds.Right);
                foreach (var name in new[] { "LineLength", "MaxLines", "CharactersPerSecond" })
                {
                    var numeric = styleWindow.FindControl<NumericUpDown>(name)!;
                    var input = Assert.Single(numeric.GetVisualDescendants().OfType<TextBox>());
                    Assert.True(input.Bounds.Width >= 40, $"{name}: numeric input has only {input.Bounds.Width} pixels.");
                    Assert.False(string.IsNullOrWhiteSpace(input.Text));
                }
                var rules = Assert.Single(styleWindow.GetVisualDescendants().OfType<Expander>(),
                    expander => Equals(expander.Header, "Rules and regression export"));
                var row = Assert.IsType<Grid>(rules.Parent);
                var conversion = Assert.Single(row.Children.OfType<StackPanel>());
                Assert.True(rules.Bounds.Left >= conversion.Bounds.Right);
            }
            styleWindow.Close();
            style.IsChecked = false;
            Assert.Empty(styleWindow.Proposals);
            var configureWindow = new MainWindow(new PluginRequest { RequestType = PluginRequestType.Configure });
            Assert.False(configureWindow.FindControl<Button>("SettingsToggleButton")!.IsVisible);
            Assert.False(configureWindow.FindControl<Expander>("ReviewTools")!.IsVisible);
            Assert.Equal(0, configureWindow.FindControl<Grid>("ReviewLayoutPanels")!.ColumnDefinitions[0].Width.Value);
            var physicalWindow = new MainWindow(new PluginRequest
            {
                SettingsVersion = ConversionOptions.Version,
                Settings = new ConversionOptions(UseReference: false).ToJson(),
                Subtitle = new PluginSubtitle
                {
                    Format = "SubRip",
                    SubRip = "1\n00:00:01,000 --> 00:00:04,000\nSuspeito de cerca de 1,80m, porte médio.\n",
                },
            });
            physicalWindow.ApplyCheckResults(new[]
            {
                new CheckLine(0, "Suspeito de cerca de 1,80m, porte médio.",
                    "Suspeito com cerca de 1,80m, porte médio.", null, null),
            }, new Dictionary<int, CheckResult>
            {
                [0] = new("Suspeito com cerca de 1,80m, porte médio.", "MiniLM: check the translation."),
            });
            var physicalReview = Assert.Single(physicalWindow.Proposals);
            Assert.Equal("Suspeito com cerca de 1,80m, porte médio.", physicalReview.ConvertedText);
            Assert.Contains(physicalReview.Rules, r => r.Id == "physical-descriptions");
            Assert.Contains("MiniLM:", physicalReview.ReviewNote);
            physicalWindow.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var physicalCheckbox = Assert.Single(physicalWindow.FindControl<ItemsControl>("ChangesList")!
                .GetVisualDescendants().OfType<CheckBox>(),
                c => Avalonia.Automation.AutomationProperties.GetName(c) == "Include text change");
            Assert.True(physicalCheckbox.IsVisible);
            Assert.True(physicalCheckbox.IsEnabled);
            Assert.True(physicalCheckbox.IsChecked);
            physicalCheckbox.IsChecked = false;
            Assert.False(physicalReview.Include);
            Assert.False(physicalWindow.FindControl<Button>("ApplyButton")!.IsEnabled);
            physicalCheckbox.IsChecked = true;
            Assert.True(physicalReview.Include);
            Assert.True(physicalWindow.FindControl<Button>("ApplyButton")!.IsEnabled);
            physicalWindow.Close();
            var warningsWindow = new MainWindow(new PluginRequest
            {
                SettingsVersion = ConversionOptions.Version,
                Settings = new ConversionOptions(UseReference: false).ToJson(),
                Subtitle = new PluginSubtitle
                {
                    Format = "SubRip",
                    SubRip = "1\n00:00:01,000 --> 00:00:02,000\nEsta linha é longa e já está correta.\n",
                },
            });
            var warnings = warningsWindow.FindControl<CheckBox>("LayoutWarningsCheck")!;
            Assert.False(warnings.IsChecked == true);
            Assert.Empty(warningsWindow.Proposals);
            warnings.IsChecked = true;
            var layoutReview = Assert.Single(warningsWindow.Proposals);
            Assert.False(layoutReview.IsChange);
            Assert.Contains("Layout:", layoutReview.ReviewNote);
            warningsWindow.Show();
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            var warningRows = warningsWindow.FindControl<ItemsControl>("ChangesList")!;
            var warningCheckbox = Assert.Single(warningRows.GetVisualDescendants().OfType<CheckBox>(),
                c => Avalonia.Automation.AutomationProperties.GetName(c) == "Include text change");
            var originalLabel = Assert.Single(warningRows.GetVisualDescendants().OfType<TextBlock>(),
                t => t.Text == "PT-BR: " + layoutReview.OriginalText);
            Assert.True(originalLabel.IsVisible);
            Assert.True(warningCheckbox.IsVisible);
            Assert.False(warningCheckbox.IsEnabled);
            Assert.False(warningCheckbox.IsChecked);
            Assert.Contains("text is unchanged", ToolTip.GetTip(warningCheckbox)!.ToString());
            var warningEditor = warningRows.GetVisualDescendants().OfType<TextBox>()
                .First(t => t.Text == layoutReview.ConvertedText);
            warningEditor.Text = "Esta linha foi revista.";
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.True(warningCheckbox.IsEnabled);
            Assert.True(warningCheckbox.IsChecked);
            Assert.Equal("Esta linha foi revista.", layoutReview.ConvertedText);
            warningCheckbox.IsChecked = false;
            Assert.False(layoutReview.Include);
            Assert.False(warningsWindow.FindControl<Button>("ApplyButton")!.IsEnabled);
            warningCheckbox.IsChecked = true;
            Assert.True(layoutReview.Include);
            Assert.True(warningsWindow.FindControl<Button>("ApplyButton")!.IsEnabled);
            warningEditor.Text = layoutReview.OriginalText;
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            Assert.False(warningCheckbox.IsEnabled);
            Assert.False(warningCheckbox.IsChecked);
            Assert.True(originalLabel.IsVisible);
            warningsWindow.Close();
            warnings.IsChecked = false;
            Assert.Empty(warningsWindow.Proposals);
        }
        finally
        {
            Environment.SetEnvironmentVariable("XDG_DATA_HOME", null);
            Directory.Delete(data, true);
        }
    }

    [Theory]
    [InlineData("Show.S01E01.pt-br.srt", "Show.S01E01.en.srt")]
    [InlineData("Show.S01E01.pt-br.srt", "Show.S01E01.English.srt")]
    [InlineData("Show S01E01 Title.Portuguese.srt", "Show S01E01 Title.eng.srt")]
    [InlineData("Movie.2010.srt", "Movie.2010.en.sdh.srt")]
    [InlineData("Movie.2010.pt.srt", "Movie_2010.[eng].srt")]
    [InlineData("Some.Show.S01E01.Pilot.720p.WEB.DL.DD5.1.H.264-GRP.pt-br.srt", "Some Show.S01E01.Pilot.English.srt")]
    public void FindsEnglishReferenceNextToSubtitle(string subtitle, string english)
    {
        using var folder = new TempFolder(subtitle, english, "Other.S01E02.en.srt", "Show.S01E01.pt-pt.srt");
        Assert.Equal(folder.Path(english), ReferenceFinder.Find(folder.Path(subtitle), null));
    }

    [Fact]
    public void FindsReferenceNamedAfterTheVideo()
    {
        using var folder = new TempFolder("Movie.mkv", "Movie.en.srt", "legendas.srt");
        Assert.Equal(folder.Path("Movie.en.srt"), ReferenceFinder.Find(folder.Path("legendas.srt"), folder.Path("Movie.mkv")));
    }

    [Theory]
    [InlineData("Show.S01E01.pt-br.srt", "Show.S01E01.pt-pt.srt", "Show.S01E01.srt")]
    [InlineData("Show.S01E01.720p.pt-br.srt", "Show.S01E01.Pilot.en.srt", "Show.S01E01.Pilot.English.forced.srt")]
    [InlineData("Show.S01E01.pt-br.srt", "Show.S01E02.en.srt", "Show2.S01E01.en.srt")]
    public void NoReferenceWithoutAnUnambiguousEnglishMatch(string subtitle, string other, string third)
    {
        using var folder = new TempFolder(subtitle, other, third);
        Assert.Null(ReferenceFinder.Find(folder.Path(subtitle), null));
        Assert.Null(ReferenceFinder.Find(null, null));
        Assert.Null(ReferenceFinder.Find(folder.Path("missing/Show.S01E01.pt-br.srt"), null));
    }

    [Theory]
    [InlineData("Você apaixona-se por um advogado", "You fall for a Harvard lawyer", "keeps você")]
    [InlineData("Ouvi dizer que o seu miúdo é o detetive chefe?", "I hear your boy is lead detective?", "keeps seu/sua")]
    [InlineData("Você viu a sua mãe?", "Did you see your mom?", "keeps você and seu/sua")]
    public void ReviewNotesForYouKeptAsVoceOrSeu(string text, string english, string expected)
    {
        var note = ReferenceReview.Note(text, ReferenceHint.FromText(english));
        Assert.Contains(expected, note);
    }

    [Theory]
    [InlineData("O Estado solicita mais tempo, Sua Excelência.", "The State pleads for more time, Your Honor.")]
    [InlineData("O senhor viu a sua mãe?", "Did you see your mom?")]
    [InlineData("Ele viu a sua mãe?", "Did he see his mom?")]
    [InlineData("Vocês vêm?", "Are you coming?")]
    [InlineData("Viste a tua mãe?", "Did you see your mom?")]
    [InlineData("Ele viu a sua mãe.", "He saw your mom and his dad.")]
    public void NoReviewForFormalThirdPersonOrAlreadyTu(string text, string english) =>
        Assert.Null(ReferenceReview.Note(text, ReferenceHint.FromText(english)));

    [Theory]
    [InlineData("Arrisquei o pescoço para conseguir prender esse tipo.", "I busted my hump trying to lock that guy up.", "dar cabo das costas")]
    [InlineData("Pare de quebrar minhas bolas.", "Stop busting my chops.", "chatear")]
    [InlineData("Isso é um pedaço de bolo.", "That's a piece of cake.", "canja")]
    [InlineData("Ele chutou o balde.", "He kicked the bucket.", "bater a bota")]
    [InlineData("Está a puxar a minha perna?", "Are you pulling my leg?", "a gozar")]
    public void IdiomHintsFromTheEnglish(string text, string english, string expected)
    {
        var note = IdiomHints.Note(text, english);
        Assert.NotNull(note);
        Assert.StartsWith("Hint:", note);
        Assert.Contains(expected, note);
    }

    [Theory]
    [InlineData("Dei cabo das costas para prender aquele tipo.", "I busted my hump trying to lock that guy up.")]
    [InlineData("Isso é canja.", "That's a piece of cake.")]
    [InlineData("Estás a gozar comigo?", "Are you pulling my leg?")]
    [InlineData("Quebrar o gelo.", "Let's break the ice.")]
    [InlineData("Vamos prendê-lo.", "Let's lock him up.")]
    [InlineData("Aposto que sim.", "You bet on the horse?")]
    [InlineData("Qualquer coisa.", null)]
    [InlineData("Sem acordo.", "If this goes down, there's no deal for you.")]
    public void NoIdiomHintWhenTranslatedOrNoIdiom(string text, string? english) =>
        Assert.Null(IdiomHints.Note(text, english));

    [Fact]
    public void ReviewOnlyLinesCannotBeSelected()
    {
        var review = new ChangeProposal(3, "Você primeiro.", "Você primeiro.", "You first.", "Review");
        Assert.False(review.IsChange);
        Assert.False(review.Include);
        review.Include = true;
        Assert.False(review.Include);
        Assert.True(new ChangeProposal(0, "a", "b").Include);
    }

    [Fact]
    public void SettingsNeverStoreOrUseFilesOutsideThePluginFolder()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new EmbeddingSettings("/x/llama-server", "/x/m.gguf", true, true, "Vulkan1"));
        Assert.DoesNotContain("/x/", json);
        var loaded = System.Text.Json.JsonSerializer.Deserialize<EmbeddingSettings>(
            "{\"RuntimePath\":\"/opt/llama/llama-server\",\"UseEmbeddings\":true,\"Vulkan\":true,\"Device\":\"Vulkan1\"}")!;
        Assert.Equal((true, true, "Vulkan1", ""), (loaded.UseEmbeddings, loaded.Vulkan, loaded.Device, loaded.RuntimePath));
        var resolved = EmbeddingAssets.WithInstalledFiles(new EmbeddingSettings("/elsewhere/llama-server", "/elsewhere/m.gguf"));
        Assert.DoesNotContain("/elsewhere/", resolved.RuntimePath + resolved.ModelPath);
    }

    private sealed class TempFolder : IDisposable
    {
        private readonly string _root = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ptpt-find-" + Guid.NewGuid().ToString("N"));

        public TempFolder(params string[] files)
        {
            Directory.CreateDirectory(_root);
            foreach (var file in files) File.WriteAllText(Path(file), "");
        }

        public string Path(string name) => System.IO.Path.Combine(_root, name);
        public void Dispose() => Directory.Delete(_root, true);
    }
}

public sealed class FamilyPossessiveArticleTests
{
    private static readonly PortugueseConverter Converter = new(ConversionOptions.Default);

    [Theory]
    [InlineData("Entendemos que sua filha\ndesapareceu.", "Percebemos que a sua filha\ndesapareceu.")]
    [InlineData("Ouvi dizer que seu filho é detetive.", "Ouvi dizer que o seu filho é detetive.")]
    [InlineData("Disse que sua mãe ligou.", "Disse que a tua mãe ligou.")]
    [InlineData("Espero que seu irmão venha.", "Espero que o teu irmão venha.")]
    [InlineData("Antes que sua mãe chegue.", "Antes que a tua mãe chegue.")]
    [InlineData("Porque meu irmão disse.", "Porque o meu irmão disse.")]
    [InlineData("Até que meu pai\nvoltou.", "Até que o meu pai\nvoltou.")]
    public void ArticleBeforeFamilyNounAfterConjunction(string text, string expected) =>
        Assert.Equal(expected, Converter.Convert(text));

    [Theory]
    [InlineData("Mas meu filho, não podes.")]
    [InlineData("E meu amor!")]
    [InlineData("Sei que, minha filha, é difícil.")]
    [InlineData("E meu querido.")]
    public void AddressIsNotGivenAnArticle(string text) =>
        Assert.Equal(text, Converter.Convert(text));

    [Fact]
    public void ThirdPersonCueKeepsSuaWithArticle() =>
        Assert.Equal("Percebemos que a sua filha\ndesapareceu.",
            Converter.Convert("Entendemos que sua filha\ndesapareceu.", null,
                ReferenceHint.FromText("We understand your daughter went missing.")));
}

public sealed class YoungPersonTests
{
    private static readonly PortugueseConverter Converter = new(ConversionOptions.Default);

    [Theory]
    [InlineData("Ouvi dizer que o seu garoto\né o detetive chefe?", "I hear your boy is lead detective?", "Ouvi dizer que o seu rapaz\né o detetive chefe?")]
    [InlineData("O garoto é bom.", "The boy is good.", "O rapaz é bom.")]
    [InlineData("Esses garotos são polícias.", "Those guys are cops.", "Esses rapazes são polícias.")]
    [InlineData("A garota da loja ligou.", "The girl from the shop called.", "A rapariga da loja ligou.")]
    [InlineData("O garoto tem cinco anos.", "The kid is five.", "O miúdo tem cinco anos.")]
    [InlineData("O garoto chorou.", "The little boy cried.", "O rapaz chorou.")]
    [InlineData("O garoto chorou.", "He cried.", "O miúdo chorou.")]
    public void EnglishReferenceChoosesRapazOrMiudo(string text, string english, string expected) =>
        Assert.Equal(expected, Converter.Convert(text, null, ReferenceHint.FromText(english)));

    [Theory]
    [InlineData("Ouvi dizer que o seu garoto\né o detetive chefe?", "Ouvi dizer que o seu rapaz\né o detetive chefe?")]
    [InlineData("Meu garoto!", "O meu rapaz!")]
    [InlineData("O garoto chorou.", "O miúdo chorou.")]
    [InlineData("A minha garota.", "A minha miúda.")]
    [InlineData("Os garotos brincam.", "Os miúdos brincam.")]
    public void WithoutReferencePossessiveMakesGarotoRapaz(string text, string expected) =>
        Assert.Equal(expected, Converter.Convert(text));

    [Fact]
    public void LocalWordListStillWins()
    {
        var list = new PortugueseLocalWordList();
        list.Words.Add(("garoto", "puto"));
        list.Ignored.Add("garota");
        var converter = new PortugueseConverter(ConversionOptions.Default, list);
        var hint = ReferenceHint.FromText("The boy and the girl.");
        Assert.Equal("O puto e a garota.", converter.Convert("O garoto e a garota.", null, hint));
    }
}

public sealed class FutureSubjunctiveCliticTests
{
    private static readonly PortugueseConverter Converter = new(ConversionOptions.Default);

    [Theory]
    [InlineData("Tudo termina mais rápido\nse você pegá-lo.", "Tudo termina mais rápido\nse o apanhares.")]
    [InlineData("Se você vê-la, me liga.", "Se a vires, liga-me.")]
    [InlineData("Se o senhor vê-la, avise.", "Se o senhor a vir, avise.")]
    [InlineData("Quando ele fazê-lo, avise.", "Quando ele o fizer, avisa.")]
    [InlineData("Assim que vocês encontrá-los, voltem.", "Assim que vocês os encontrarem, voltem.")]
    [InlineData("Se pegá-lo, ligue.", "Se o apanhares, liga.")]
    [InlineData("Mas se dizê-lo, acabou.", "Mas se o disser, acabou.")]
    [InlineData("Não sei se fazê-lo.", "Não sei se fazê-lo.")]
    [InlineData("Sei quando apanhá-lo.", "Sei quando apanhá-lo.")]
    [InlineData("Como encontrá-lo?", "Como encontrá-lo?")]
    public void PronounMovesBeforeFutureSubjunctive(string text, string expected) =>
        Assert.Equal(expected, Converter.Convert(text, null, ReferenceHint.FromText("If you see him.")));
}

public sealed class NearDemonstrativeTests
{
    private static readonly PortugueseConverter Converter = new(ConversionOptions.Default);

    [Theory]
    [InlineData("- Amém.\n- Eu cuido disso.", "Amen. / I got this one.", "- Amém.\n- Eu cuido disto.")]
    [InlineData("Olhe essa foto.", "Look at this picture.", "Olha esta foto.")]
    [InlineData("Nesses casos, ligue.", "In these cases, call.", "Nestes casos, ligue.")]
    [InlineData("Eu cuido disso.", "I'll take care of that.", "Eu cuido disso.")]
    [InlineData("Eu cuido disso.", "I'll handle it.", "Eu cuido disso.")]
    [InlineData("Por isso essa foto.", "That's why this picture.", "Por isso essa foto.")]
    [InlineData("Por isso vim.", "This is why I came.", "Por isso vim.")]
    [InlineData("Se ele desse isso...", "If he gave this...", "Se ele desse isto...")]
    [InlineData("-Pode ser essa?\n-Claro.", "- This one? / - Sure.", "-Pode ser esta?\n-Claro.")]
    public void EnglishThisMakesIsto(string text, string english, string expected) =>
        Assert.Equal(expected, Converter.Convert(text, null, ReferenceHint.FromText(english)));

    [Fact]
    public void WithoutReferenceIssoStays() => Assert.Equal("Eu cuido disso.", Converter.Convert("Eu cuido disso."));
}

public sealed class ImpersonalQuestionTests
{
    private static readonly PortugueseConverter Converter = new(ConversionOptions.Default);

    [Theory]
    [InlineData("Pode ser essa?", "Pode ser essa?")]
    [InlineData("Pode haver mais?", "Pode haver mais?")]
    [InlineData("Pode me ajudar?", "Podes ajudar-me?")]
    public void PodeSerHasNoYou(string text, string expected) => Assert.Equal(expected, Converter.Convert(text));
}

public sealed class EmContractionTests
{
    private static readonly PortugueseConverter Converter = new(ConversionOptions.Default);

    [Theory]
    [InlineData("se eu fizer com que Becky testemunhe\nem um novo julgamento.", "se eu fizer com que Becky testemunhe\nnum novo julgamento.")]
    [InlineData("Em um avião para Scottsdale.", "Num avião para Scottsdale.")]
    [InlineData("Ela confiou em uma amostra.", "Ela confiou numa amostra.")]
    [InlineData("O livro está em outro lugar.", "O livro está noutro lugar.")]
    [InlineData("Pensa em outra pessoa.", "Pensa noutra pessoa.")]
    [InlineData("Viste em algum sítio?", "Viste nalgum sítio?")]
    [InlineData("Em umas horas.", "Numas horas.")]
    [InlineData("ESTÁ EM UM CARRO.", "ESTÁ NUM CARRO.")]
    [InlineData("Isto transformou-se em\num barril de pólvora.", "Isto transformou-se\nnum barril de pólvora.")]
    [InlineData("Ele tem um carro.", "Ele tem um carro.")]
    [InlineData("Item um.", "Item um.")]
    public void EmContractsWithIndefinites(string text, string expected) => Assert.Equal(expected, Converter.Convert(text));
}
