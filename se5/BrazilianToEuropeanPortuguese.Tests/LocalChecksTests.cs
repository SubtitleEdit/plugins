using System.Text.Json;
using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;
using Xunit;
using Xunit.Abstractions;

namespace BrazilianToEuropeanPortuguese.Tests;

// ReferenceTests changes XDG_DATA_HOME for the process; do not run beside it.
[Collection("XdgDataHome")]
public sealed class LocalChecksTests(ITestOutputHelper output)
{
    private static string[] Texts(string original, string converted) =>
        JudgeAlternatives.Generate(original, converted).Select(c => c.Text).ToArray();

    [Fact]
    public void PluralSubjectOffersTem()
    {
        Assert.Contains("Os adultos têm muitas coisas.", Texts("Os adultos tem muitas coisas.", "Os adultos tem muitas coisas."));
    }

    [Theory]
    [InlineData("Qual é, tem de nos dar algo mais.")]
    [InlineData("Mas tem de vir.")]
    [InlineData("Ele tem razão. Eles sabem.")]
    public void NoPluralSubjectOffersNoTem(string text) => Assert.DoesNotContain(Texts(text, text), t => t.Contains("têm"));

    [Fact]
    public void PluralVerbAlwaysOffersSingular()
    {
        Assert.Contains("Dar permissão para abrir os armários tem implicações.",
            Texts("Dar permissão para abrir os armários tem implicações.", "Dar permissão para abrir os armários têm implicações."));
    }

    [Fact]
    public void RapazOnlyForGarotoAndKeepsCase()
    {
        Assert.Contains("Rapaz, que susto.", Texts("Garoto, que susto.", "Miúdo, que susto."));
        Assert.Empty(Texts("Criança, que susto.", "Miúdo, que susto."));
    }

    [Fact]
    public void TiraOffersPoliciaAndMauOffersMa()
    {
        Assert.Contains("Ele é um polícia.", Texts("Ele é um tira.", "Ele é um tira."));
        Assert.Contains("Tem coisas realmente más.", Texts("Tem coisas realmente ruins.", "Tem coisas realmente maus."));
    }

    [Fact]
    public void ScoringTextHasNoTagsOrLineBreaks() =>
        Assert.Equal("Olá, Ann. Tudo bem?", LocalChecks.ForScoring("<i>Olá, Ann.</i>\r\n{\\an8}Tudo bem?"));

    [Fact]
    public async Task IdiomMeaningChecksOriginalAndConvertedAgainstPlainEnglish()
    {
        var seen = new List<string?>();
        Task<float[]?> Embed(string? text)
        {
            seen.Add(text);
            return Task.FromResult<float[]?>(text == "It's a very easy task."
                ? new[] { 1f, 0f } : text == "É canja." ? new[] { .8f, .6f } : new[] { .2f, .9797959f });
        }
        var line = new CheckLine(0, "É um pedaço de bolo.", "É canja.", null, "It's a piece of cake.");
        var note = await LocalChecks.IdiomMeaningNoteAsync(line, line.Converted, Embed);
        Assert.Contains("possible English idiom", note);
        Assert.Contains("before the rules", note);
        Assert.Contains("after", note);
        Assert.Equal(new[] { "It's a very easy task.", line.Original, line.Converted }, seen);
    }

    [Theory]
    [InlineData(.8f, .8f, false)]
    [InlineData(.2f, .2f, true)]
    [InlineData(.8f, .2f, true)]
    [InlineData(.8f, .75f, true)]
    [InlineData(.8f, .78f, false)]
    [InlineData(.4501f, .4501f, false)]
    [InlineData(.4499f, .4499f, true)]
    [InlineData(.5999f, .54f, false)]
    [InlineData(.6001f, .54f, true)]
    [InlineData(.8f, .7601f, false)]
    [InlineData(.8f, .7599f, true)]
    [InlineData(.56f, .4601f, false)]
    [InlineData(.56f, .4599f, true)]
    public async Task IdiomMeaningUsesExistingThresholds(float before, float after, bool flagged)
    {
        var line = new CheckLine(0, "Original.", "Converted.", null, "I'm skint.");
        Task<float[]?> Embed(string? text) => Task.FromResult<float[]?>(text == "I have no money."
            ? new[] { 1f, 0f } : new[] { text == line.Original ? before : after, 0f });
        var note = await LocalChecks.IdiomMeaningNoteAsync(line, line.Converted, Embed);
        Assert.Equal(flagged, note != null);
    }

