using SubtitleEdit.Plugins.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

/// <summary>
/// Subtitle Edit's Batch convert: a "batch" request converts every line headless with the rule groups
/// saved by Subtitle Edit (settings) plus the local word list; a "configure" request returns the settings.
/// </summary>
public static class BatchMode
{
    public const string Name = "Brazilian to European Portuguese";

    public static PluginResponse BuildBatchResponse(PluginRequest request) =>
        BuildBatchResponse(request, PortugueseLocalWordList.GetDefaultPath());

    public static PluginResponse BuildBatchResponse(PluginRequest request, string wordListPath) =>
        BuildBatchResponse(request, wordListPath, null);

    internal static PluginResponse BuildBatchResponse(PluginRequest request, string wordListPath, EmbeddingSettings? embedding)
    {
        var options = ConversionOptions.Load(request);
        PortugueseLocalWordList localWordList;
        try
        {
            localWordList = PortugueseLocalWordList.Load(wordListPath);
        }
        catch (Exception ex)
        {
            return new PluginResponse
            {
                Status = PluginStatus.Error,
                Message = $"{Name}: could not read the local word list {wordListPath}: {ex.Message}",
            };
        }

        var blocks = SubRipParser.Parse(request.Subtitle.SubRip);
        var (alignment, referenceNote) = options.UseReference ? AlignReference(blocks, request.Subtitle.FileName, embedding) : (null, string.Empty);
        var speakers = SubtitleConversion.Speakers(request, blocks, out var speakerNote);
        if (speakerNote != null) referenceNote += " " + speakerNote;
        var conversions = SubtitleConversion.Convert(blocks, options, localWordList, alignment?.Texts, speakers: speakers);
        var changesByLine = new Dictionary<int, string>();
        var checkLines = new List<CheckLine>();
        for (var i = 0; i < blocks.Count; i++)
        {
            var converted = conversions[i].Text;
            var changed = converted != blocks[i].Text;
            if (changed)
            {
                changesByLine[i] = converted;
            }
            checkLines.Add(new CheckLine(i, blocks[i].Text, changed ? converted : blocks[i].Text,
                i == 0 ? null : changesByLine.TryGetValue(i - 1, out var previous) ? previous : blocks[i - 1].Text,
                alignment?.Texts[i], conversions[i].MeaningSentence));
        }

        referenceNote += OptionalChecks(blocks, checkLines, changesByLine, embedding, options, localWordList);
        var review = 0;
        var grammar = 0;
        var idioms = 0;
        var layout = 0;
        var contextReview = 0;
        for (var i = 0; i < blocks.Count; i++)
        {
            var final = changesByLine.TryGetValue(i, out var judged) ? judged : blocks[i].Text;
            if (ReferenceReview.Note(final, conversions[i].Hint) != null) review++;
            if (GrammarReview.Note(final) != null) grammar++;
            if (IdiomHints.Note(final, alignment?.Texts[i]) != null) idioms++;
            if (conversions[i].ContextNote != null) contextReview++;
            final = SubtitleLayout.Rewrap(final, options);
            if (SubtitleLayout.Review(final, blocks[i], options) != null) layout++;
            if (final != blocks[i].Text) changesByLine[i] = final;
            else changesByLine.Remove(i);
        }
        if (alignment != null && review > 0)
            referenceNote += $" {review} line(s) to review (English you/your kept as você/seu).";
        if (idioms > 0)
            referenceNote += $" {idioms} line(s) with an English idiom to check (open the plugin to see the PT-PT hints).";
        if (grammar > 0)
            referenceNote += $" {grammar} line(s) with a verb-form or auxiliary construction to check - open the plugin to review them.";
        if (layout > 0) referenceNote += $" {layout} line(s) with layout/reading-speed warnings (open the plugin for details).";
        if (contextReview > 0) referenceNote += $" {contextReview} line(s) with address/sentence-context review notes.";
        if (options.SentenceContext) referenceNote += " Sentence-context conversion enabled.";

        var validation = string.Empty;
        if (options.ValidateSpelling)
        {
            var words = NaturaDictionary.Load(options.PostAgreement).UnknownWords(
                blocks.Select((block, index) => changesByLine.TryGetValue(index, out var text) ? text : block.Text));
            validation = words.Length == 0 ? " Dictionary validation: all words recognised."
                : $" Dictionary validation: {words.Length} unrecognised words (not necessarily errors): " +
                  string.Join(", ", words.Take(30)) + (words.Length > 30 ? "…" : "");
        }
        if (changesByLine.Count == 0)
        {
            return new PluginResponse
            {
                Status = PluginStatus.Ok,
                Message = $"{Name}: nothing to convert." + referenceNote + validation,
                Settings = options.ToJson(),
                SettingsVersion = ConversionOptions.Version,
            };
        }

        var response = BuildOkResponse(blocks, changesByLine, options);
        response.Message += referenceNote + validation;
        return response;
    }

