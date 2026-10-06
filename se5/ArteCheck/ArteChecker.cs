using SubtitleEdit.Plugins.Shared;

namespace SubtitleEdit.Plugins.ArteCheck;

/// <summary>
/// ARTE EBU STL / teletext delivery checks (ported from SubtitleEdit/subtitleedit#15185 by
/// Triathlon-rally). ARTE delivers at 25 fps, so every time code is a multiple of 40 ms.
/// </summary>
public sealed class ArteChecker
{
    public const double FrameMs = 40.0;
    public const int BottomRow = 23;

    public const string GroupHeader = "Header / language";
    public const string GroupStartTimeCode = "Start time code";
    public const string GroupBlank = "ARTE blank subtitle";
    public const string GroupFrames = "Frame-accurate time codes";
    public const string GroupDuration = "Display duration";
    public const string GroupLayout = "Split / rebalance";
    public const string GroupPosition = "Teletext line position";
    public const string GroupColors = "Teletext colors";
    public const string GroupItalic = "Italic formatting";
    public const string GroupSpaces = "Unneeded spaces";
    public const string GroupGaps = "Minimum gaps";

    private readonly ArteOptions _options;
    private readonly List<ArteFix> _fixes = new();

    private ArteChecker(ArteOptions options) => _options = options;

    private double MinimumGapMs => _options.MinimumGapFrames * FrameMs;

    public static List<ArteFix> Analyze(IReadOnlyList<PluginParagraph> source, string? header, ArteOptions options)
    {
        var checker = new ArteChecker(options);
        var subtitle = ConvertFrameRate(source.Select(p => p.Clone()).ToList(), options.SourceFrameRate);
        checker.Run(subtitle, header);
        foreach (var fix in checker._fixes.Where(f => f.Index > 0 && f.Index <= subtitle.Count && string.IsNullOrEmpty(f.TimeRange)))
        {
            fix.TimeRange = FormatTimeRange(subtitle[fix.Index - 1]);
        }

        return checker._fixes;
    }

    private void Run(List<PluginParagraph> subtitle, string? header)
    {
        var enabled = _options.EnabledChecks;
        AnalyzeHeader(header);
        if (_options.ShiftToStartTimeCode)
        {
            AnalyzeStartTimeCodeShift(subtitle);
        }

        if (enabled.Contains(ArteCheckType.BlankSubtitle)) AnalyzeBlankSubtitle(subtitle);
        if (enabled.Contains(ArteCheckType.FrameAccurateTimeCodes)) AnalyzeFrameAccurateTimeCodes(subtitle);
        if (enabled.Contains(ArteCheckType.DisplayDuration)) AnalyzeDisplayDurations(subtitle);
        if (enabled.Contains(ArteCheckType.MaximumTwoLines)) AnalyzeMaximumTwoLines(subtitle);
        if (enabled.Contains(ArteCheckType.TeletextLinePosition)) AnalyzeLinePositions(subtitle);

        // A color code takes a teletext cell, so colors are normalized before line lengths are measured.
        if (enabled.Contains(ArteCheckType.TeletextColors)) AnalyzeColors(subtitle);
        if (enabled.Contains(ArteCheckType.TeletextLineLength)) AnalyzeLineLengths(subtitle);
        if (enabled.Contains(ArteCheckType.Italic)) AnalyzeItalic(subtitle);
        if (enabled.Contains(ArteCheckType.UnneededSpaces)) AnalyzeUnneededSpaces(subtitle);
        if (enabled.Contains(ArteCheckType.MinimumGaps)) AnalyzeMinimumGaps(subtitle);
    }

    private void Add(ArteFix fix) => _fixes.Add(fix);

    // ---- frame rate / time code helpers ------------------------------------------------------

    public static double GetFrameForCalculation(double frameRate) =>
        Math.Abs(frameRate - 23.976) < 0.001 ? 24000.0 / 1001.0 :
        Math.Abs(frameRate - 29.97) < 0.001 ? 30000.0 / 1001.0 :
        Math.Abs(frameRate - 59.94) < 0.001 ? 60000.0 / 1001.0 :
        frameRate;

    /// <summary>
    /// Converts to 25 fps around the programme start (10:00:00:00, 01:00:00:00, ...): only the time
    /// relative to it is scaled, so the start time code itself does not move.
    /// </summary>
    public static List<PluginParagraph> ConvertFrameRate(List<PluginParagraph> subtitle, double sourceFrameRate)
    {
        if (Math.Abs(sourceFrameRate - 25.0) < 0.001 || subtitle.Count == 0)
        {
            return subtitle;
        }

        var factor = GetFrameForCalculation(sourceFrameRate) / 25.0;
        var reference = GetStartTimeCodeReference(subtitle);
        foreach (var p in subtitle)
        {
            p.StartMs = reference + (p.StartMs - reference) * factor;
            p.EndMs = reference + (p.EndMs - reference) * factor;
        }

        return subtitle;
    }