    [Fact]
    public async Task IdiomMeaningDoesNotEmbedWhenNoGlossExists()
    {
        var line = new CheckLine(0, "Olá.", "Olá.", null, "Hello.");
        Assert.Null(await LocalChecks.IdiomMeaningNoteAsync(line, line.Converted,
            _ => throw new InvalidOperationException("No embedding should be requested.")));
    }

    [Fact]
    public async Task IdiomMeaningReusesUnchangedTextAndDoesNotHideEmbeddingFailures()
    {
        var line = new CheckLine(0, "Estou sem dinheiro.", "Estou sem dinheiro.", null, "I'm skint.");
        var calls = 0;
        Task<float[]?> Embed(string? _) { calls++; return Task.FromResult<float[]?>(new[] { 1f, 0f }); }
        Assert.Null(await LocalChecks.IdiomMeaningNoteAsync(line, line.Converted, Embed));
        Assert.Equal(2, calls);
        await Assert.ThrowsAsync<IOException>(() => LocalChecks.IdiomMeaningNoteAsync(line, line.Converted,
            _ => throw new IOException("Embedding failed.")));
    }

    [Fact]
    public void TokenGrammarForcesTokenIds() =>
        Assert.Equal("root ::= <[12]> <[345]>", EmbeddingRuntime.TokenGrammar(new[] { 12, 345 }));

    [Fact]
    public void CheckSettingsAreSavedButPathsAreNot()
    {
        var json = JsonSerializer.Serialize(new EmbeddingSettings(JudgeModelPath: "/x/judge.gguf", CheckMeaning: false, UseJudge: true));
        var back = JsonSerializer.Deserialize<EmbeddingSettings>(json)!;
        Assert.False(back.CheckMeaning);
        Assert.True(back.UseJudge);
        Assert.DoesNotContain("judge.gguf", json);
    }

    [Fact]
    public async Task NothingRunsWithoutModels()
    {
        var settings = new EmbeddingSettings(UseJudge: false, CheckMeaning: false);
        var lines = new[]
        {
            new CheckLine(0, "Eles tem razão.", "Eles tem razão.", null, null),
            new CheckLine(1, "Estou no meu Tod.", "Estou no meu Tod.", null, "I'm on my tod."),
        };
        var (results, summary) = await LocalChecks.RunAsync(lines, settings, new Progress<string>(_ => { }), CancellationToken.None);
        Assert.Empty(results);
        Assert.Equal(string.Empty, summary);
    }

    [OptInFact("PTPT_TEST_JUDGE", "needs the downloaded Gervásio model and llama-server")]
    public async Task GervasioPrefersAgreement()
    {
        var settings = EmbeddingAssets.WithInstalledFiles(EmbeddingAssets.Load() with { UseJudge = true });
        await using var runtime = await EmbeddingRuntime.StartJudgeAsync(settings, new Progress<string>(_ => { }), CancellationToken.None);
        const string context = "Legendas de uma série, em português europeu (de Portugal):\n";
        Assert.True(await runtime.ScoreAsync(context, "Ela tem dois filhos.", CancellationToken.None)
            > await runtime.ScoreAsync(context, "Ela têm dois filhos.", CancellationToken.None));
        var lines = new[]
        {
            new CheckLine(0, "Ouvi dizer que o seu garoto é o detetive chefe?", "Ouvi dizer que o seu miúdo é o detetive chefe?", null, null),
            new CheckLine(1, "Ele tira o casaco.", "Ele tira o casaco.", null, null),
        };
        var (results, _) = await LocalChecks.RunAsync(lines, settings with { CheckMeaning = false }, new Progress<string>(_ => { }), CancellationToken.None);
        Assert.Equal("Ouvi dizer que o seu rapaz é o detetive chefe?", results[0].Text);
        Assert.False(results.ContainsKey(1));
    }