    /// <summary>
    /// The English .srt next to the file, aligned like the window does (MiniLM on CPU/Vulkan when ticked and downloaded).
    /// A failing reference never fails the file: it falls back to timing or to no reference, and says so.
    /// </summary>
    internal static (ReferenceAlignment? Alignment, string Note) AlignReference(List<SrtBlock> blocks, string? fileName,
        EmbeddingSettings? settings = null)
    {
        string? path;
        List<SrtBlock> reference;
        try
        {
            path = ReferenceFinder.Find(fileName, null);
            if (path == null) return (null, string.Empty);
            reference = ReferenceAligner.Load(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or InvalidDataException)
        {
            return (null, $" English reference ignored: {ex.Message}");
        }

        var name = Path.GetFileName(path);
        settings ??= EmbeddingAssets.Load();
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(10));
            var (alignment, note) = Task.Run(() => ReferenceRunner.AlignAsync(blocks, reference, settings, new Progress<string>(_ => { }), timeout.Token))
                .GetAwaiter().GetResult();
            return (alignment, $" English reference {name}: {alignment.Describe()}{note}");
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            var alignment = ReferenceAligner.AlignByTiming(blocks, reference);
            return (alignment, $" English reference {name}: MiniLM failed ({ex.Message}), so only timing was used. {alignment.Describe()}");
        }
    }

    /// <summary>Optional post-rule checks. Grammar findings are counted; details are available in preview.</summary>
    private static string OptionalChecks(List<SrtBlock> blocks, List<CheckLine> lines, Dictionary<int, string> changesByLine,
        EmbeddingSettings? settings, ConversionOptions options, PortugueseLocalWordList words)
    {
        settings = EmbeddingAssets.WithInstalledFiles(settings ?? EmbeddingAssets.Load());
        var grammar = options.GrammarCheck && options.RuleEnabled("languagetool");
        if (!grammar) settings = settings with { CheckMeaning = false };
        if (!LocalChecks.JudgeReady(settings) && !grammar) return string.Empty;
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(60));
            var (results, summary) = Task.Run(() => LocalChecks.RunAsync(lines, settings, new Progress<string>(_ => { }), timeout.Token, options, words))
                .GetAwaiter().GetResult();
            foreach (var (index, result) in results)
            {
                if (result.Text != blocks[index].Text) changesByLine[index] = result.Text;
                else changesByLine.Remove(index);
            }
            return " " + summary;
        }
        catch (Exception ex) when (ex is IOException or System.Net.Http.HttpRequestException or InvalidOperationException or
            UnauthorizedAccessException or System.Text.Json.JsonException or OperationCanceledException or
            PlatformNotSupportedException or System.ComponentModel.Win32Exception)
        {
            return $" Optional checks failed; rule-only output retained: {ex.Message}";
        }
    }

    public static PluginResponse BuildConfigureResponse(ConversionOptions options) => new()
    {
        Status = PluginStatus.Ok,
        Message = $"{Name}: settings saved.",
        Settings = options.ToJson(),
        SettingsVersion = ConversionOptions.Version,
    };

    internal static PluginResponse BuildOkResponse(List<SrtBlock> blocks, IReadOnlyDictionary<int, string> changesByLine, ConversionOptions options)
    {
        // A line the user emptied by hand is deleted from the subtitle.
        var removed = new HashSet<int>();
        for (var i = 0; i < blocks.Count; i++)
        {
            if (changesByLine.TryGetValue(i, out var newText))
            {
                if (string.IsNullOrWhiteSpace(newText)) removed.Add(i);
                else blocks[i].Text = newText;
            }
        }

        if (removed.Count > 0)
        {
            var kept = blocks.Where((_, i) => !removed.Contains(i)).ToList();
            blocks.Clear();
            blocks.AddRange(kept);
        }

        return new PluginResponse
        {
            Status = PluginStatus.Ok,
            Message = $"Converted {changesByLine.Count - removed.Count} line(s) to European Portuguese." +
                      (removed.Count > 0 ? $" Removed {removed.Count} empty line(s)." : string.Empty),
            UndoDescription = Name,
            Settings = options.ToJson(),
            SettingsVersion = ConversionOptions.Version,
            Subtitle = new PluginSubtitle
            {
                Format = "SubRip",
                Native = SubRipParser.Serialize(blocks),
            },
        };
    }
}