    /// <summary>The leading blank subtitle's start, or else the full hour the first subtitle is in.</summary>
    public static double GetStartTimeCodeReference(IReadOnlyList<PluginParagraph> subtitle)
    {
        if (subtitle.Count == 0)
        {
            return 0;
        }

        var first = subtitle[0];
        return TeletextText.IsBlank(first.Text)
            ? first.StartMs
            : Math.Floor(first.StartMs / 3_600_000.0) * 3_600_000.0;
    }

    public static double RoundToFrame(double ms) => Math.Round(ms / FrameMs, MidpointRounding.AwayFromZero) * FrameMs;

    public static string FormatTimeCode(double ms)
    {
        var frames = (long)Math.Round(ms / FrameMs, MidpointRounding.AwayFromZero);
        return $"{frames / 90000:00}:{frames / 1500 % 60:00}:{frames / 25 % 60:00}:{frames % 25:00}";
    }

    /// <summary>HH:MM:SS.mmm - shows the off-frame milliseconds that frame rounding removes.</summary>
    public static string FormatPrecise(double ms)
    {
        var t = TimeSpan.FromMilliseconds(Math.Round(ms));
        return $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}.{t.Milliseconds:000}";
    }

    public static string FormatDuration(double ms)
    {
        var frames = (long)Math.Round(ms / FrameMs, MidpointRounding.AwayFromZero);
        return $"{frames / 25} s {frames % 25:00} fr";
    }

    public static string FormatTimeRange(PluginParagraph p) =>
        $"{FormatTimeCode(p.StartMs)} → {FormatTimeCode(p.EndMs)}  ·  {FormatDuration(p.EndMs - p.StartMs)}";

    public static bool TryParseTimeCode(string text, out double ms)
    {
        ms = 0;
        var parts = text.Trim().Split(':', ';', '.', ',');
        if (parts.Length != 4 || !parts.All(p => int.TryParse(p, out _)))
        {
            return false;
        }

        var v = parts.Select(int.Parse).ToArray();
        if (v[1] > 59 || v[2] > 59 || v[3] > 24)
        {
            return false;
        }

        ms = ((v[0] * 3600.0 + v[1] * 60 + v[2]) * 25 + v[3]) * FrameMs;
        return true;
    }

    // ---- checks ------------------------------------------------------------------------------

    private void AnalyzeHeader(string? headerText)
    {
        var hasStlHeader = GsiHeader.IsStlHeader(headerText);
        var header = hasStlHeader ? GsiHeader.Parse(headerText!) : GsiHeader.CreateDefault();
        var original = header.Clone();
        header.CodePage = "850";
        header.DiskFormatCode = "STL25.01";
        header.DisplayStandardCode = "2";
        header.CharacterCodeTable = "00";
        header.LanguageCode = _options.Profile.LanguageCode;
        header.MaxCharactersPerRow = "40";
        header.MaxRows = "23";
        var after = header.DescribeDifferences(original);
        if (after.Length > 0)
        {
            var before = hasStlHeader ? original.DescribeDifferences(header) : "No EBU STL header";
            Add(new ArteFix(GroupHeader, true, 0, before, after,
                hasStlHeader
                    ? $"Set the EBU header to the {_options.Profile.Name} delivery values (25 fps teletext, code page 850)."
                    : $"Create an EBU header with the {_options.Profile.Name} delivery values.",
                ArteFixKind.Header)
            {
                ProposedHeader = header.ToString(),
            });
        }
    }

    private void AnalyzeStartTimeCodeShift(List<PluginParagraph> subtitle)
    {
        var current = GetStartTimeCodeReference(subtitle);
        var target = RoundToFrame(_options.TargetStartMs);
        if (Math.Abs(target - current) < 0.01)
        {
            return;
        }

        Add(new ArteFix(GroupStartTimeCode, true, 0, FormatTimeCode(current), FormatTimeCode(target),
            "Shift every subtitle so the programme starts at the requested time code.", ArteFixKind.ShiftStartTimeCode)
        {
            ProposedStartMs = target - current,
        });
    }

    private void AnalyzeBlankSubtitle(List<PluginParagraph> subtitle)
    {
        if (subtitle.Count == 0)
        {
            return;
        }

        var first = subtitle[0];
        var startMs = GetStartTimeCodeReference(subtitle);
        var expectedEndMs = startMs + 200.0;
        if (!TeletextText.IsBlank(first.Text))
        {
            if (Math.Abs(first.StartMs - startMs) < 0.01)
            {
                Add(new ArteFix(GroupBlank, false, 1, first.Text, string.Empty,
                    "The first subtitle starts on the programme start time code, where ARTE needs the blank control subtitle. Move it manually."));
                return;
            }

            var blank = new PluginParagraph { StartMs = startMs, EndMs = expectedEndMs, Text = string.Empty, MarginV = "22" };
            Add(new ArteFix(GroupBlank, true, 0, "Missing", FormatTimeRange(blank),
                "Insert the blank ARTE control subtitle (5 frames) at the programme start.", ArteFixKind.CreateBlankSubtitle)
            {
                ProposedParagraph = blank,
            });
            return;
        }

        if (Math.Abs(first.EndMs - expectedEndMs) >= 0.01)
        {
            var nextStart = subtitle.Count > 1 ? subtitle[1].StartMs : double.PositiveInfinity;
            var canFix = expectedEndMs <= nextStart - MinimumGapMs;
            var expected = new PluginParagraph { StartMs = startMs, EndMs = expectedEndMs };
            Add(new ArteFix(GroupBlank, canFix, 1, FormatTimeRange(first), canFix ? FormatTimeRange(expected) : string.Empty,
                canFix
                    ? "The blank control subtitle must last exactly 5 frames."
                    : "The blank control subtitle must last 5 frames, but the next subtitle starts too early.",
                ArteFixKind.DisplayDuration)
            {
                ProposedEndMs = canFix ? expectedEndMs : null,
            });
        }
    }

    private void AnalyzeFrameAccurateTimeCodes(List<PluginParagraph> subtitle)
    {
        for (var i = 0; i < subtitle.Count; i++)
        {
            var p = subtitle[i];
            var start = RoundToFrame(p.StartMs);
            var end = RoundToFrame(p.EndMs);
            if (Math.Abs(p.StartMs - start) < 0.01 && Math.Abs(p.EndMs - end) < 0.01)
            {
                continue;
            }

            if (end <= start)
            {
                Add(new ArteFix(GroupFrames, false, i + 1, FormatTimeRange(p), string.Empty,
                    "Rounding to whole 25 fps frames leaves no duration."));
                continue;
            }

            Add(new ArteFix(GroupFrames, true, i + 1, $"{FormatPrecise(p.StartMs)} → {FormatPrecise(p.EndMs)}",
                $"{FormatPrecise(start)} → {FormatPrecise(end)}",
                "Round the time codes to whole 25 fps frames.", ArteFixKind.FrameAccurateTimeCode)
            {
                ProposedStartMs = start,
                ProposedEndMs = end,
            });
        }
    }

    /// <summary>Reading time the text needs (minimum duration or CPS, whichever is longer).</summary>
    private double RequiredDurationMs(string text)
    {
        var cps = _options.MaximumCharactersPerSecond;
        var reading = cps > 0 ? TeletextText.VisibleCharacterCount(text) * 1000.0 / cps : 0;
        return Math.Max(_options.MinimumDurationMs, reading);
    }

    private double AcceptedMinimumDurationMs(string text) =>
        _options.AcceptShortDurations
            ? _options.ShortMinimumFrames * FrameMs
            : RequiredDurationMs(text) * Math.Max(0, 1.0 - _options.ReadingDurationTolerancePercent / 100.0);

    private void AnalyzeDisplayDurations(List<PluginParagraph> subtitle)
    {
        var maximumMs = _options.MaximumDurationMs;
        for (var i = 0; i < subtitle.Count; i++)
        {
            var p = subtitle[i];
            if (TeletextText.IsBlank(p.Text))
            {
                continue;
            }

            var duration = p.EndMs - p.StartMs;
            var required = RequiredDurationMs(p.Text);
            var accepted = AcceptedMinimumDurationMs(p.Text);
            var tooShort = duration < accepted;
            var tooLong = maximumMs > 0 && duration > maximumMs;
            if (!tooShort && !tooLong)
            {
                continue;
            }

            var desiredEnd = RoundToFrame(p.StartMs + maximumMs);
            double? proposedStart = null;
            bool canFix;
            if (tooShort)
            {
                // Keep the existing minimum policy; place the complete target on one side only.
                var targetFrames = (long)Math.Ceiling((_options.AcceptShortDurations
                    ? _options.ShortMinimumFrames * FrameMs : required) / FrameMs);
                var startFrame = (long)Math.Ceiling(p.StartMs / FrameMs);
                var endFrame = (long)Math.Floor(p.EndMs / FrameMs);
                var desiredEndFrame = startFrame + targetFrames;
                var latestEndFrame = i + 1 < subtitle.Count
                    ? (long)Math.Floor(subtitle[i + 1].StartMs / FrameMs) - _options.MinimumGapFrames
                    : long.MaxValue;
                desiredEnd = desiredEndFrame * FrameMs;
                canFix = desiredEnd > p.StartMs && desiredEndFrame <= latestEndFrame;
                if (!canFix)
                {
                    var desiredStartFrame = endFrame - targetFrames;
                    var earliestStartFrame = i > 0
                        ? Math.Max(0L, (long)Math.Ceiling(subtitle[i - 1].EndMs / FrameMs) + _options.MinimumGapFrames)
                        : 0L;
                    if (desiredStartFrame >= earliestStartFrame)
                    {
                        proposedStart = desiredStartFrame * FrameMs;
                        canFix = true;
                    }
                }
            }
            else
            {
                // Maximum-duration shortening retains its existing behavior.
                var nextStart = i + 1 < subtitle.Count ? subtitle[i + 1].StartMs - MinimumGapMs : double.PositiveInfinity;
                canFix = desiredEnd > p.StartMs && desiredEnd <= nextStart && desiredEnd >= required + p.StartMs;
            }
            var issue = tooShort
                ? _options.AcceptShortDurations
                    ? $"Shown for {FormatDuration(duration)}, below the {_options.ShortMinimumFrames}-frame short minimum."
                    : $"Shown for {FormatDuration(duration)}, below the tolerated minimum of {FormatDuration(accepted)} " +
                      $"({_options.ReadingDurationTolerancePercent:0.#}% under the {FormatDuration(required)} reading time)."
                : $"Shown for {FormatDuration(duration)}, above the maximum of {FormatDuration(maximumMs)}.";
            Add(new ArteFix(GroupDuration, canFix, i + 1, FormatDuration(duration),
                canFix ? FormatDuration(proposedStart.HasValue ? p.EndMs - proposedStart.Value : desiredEnd - p.StartMs) : string.Empty,
                canFix ? issue + (proposedStart.HasValue ? " Optional: move the in time." : " Optional: move the out time.")
                    : issue + (tooShort ? " No room to move the out or in time - edit manually." : " No room to move the out time - edit manually."),
                ArteFixKind.DisplayDuration, applyByDefault: false)
            {
                ProposedStartMs = proposedStart,
                ProposedEndMs = canFix && !proposedStart.HasValue ? desiredEnd : null,
            });
        }
    }

    private void AnalyzeMaximumTwoLines(List<PluginParagraph> subtitle)
    {
        for (var i = 0; i < subtitle.Count; i++)
        {
            var lineCount = TeletextText.LineCount(subtitle[i].Text);
            if (lineCount > 2)
            {
                AddLayoutProposal(subtitle[i], i + 1, $"{lineCount} lines - teletext allows two.");
            }
        }
    }

    private void AnalyzeLinePositions(List<PluginParagraph> subtitle)
    {
        // Double height: the stored row is the first physical row, so one line belongs on 22 (22+23)
        // and two lines on 20 (20+21, 22+23).
        var correctBottom = 0;
        var oneRowHigh = 0;
        foreach (var p in subtitle)
        {
            if (string.IsNullOrWhiteSpace(p.Text) || !int.TryParse(p.MarginV, out var row))
            {
                continue;
            }

            var lines = TeletextText.LineCount(p.Text);
            if (lines > 2)
            {
                continue;
            }

            var expected = lines == 1 ? 22 : 20;
            if (row == expected)
            {
                correctBottom++;
            }
            else if (row == expected - 1)
            {
                oneRowHigh++;
            }
        }

        var shiftWholeFile = oneRowHigh > 0 && oneRowHigh > correctBottom;
        for (var i = 0; i < subtitle.Count; i++)
        {
            var p = subtitle[i];
            if (string.IsNullOrWhiteSpace(p.Text))
            {
                if (p.MarginV != "22")
                {
                    Add(new ArteFix(GroupPosition, true, i + 1, string.IsNullOrWhiteSpace(p.MarginV) ? "Not set" : p.MarginV, "22",
                        "A blank subtitle sits on row 22.", ArteFixKind.TeletextLinePosition));
                }

                continue;
            }

            var lineCount = TeletextText.LineCount(p.Text);
            if (lineCount > 2)
            {
                continue;
            }

            var expectedRow = lineCount == 1 ? 22 : 20;
            var hasRow = int.TryParse(p.MarginV, out var currentRow);
            if (shiftWholeFile && hasRow)
            {
                Add(new ArteFix(GroupPosition, true, i + 1, currentRow.ToString(), (currentRow + 1).ToString(),
                    "Most subtitles sit one row too high - move the whole file down one row.", ArteFixKind.TeletextLinePosition));
                continue;
            }

            if (hasRow && ((lineCount == 1 && currentRow == BottomRow) || (lineCount == 2 && currentRow > expectedRow)))
            {
                Add(new ArteFix(GroupPosition, true, i + 1, currentRow.ToString(), expectedRow.ToString(),
                    $"Double height: {lineCount} line(s) must start on row {expectedRow} to stay on the page.",
                    ArteFixKind.TeletextLinePosition));
                continue;
            }

            // Higher rows are deliberate (e.g. a title at the top) and stay as they are.
            if (!hasRow)
            {
                Add(new ArteFix(GroupPosition, true, i + 1, "Not set", expectedRow.ToString(),
                    $"No teletext row - place {lineCount} line(s) at the bottom (row {expectedRow}).",
                    ArteFixKind.TeletextLinePosition));
            }
        }
    }

    private static string NormalizeColors(string text, bool isSdh, out bool hasUnsupported)
    {
        var unsupported = false;
        var normalized = TeletextText.ColorAttribute.Replace(text, match =>
        {
            var color = match.Groups["quoted"].Success ? match.Groups["quoted"].Value :
                match.Groups["single"].Success ? match.Groups["single"].Value : match.Groups["bare"].Value;
            var teletext = TeletextText.NearestTeletextColor(color);
            if (teletext == null)
            {
                unsupported = true;
                return match.Value;
            }

            if (isSdh && TeletextText.IsStandardForeground(color)) return match.Value;
            return "color=\"" + (isSdh ? teletext : "Yellow") + "\"";
        });
        hasUnsupported = unsupported;
        return normalized;
    }

    private void AnalyzeColors(List<PluginParagraph> subtitle)
    {
        var isSdh = _options.Profile.IsSdh;

        var relevant = subtitle.Where(p => !TeletextText.IsBlank(p.Text)).ToList();
        // Near-yellow votes as yellow (it is written as yellow) but is still normalized below.
        var yellowCount = relevant.Count(p => TeletextText.EffectiveForeground(p.Text, nearest: true) == "Yellow");
        var plainCount = relevant.Count(p => TeletextText.EffectiveForeground(p.Text, nearest: true) == "None");
        var wantYellow = !isSdh && yellowCount > plainCount;
        // Prove the excess against the same foreground-free text, before layout analysis.
        if (wantYellow && relevant.Any(p =>
        {
            var plain = TeletextText.WithoutForeground(p.Text);
            return TeletextText.Fits(plain, _options.TeletextMaxCells) &&
                !TeletextText.Fits(TeletextText.WithYellow(plain), _options.TeletextMaxCells);
        })) wantYellow = false;

        for (var i = 0; i < subtitle.Count; i++)
        {
            var text = subtitle[i].Text;
            if (TeletextText.IsBlank(text)) continue;
            string normalized;
            bool unsupported;
            if (isSdh)
            {
                normalized = NormalizeColors(text, true, out unsupported);
            }
            else
            {
                // Boxing is for SDH only; removing it is separate from the color decision.
                var withoutBox = TeletextText.RemoveBox(text);
                var effective = TeletextText.EffectiveForeground(withoutBox);
                unsupported = TeletextText.ColorAttribute.Matches(withoutBox).Any(m =>
                    TeletextText.NearestTeletextColor(TeletextText.ColorValue(m)) == null);
                normalized = effective == (wantYellow ? "Yellow" : "None") || unsupported
                    ? withoutBox : wantYellow ? TeletextText.WithYellow(TeletextText.WithoutForeground(withoutBox))
                    : TeletextText.WithoutForeground(withoutBox);
            }

            if (normalized != text)
            {
                Add(new ArteFix(GroupColors, true, i + 1, text, normalized,
                    isSdh ? "Map the colors to the eight teletext colors." :
                    TeletextText.HasBox(text) ? "Boxing is for SDH only; normal subtitles are yellow or uncolored." :
                    "Normal subtitles are either all yellow or uncolored.",
                    ArteFixKind.TeletextColor));
            }

            if (unsupported)
            {
                Add(new ArteFix(GroupColors, false, i + 1, text, string.Empty,
                    "A color cannot be mapped to a teletext color - choose one manually."));
            }
        }
    }

    private void AnalyzeLineLengths(List<PluginParagraph> subtitle)
    {
        for (var i = 0; i < subtitle.Count; i++)
        {
            var p = subtitle[i];
            if (string.IsNullOrWhiteSpace(p.Text))
            {
                continue;
            }

            // Measure the text as it will be written: after a proposed color normalization.
            var text = _fixes.LastOrDefault(f => f.Index == i + 1 && f.Kind == ArteFixKind.TeletextColor)?.After ?? p.Text;
            var lines = TeletextText.MeasureLines(text);
            for (var lineIndex = 0; lineIndex < lines.Count; lineIndex++)
            {
                var (visible, colorCells) = lines[lineIndex];
                var maximum = Math.Max(1, _options.TeletextMaxCells - colorCells);
                if (visible.Length > maximum)
                {
                    var layout = text == p.Text ? p : new PluginParagraph { StartMs = p.StartMs, EndMs = p.EndMs, Text = text, MarginV = p.MarginV };
                    AddLayoutProposal(layout, i + 1,
                        $"Line {lineIndex + 1} has {visible.Length} characters" +
                        (colorCells > 0 ? $" + {colorCells} color cell(s)" : string.Empty) +
                        $" - the limit is {_options.TeletextMaxCells}.");
                    break;
                }
            }
        }
    }

    private void AddLayoutProposal(PluginParagraph p, int index, string reason)
    {
        // A subtitle can fail both the line-count and the line-width check; offer one fix.
        if (_fixes.Any(f => f.Index == index && f.Group == GroupLayout))
        {
            return;
        }

        var lines = TeletextText.SplitLines(p.Text);
        var isDialog = lines.Length == 2 && lines.All(l => TeletextText.RemoveTags(l).TrimStart().StartsWith('-'));
        var proposed = isDialog ? null : TeletextText.Rebalance(p.Text, _options.TeletextMaxCells);
        if (proposed != null && proposed != p.Text)
        {
            Add(new ArteFix(GroupLayout, true, index, p.Text, proposed, reason + " Rebalance the line break.", ArteFixKind.Rebalance));
            return;
        }

        if (!AddSplitProposal(p, index, reason, isDialog ? lines : null))
        {
            Add(new ArteFix(GroupLayout, false, index, p.Text, string.Empty, reason + " No automatic split fits - edit manually."));
        }
    }

    private bool AddSplitProposal(PluginParagraph p, int index, string reason, string[]? speakerLines = null)
    {
        bool Alarm(string message)
        {
            Add(new ArteFix(GroupLayout, false, index, p.Text, string.Empty, reason + " " + message));
            return true;
        }

        if (TeletextText.HasFontColor(p.Text))
        {
            return Alarm("Colored text is not split automatically - edit manually.");
        }

        // Do not cut other formatting scopes across the preserved speaker boundary.
        if (speakerLines != null && System.Text.RegularExpressions.Regex.IsMatch(TeletextText.RemoveItalic(p.Text), @"<[^>]*>"))
        {
            return Alarm("Formatted text is not split automatically - edit manually.");
        }

        List<string>? texts;
        if (speakerLines != null)
        {
            texts = new List<string>();
            foreach (var line in speakerLines)
            {
                var wrapped = TeletextText.Rebalance(TeletextText.RemoveItalic(line), _options.TeletextMaxCells);
                if (wrapped == null)
                {
                    return false;
                }

                texts.Add(wrapped);
            }
        }
        else
        {
            texts = TeletextText.SplitToFit(TeletextText.RemoveItalic(p.Text), _options.TeletextMaxCells);
        }
        if (texts == null)
        {
            return false;
        }

        if (TeletextText.Words(string.Join(" ", texts)) != TeletextText.Words(TeletextText.RemoveItalic(p.Text)))
        {
            return Alarm("A split would change the text - edit manually.");
        }

        var parts = texts.Select(t => new PluginParagraph { Text = t }).ToList();
        if (!TryFitSplitTiming(parts, p.StartMs, p.EndMs, out var timingError))
        {
            return Alarm(timingError);
        }

        var preview = string.Join("\n\n", parts.Select((part, n) => $"{n + 1}.  {FormatTimeRange(part)}\n{part.Text}"));
        Add(new ArteFix(GroupLayout, true, index, p.Text, preview,
            reason + $" Split into {parts.Count} subtitles inside the original time range." +
            (TeletextText.HasItalic(p.Text) ? " Removes the italic tags." : string.Empty),
            ArteFixKind.Split)
        {
            SplitParagraphs = parts,
        });
        return true;
    }

    /// <summary>Shares the original time range out in whole frames, by text length, with the minimum gap between parts.</summary>
    private bool TryFitSplitTiming(List<PluginParagraph> parts, double startMs, double endMs, out string error)
    {
        error = string.Empty;
        startMs = RoundToFrame(startMs);
        endMs = RoundToFrame(endMs);
        var available = (int)Math.Round((endMs - startMs) / FrameMs) - _options.MinimumGapFrames * (parts.Count - 1);
        var weights = parts.Select(p => Math.Max(1, TeletextText.VisibleCharacterCount(p.Text))).ToArray();
        var required = parts.Select(p => Math.Max(1, (int)Math.Ceiling(RequiredDurationMs(p.Text) / FrameMs))).ToArray();
        var minimum = parts.Select(p => Math.Max(1, (int)Math.Ceiling(AcceptedMinimumDurationMs(p.Text) / FrameMs))).ToArray();
        if (available < minimum.Sum())
        {
            error = $"Not enough time for {parts.Count} subtitles plus {_options.MinimumGapFrames}-frame gaps - edit manually.";
            return false;
        }

        var maximum = _options.MaximumDurationMs > 0 ? Math.Max(1, (int)Math.Floor(_options.MaximumDurationMs / FrameMs)) : available;
        var durations = minimum.ToArray();
        for (var remaining = available - durations.Sum(); remaining > 0; remaining--)
        {
            // Fill the parts still short of their reading time first, then share the rest by length.
            var best = -1;
            for (var i = 0; i < parts.Count; i++)
            {
                if (durations[i] >= maximum)
                {
                    continue;
                }

                if (best < 0)
                {
                    best = i;
                    continue;
                }

                var shortI = durations[i] < required[i];
                var shortBest = durations[best] < required[best];
                if (shortI != shortBest ? shortI : (double)durations[i] / weights[i] < (double)durations[best] / weights[best])
                {
                    best = i;
                }
            }

            if (best < 0)
            {
                break;
            }

            durations[best]++;
        }

        var cursor = startMs;
        for (var i = 0; i < parts.Count; i++)
        {
            parts[i].StartMs = cursor;
            parts[i].EndMs = cursor + durations[i] * FrameMs;
            cursor += (durations[i] + _options.MinimumGapFrames) * FrameMs;
        }

        return true;
    }

    private void AnalyzeItalic(List<PluginParagraph> subtitle)
    {
        for (var i = 0; i < subtitle.Count; i++)
        {
            var text = subtitle[i].Text;
            if (TeletextText.HasItalic(text))
            {
                Add(new ArteFix(GroupItalic, true, i + 1, text, TeletextText.RemoveItalic(text),
                    "Teletext has no italics - remove the italic tags.", ArteFixKind.RemoveItalic));
            }
        }
    }

    private void AnalyzeUnneededSpaces(List<PluginParagraph> subtitle)
    {
        for (var i = 0; i < subtitle.Count; i++)
        {
            var p = subtitle[i];
            if (_fixes.Any(f => f.Index == i + 1 && f.Kind is ArteFixKind.Rebalance or ArteFixKind.Split))
            {
                continue;
            }

            var cleaned = TeletextText.TrimLines(p.Text);
            if (cleaned != TeletextText.NormalizeNewLines(p.Text))
            {
                Add(new ArteFix(GroupSpaces, true, i + 1, p.Text, cleaned,
                    "Leading or trailing spaces take teletext cells.", ArteFixKind.UnneededSpaces));
            }
        }
    }

    private void AnalyzeMinimumGaps(List<PluginParagraph> subtitle)
    {
        var minimumGapFrames = _options.MinimumGapFrames;
        var starts = subtitle.Select(p => RoundToFrame(p.StartMs)).ToArray();
        var ends = subtitle.Select(p => RoundToFrame(p.EndMs)).ToArray();
        for (var i = 1; i < subtitle.Count; i++)
        {
            var gapFrames = (int)Math.Round((starts[i] - ends[i - 1]) / FrameMs, MidpointRounding.AwayFromZero);
            if (gapFrames >= minimumGapFrames)
            {
                continue;
            }

            var missing = minimumGapFrames - gapFrames;
            var split = _fixes.FirstOrDefault(f => f.Index == i && f.SplitParagraphs != null);
            var previousStart = split?.SplitParagraphs![^1].StartMs ?? starts[i - 1];
            var previousText = split?.SplitParagraphs![^1].Text ?? subtitle[i - 1].Text;
            var previousCapacity = Math.Max(0, (int)Math.Floor((ends[i - 1] - previousStart - AcceptedMinimumDurationMs(previousText)) / FrameMs + 0.0001));
            var currentCapacity = Math.Max(0, (int)Math.Floor((ends[i] - starts[i] - AcceptedMinimumDurationMs(subtitle[i].Text)) / FrameMs + 0.0001));
            var gapText = $"{gapFrames} frame{(Math.Abs(gapFrames) == 1 ? string.Empty : "s")}";
            if (previousCapacity + currentCapacity < missing)
            {
                Add(new ArteFix(GroupGaps, false, i, gapText, string.Empty,
                    $"No room for a {minimumGapFrames}-frame gap between subtitles {i} and {i + 1} without going under the minimum durations - edit manually.",
                    ArteFixKind.MinimumGap));
                continue;
            }

            // Share the missing frames: half from the previous out time, half from the next in time.
            var previousShift = Math.Min(previousCapacity, missing / 2);
            var currentShift = Math.Min(currentCapacity, missing - previousShift);
            var remaining = missing - previousShift - currentShift;
            var addPrevious = Math.Min(previousCapacity - previousShift, remaining);
            previousShift += addPrevious;
            remaining -= addPrevious;
            currentShift += Math.Min(currentCapacity - currentShift, remaining);

            var newEnd = ends[i - 1] - previousShift * FrameMs;
            var newStart = starts[i] + currentShift * FrameMs;
            ends[i - 1] = newEnd;
            starts[i] = newStart;
            var how = previousShift > 0 && currentShift > 0
                ? $"Out time of {i} {previousShift} frame(s) earlier, in time of {i + 1} {currentShift} frame(s) later."
                : previousShift > 0
                    ? $"Out time of {i} {previousShift} frame(s) earlier."
                    : $"In time of {i + 1} {currentShift} frame(s) later.";
            Add(new ArteFix(GroupGaps, true, i, gapText, $"{minimumGapFrames} frames", how, ArteFixKind.MinimumGap)
            {
                ProposedEndMs = newEnd,
                ProposedStartMs = newStart,
            });
        }
    }

    // ---- apply -------------------------------------------------------------------------------

    public sealed record ApplyResult(List<PluginParagraph> Paragraphs, string? Header, int Applied);

    public static ApplyResult Apply(IReadOnlyList<PluginParagraph> source, string? header, IReadOnlyList<ArteFix> fixes, ArteOptions options)
    {
        var subtitle = source.Select(p => p.Clone()).ToList();
        var applied = 0;
        if (Math.Abs(options.SourceFrameRate - 25.0) > 0.001)
        {
            subtitle = ConvertFrameRate(subtitle, options.SourceFrameRate);
            applied++;
        }

        var selected = fixes.Where(f => f.CanBeFixed && f.Apply).ToList();

        // Rows first (a rebalance keeps the corrected bottom edge), italic removal last.
        foreach (var fix in selected
                     .Where(f => f.Kind is not (ArteFixKind.Split or ArteFixKind.CreateBlankSubtitle or ArteFixKind.ShiftStartTimeCode))
                     .OrderBy(f => f.Kind == ArteFixKind.TeletextLinePosition ? 0 : f.Kind == ArteFixKind.RemoveItalic ? 2 : 1))
        {
            if (fix.Kind == ArteFixKind.Header)
            {
                header = fix.ProposedHeader;
                applied++;
                continue;
            }

            if (fix.Index <= 0 || fix.Index > subtitle.Count)
            {
                continue;
            }

            var p = subtitle[fix.Index - 1];
            switch (fix.Kind)
            {
                case ArteFixKind.FrameAccurateTimeCode:
                    p.StartMs = fix.ProposedStartMs!.Value;
                    p.EndMs = fix.ProposedEndMs!.Value;
                    break;
                case ArteFixKind.DisplayDuration:
                    if (fix.ProposedStartMs.HasValue) p.StartMs = fix.ProposedStartMs.Value;
                    if (fix.ProposedEndMs.HasValue) p.EndMs = fix.ProposedEndMs.Value;
                    break;
                case ArteFixKind.MinimumGap:
                    p.EndMs = fix.ProposedEndMs!.Value;
                    if (fix.ProposedStartMs.HasValue && fix.Index < subtitle.Count)
                    {
                        subtitle[fix.Index].StartMs = fix.ProposedStartMs.Value;
                    }

                    break;
                case ArteFixKind.TeletextLinePosition:
                    p.MarginV = fix.After;
                    break;
                case ArteFixKind.Rebalance:
                    p.MarginV = RowKeepingBottomEdge(p.MarginV, TeletextText.LineCount(p.Text), TeletextText.LineCount(fix.After), options.DoubleHeight) ?? p.MarginV;
                    p.Text = fix.After;
                    break;
                case ArteFixKind.RemoveItalic:
                    p.Text = TeletextText.RemoveItalic(p.Text);
                    break;
                case ArteFixKind.UnneededSpaces:
                case ArteFixKind.TeletextColor:
                    p.Text = fix.After;
                    break;
                default:
                    continue;
            }

            applied++;
        }

        // Splits after every index-based edit, from the end so earlier indices stay valid.
        foreach (var fix in selected.Where(f => f.Kind == ArteFixKind.Split).OrderByDescending(f => f.Index))
        {
            var original = subtitle[fix.Index - 1];
            var trim = selected.Any(f => f.Index == fix.Index && f.Kind == ArteFixKind.UnneededSpaces);
            var replacements = fix.SplitParagraphs!.Select(part =>
            {
                var result = original.Clone();
                result.Text = trim ? TeletextText.TrimLines(part.Text) : part.Text;
                result.StartMs = part.StartMs;
                result.EndMs = part.EndMs;
                result.MarginV = RowKeepingBottomEdge(original.MarginV, TeletextText.LineCount(original.Text), TeletextText.LineCount(result.Text), options.DoubleHeight) ?? original.MarginV;
                return result;
            }).ToList();

            // A gap fix may have moved the original start or end.
            replacements[0].StartMs = original.StartMs;
            replacements[^1].EndMs = original.EndMs;
            subtitle.RemoveAt(fix.Index - 1);
            subtitle.InsertRange(fix.Index - 1, replacements);
            applied++;
        }

        foreach (var fix in selected.Where(f => f.Kind == ArteFixKind.CreateBlankSubtitle))
        {
            subtitle.Insert(0, fix.ProposedParagraph!.Clone());
            applied++;
        }

        foreach (var fix in selected.Where(f => f.Kind == ArteFixKind.ShiftStartTimeCode))
        {
            var offset = fix.ProposedStartMs!.Value;
            foreach (var p in subtitle)
            {
                p.StartMs += offset;
                p.EndMs += offset;
            }

            if (GsiHeader.IsStlHeader(header))
            {
                var gsi = GsiHeader.Parse(header!);
                var frames = Math.Max(0L, (long)Math.Round(GetStartTimeCodeReference(subtitle) / FrameMs));
                gsi.TimeCodeStartOfProgramme = $"{frames / 90000:00}{frames / 1500 % 60:00}{frames / 25 % 60:00}{frames % 25:00}";
                header = gsi.ToString();
            }

            applied++;
        }

        return new ApplyResult(subtitle, header, applied);
    }

    /// <summary>
    /// The row that keeps a bottom-anchored subtitle's bottom edge when its line count changes
    /// (double height: two rows per line). Null when the row must stay as it is.
    /// </summary>
    public static string? RowKeepingBottomEdge(string? marginV, int oldLineCount, int newLineCount, bool doubleHeight)
    {
        if (oldLineCount == newLineCount || oldLineCount < 1 || newLineCount < 1 ||
            !int.TryParse(marginV, out var row) || row < 1 || row > BottomRow)
        {
            return null;
        }

        var newRow = row + (oldLineCount - newLineCount) * (doubleHeight ? 2 : 1);
        return newRow is >= 1 and <= BottomRow ? newRow.ToString() : null;
    }
}