    [OptInFact("PTPT_TEST_IDIOM_MODEL", "real MiniLM idiom and screenshot checks with the installed local model")]
    public async Task RealMiniLmChecksIdiomReadingsAndScreenshotCue()
    {
        var folder = Environment.GetEnvironmentVariable("PTPT_EMBEDDING_DIR") ?? EmbeddingAssets.DirectoryPath;
        var settings = new EmbeddingSettings(
            Directory.GetFiles(folder, OperatingSystem.IsWindows() ? "llama-server.exe" : "llama-server",
                SearchOption.AllDirectories).Single(), Path.Combine(folder, EmbeddingAssets.ModelFile));
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        await using var runtime = await EmbeddingRuntime.StartAsync(settings, new Progress<string>(output.WriteLine), timeout.Token);
        async Task<float[]?> Embed(string? text) => text == null ? null
            : await runtime.EmbedAsync(ReferenceAligner.Clean(text), timeout.Token);
        const string reference = "He was on his tod.";
        var reading = IdiomHints.MeaningReference(reference)!;
        var english = (await Embed(reading))!;
        var faithful = (await Embed("Ele estava sozinho."))!;
        var literal = (await Embed("Ele estava no seu Tod."))!;
        var goodScore = ReferenceAligner.Dot(faithful, english);
        var literalScore = ReferenceAligner.Dot(literal, english);
        output.WriteLine($"Idiom reading: faithful {goodScore:0.000}, literal {literalScore:0.000}.");
        Assert.True(goodScore > literalScore);
        var line = new CheckLine(0, "Você não lembra de mim,\nmas vou te contar uma parada",
            "Não te lembras de mim,\nmas vou contar-te uma coisa", null,
            "You don't remember me, but I have something to tell you");
        var before = (await Embed(line.Original))!;
        var after = (await Embed(line.Converted))!;
        var originalEnglish = (await Embed(line.Reference))!;
        output.WriteLine($"Screenshot: BR/PT {ReferenceAligner.Dot(before, after):0.000}; " +
            $"BR/EN {ReferenceAligner.Dot(before, originalEnglish):0.000}; PT/EN {ReferenceAligner.Dot(after, originalEnglish):0.000}.");
        Assert.Equal(line.Converted, new PortugueseConverter(new ConversionOptions(PostAgreement: false))
            .Convert(line.Original, null, ReferenceHint.FromText(line.Reference)));
        Assert.True(ReferenceAligner.Dot(before, after) >= LocalChecks.MeaningThreshold);
    }

    [OptInFact("PTPT_TEST_FILE_REVIEW", "local full-file reference alignment, conversion and meaning review")]
    public async Task RealSubtitlePipelinePreservesCuesAndReviewsEveryConvertedLine()
    {
        var sourcePath = Environment.GetEnvironmentVariable("PTPT_SOURCE_SRT")
            ?? throw new InvalidOperationException("Set PTPT_SOURCE_SRT.");
        var englishPath = Environment.GetEnvironmentVariable("PTPT_ENGLISH_SRT")
            ?? throw new InvalidOperationException("Set PTPT_ENGLISH_SRT.");
        var source = ReferenceAligner.Load(sourcePath);
        var reference = ReferenceAligner.Load(englishPath);
        var settings = EmbeddingAssets.WithInstalledFiles(new EmbeddingSettings(UseEmbeddings: true));
        Assert.True(LocalChecks.MeaningReady(settings));
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(10));
        var progress = new Progress<string>(output.WriteLine);
        var (alignment, alignmentNote) = await ReferenceRunner.AlignAsync(source, reference, settings, progress, timeout.Token);
        output.WriteLine(alignment.Describe() + alignmentNote);
        Assert.True(alignment.Coverage >= ReferenceAligner.MinimumUsefulCoverage);
        var options = new ConversionOptions(PostAgreement: false, ValidateSpelling: true);
        var converted = SubtitleConversion.Convert(source, options, english: alignment.Texts);
        Assert.Equal(source.Count, converted.Count);
        Assert.All(converted, cue =>
        {
            Assert.False(string.IsNullOrWhiteSpace(cue.Text));
            Assert.DoesNotMatch(@"[\uE000-\uF8FF]", cue.Text);
        });
        var lines = converted.Select((cue, i) => new CheckLine(i, source[i].Text, cue.Text,
            i == 0 ? null : converted[i - 1].Text, alignment.Texts[i])).ToArray();
        var (results, summary) = await LocalChecks.RunAsync(lines, settings, progress, timeout.Token);
        output.WriteLine(summary);
        Assert.All(results, result => Assert.Equal(converted[result.Key].Text, result.Value.Text));
        Assert.Contains("MiniLM:", summary);
        var roundTrip = SubRipParser.Parse(SubRipParser.Serialize(source.Select((cue, i) => new SrtBlock
        {
            StartMs = cue.StartMs, EndMs = cue.EndMs, Text = converted[i].Text,
        }).ToList()));
        Assert.Equal(source.Select(cue => (cue.StartMs, cue.EndMs)), roundTrip.Select(cue => (cue.StartMs, cue.EndMs)));
        var screenshotCue = source.FindIndex(cue => cue.Text.Contains("vou te contar uma parada", StringComparison.Ordinal));
        if (screenshotCue >= 0)
        {
            Assert.Contains("vou contar-te uma coisa", converted[screenshotCue].Text);
            Assert.Contains("something to tell you", alignment.Texts[screenshotCue]);
            output.WriteLine("Screenshot cue: converted correctly with MiniLM-aligned English.");
        }
        var unknown = NaturaDictionary.Load(options.PostAgreement).UnknownWords(converted.Select(cue => cue.Text));
        output.WriteLine($"Reviewed {source.Count} cues; dictionary reports {unknown.Length} unrecognised words (not necessarily errors).");
    }

    [OptInFact("PTPT_TEST_VULKAN", "real MiniLM meaning review on a Vulkan GPU")]
    public async Task MissingInfinitiveEndingKeepsMeaningOnVulkan()
    {
        var settings = EmbeddingAssets.WithInstalledFiles(new EmbeddingSettings(Vulkan: true,
            Device: Environment.GetEnvironmentVariable("PTPT_VULKAN_DEVICE") ?? "Vulkan0"));
        Assert.True(LocalChecks.MeaningReady(settings), "MiniLM and llama-server must be installed in the plugin's download folder.");
        const string original = "Não vou fala mais com vocês.";
        const string english = "I'm not going to talk to you anymore.";
        var converted = new PortugueseConverter().Convert(original);
        Assert.Equal("Não vou falar mais com vocês.", converted);
        Assert.NotNull(GrammarReview.Note(original));
        Assert.Null(GrammarReview.Note(converted));
        Assert.True(LocalChecks.IsComparable(original, english));

        await using (var runtime = await EmbeddingRuntime.StartAsync(settings, new Progress<string>(output.WriteLine), CancellationToken.None))
        {
            Assert.True(runtime.UsesGpu);
            var before = await runtime.EmbedAsync(ReferenceAligner.Clean(original), CancellationToken.None);
            var after = await runtime.EmbedAsync(ReferenceAligner.Clean(converted), CancellationToken.None);
            var reference = await runtime.EmbedAsync(ReferenceAligner.Clean(english), CancellationToken.None);
            var similarity = ReferenceAligner.Dot(before, after);
            var beforeEnglish = ReferenceAligner.Dot(before, reference);
            var afterEnglish = ReferenceAligner.Dot(after, reference);
            output.WriteLine($"Original/corrected: {similarity:0.000}; original/English: {beforeEnglish:0.000}; corrected/English: {afterEnglish:0.000}");
            Assert.True(similarity >= LocalChecks.MeaningThreshold);
            Assert.True(afterEnglish >= LocalChecks.TranslationThreshold);
            Assert.True(beforeEnglish - afterEnglish < LocalChecks.ReferenceDrop);
        }

        var lines = new[]
        {
            new CheckLine(0, original, converted, null, english),
            new CheckLine(1, original, original, null, english),
        };
        var (results, summary) = await LocalChecks.RunAsync(lines, settings, new Progress<string>(output.WriteLine), CancellationToken.None);
        Assert.Empty(results);
        Assert.Contains("0 line(s) whose meaning may have changed", summary);
        Assert.Contains("0 line(s) that may not match the English", summary);
    }
}
