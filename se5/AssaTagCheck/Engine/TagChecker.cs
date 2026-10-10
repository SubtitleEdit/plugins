using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.AssaTagCheck;

/// <summary>
/// Checks the override tags of one subtitle line and fixes what can be fixed safely.
/// Works on the text as Subtitle Edit holds it: line breaks instead of \N, and
/// &lt;i&gt;/&lt;b&gt;/&lt;u&gt;/&lt;font&gt; for the tags Subtitle Edit converts.
/// </summary>
public static class TagChecker
{
    private static readonly Regex NumberInText = new(@"[-+]?(\d+(\.\d*)?|\.\d+)", RegexOptions.Compiled);
    private static readonly Regex NumbersOnly = new(@"^[-+\d.\s,;]+$", RegexOptions.Compiled);
    private static readonly Regex HtmlColor = new(@"^#([0-9A-Fa-f]{2})([0-9A-Fa-f]{2})([0-9A-Fa-f]{2})$", RegexOptions.Compiled);
    private static readonly Regex TagRun = new(@"^(\\[0-9A-Za-z]+(\([^(){}]*\)|&H[0-9A-Fa-f]+&|[-+.\d]+)?)+$", RegexOptions.Compiled);
    private static readonly Regex MissingBackslash = new(
        @"^\s*/?(an[1-9]|i[01]|b[01]|u[01]|s[01]|pos\(\s*-?[\d.]+\s*,\s*-?[\d.]+\s*\)|fad\(\s*\d+\s*,\s*\d+\s*\))\s*$",
        RegexOptions.Compiled);

    private static readonly string[] ToggleKeys = { "i", "b", "u", "s" };
    private static readonly string[] HtmlFontKeys = { "fn", "fs", "1c" };

    public static LineResult CheckLine(string? text, string? style, double durationMs, AssaScript script, IReadOnlySet<RuleId> enabled)
    {
        var issues = new List<Issue>();
        var current = text ?? string.Empty;

        current = FixBraces(current, enabled, issues);

        var tokens = Tokenizer.Tokenize(current);
        CheckTags(tokens, script, enabled, issues);
        current = Tokenizer.Serialize(tokens);

        tokens = Tokenizer.Tokenize(current);
        CheckPlacement(tokens, durationMs, script, enabled, issues);
        current = Tokenizer.Serialize(tokens);

        var lineStyle = script.FindStyle(style, ignoreCase: true) ?? script.DefaultStyle;
        tokens = Tokenizer.Tokenize(current);
        Tidy(tokens, lineStyle, script, enabled, issues);
        current = Tokenizer.Serialize(tokens);

        var styleAfter = CheckStyle(style, script, enabled, issues);
        return new LineResult(current, styleAfter, issues);
    }

    /// <summary>Fonts used by the lines that are neither installed nor attached in [Fonts].</summary>
    public static List<ScriptIssue> CheckScript(IReadOnlyList<(string Text, string? Style)> lines, AssaScript script, Func<string, bool> isFontInstalled)
    {
        var issues = new List<ScriptIssue>();
        if (!script.HasStyles)
        {
            return issues;
        }

        var fontLines = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var (text, style) in lines)
        {
            var fonts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var lineStyle = script.FindStyle(style, ignoreCase: true) ?? script.DefaultStyle;
            if (lineStyle != null && lineStyle.FontName.Length > 0)
            {
                fonts.Add(lineStyle.FontName);
            }

            foreach (var block in Tokenizer.Tokenize(text ?? string.Empty).OfType<BlockToken>())
            {
                foreach (var tag in block.Tags.Where(t => t.Known))
                {
                    if (tag.Name == "fn" && tag.Value.Length > 0)
                    {
                        fonts.Add(tag.Value);
                    }
                    else if (tag.Name == "r" && script.FindStyle(tag.Value, ignoreCase: true) is { FontName.Length: > 0 } rStyle)
                    {
                        fonts.Add(rStyle.FontName);
                    }
                }
            }

            foreach (var font in fonts)
            {
                fontLines[font] = fontLines.GetValueOrDefault(font) + 1;
            }
        }

        foreach (var (font, count) in fontLines.OrderByDescending(f => f.Value))
        {
            var name = font.TrimStart('@');
            if (isFontInstalled(name) || IsAttached(name, script))
            {
                continue;
            }

            issues.Add(new ScriptIssue(
                $"Font \"{name}\" not found",
                $"{Lines(count)} use it. It is not installed on this computer and not attached to the script, so players will draw another font."));
        }

        return issues;
    }

    private static bool IsAttached(string font, AssaScript script)
    {
        if (script.AttachedFonts.Contains(font))
        {
            return true;
        }

        var wanted = Normalize(font);
        return wanted.Length > 0 && script.AttachedFonts.Any(a => Normalize(a).StartsWith(wanted, StringComparison.Ordinal));

        static string Normalize(string s) => new(s.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
    }

    // ------------------------------------------------------------------ braces

    private static string FixBraces(string text, IReadOnlySet<RuleId> enabled, List<Issue> issues)
    {
        if (text.IndexOf('{') < 0 && text.IndexOf('}') < 0)
        {
            return text;
        }

        var tokens = Tokenizer.Tokenize(text);
        var sb = new StringBuilder(text.Length + 4);
        Token? previous = null;
        foreach (var token in tokens)
        {
            switch (token)
            {
                case BlockToken { Closed: true } block when block.Content.Trim().Length == 0:
                    if (Report(enabled, issues, RuleId.EmptyBlock, "Empty override block {}", true))
                    {
                        break;
                    }

                    sb.Append(block.Serialize());
                    break;

                case BlockToken { Closed: true, IsComment: true } block when MissingBackslash.Match(block.Content) is { Success: true } match:
                    var fixedBlock = "{\\" + match.Groups[1].Value + "}";
                    if (Report(enabled, issues, RuleId.MissingBackslash, $"{{{block.Content}}} is a comment, not a tag - should be {fixedBlock}", true))
                    {
                        sb.Append(fixedBlock);
                        break;
                    }

                    sb.Append(block.Serialize());
                    break;

                case BlockToken { Closed: false } block:
                    sb.Append(FixUnclosed(block, enabled, issues));
                    break;

                case TextToken textToken when textToken.Text.IndexOf('}') >= 0:
                    sb.Append(FixStrayBraces(textToken.Text, previous is BlockToken { Closed: true }, enabled, issues));
                    break;

                default:
                    sb.Append(token.Serialize());
                    break;
            }

            previous = token;
        }

        return sb.ToString();
    }

    private static string FixUnclosed(BlockToken block, IReadOnlySet<RuleId> enabled, List<Issue> issues)
    {
        var content = block.Content;
        if (content.Trim().Length == 0)
        {
            return Report(enabled, issues, RuleId.UnclosedBlock, "A lone { with nothing after it", true) ? content : "{" + content;
        }

        var tagEnd = content.TrimStart().StartsWith('\\') ? GreedyTagEnd(content) : -1;
        if (tagEnd <= 0)
        {
            Report(enabled, issues, RuleId.UnclosedBlock, $"Missing }} in {{{Shorten(content)} - players hide or show the rest of the line", false);
            return "{" + content;
        }

        return Report(enabled, issues, RuleId.UnclosedBlock, $"Missing }} after {{{content[..tagEnd]}", true)
            ? "{" + content[..tagEnd] + "}" + content[tagEnd..]
            : "{" + content;
    }

    /// <summary>Where the tags of an unclosed block end and the text starts, or -1 if that can't be told.</summary>
    private static int GreedyTagEnd(string content)
    {
        var i = content.IndexOf('\\');
        while (i >= 0 && i < content.Length && content[i] == '\\')
        {
            var name = AssaTags.MatchName(content, i + 1);
            if (name == null)
            {
                return -1;
            }

            var j = i + 1 + name.Length;
            switch (AssaTags.Spec(name)!.Kind)
            {
                case TagKind.Number:
                case TagKind.Choice:
                    while (j < content.Length && (char.IsDigit(content[j]) || content[j] is '.' or '-' or '+'))
                    {
                        j++;
                    }

                    break;
                case TagKind.Color:
                case TagKind.Alpha:
                    var hex = Regex.Match(content[j..], @"^&?[Hh]?[0-9A-Fa-f]*&*");
                    j += hex.Length;
                    break;
                case TagKind.Parenthesis:
                case TagKind.Transform:
                    if (j >= content.Length || content[j] != '(')
                    {
                        return -1;
                    }

                    var close = content.IndexOf(')', j);
                    if (close < 0)
                    {
                        return -1;
                    }

                    j = close + 1;
                    break;
                default: // \fn, \r: the name could run into the text
                    var next = content.IndexOf('\\', j);
                    if (next < 0)
                    {
                        return -1;
                    }

                    j = next;
                    break;
            }

            i = j;
        }

        return i;
    }

    private static string FixStrayBraces(string text, bool afterBlock, IReadOnlySet<RuleId> enabled, List<Issue> issues)
    {
        var sb = new StringBuilder(text.Length + 2);
        var segmentStart = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] != '}')
            {
                continue;
            }

            var segment = text[segmentStart..i];
            if (segment.Length == 0 && afterBlock && segmentStart == 0)
            {
                if (!Report(enabled, issues, RuleId.StrayBrace, "Extra } after an override block", true))
                {
                    sb.Append('}');
                }
            }
            else if (TagRun.IsMatch(segment) && AssaTags.MatchName(segment, 1) != null)
            {
                if (Report(enabled, issues, RuleId.StrayBrace, $"Missing {{ before {segment}}}", true))
                {
                    sb.Append('{');
                }

                sb.Append(segment).Append('}');
            }
            else
            {
                sb.Append(segment).Append('}'); // a literal brace in the text
            }

            segmentStart = i + 1;
        }

        sb.Append(text[segmentStart..]);
        return sb.ToString();
    }

    // ------------------------------------------------------------------ tags

    private static void CheckTags(List<Token> tokens, AssaScript script, IReadOnlySet<RuleId> enabled, List<Issue> issues)
    {
        foreach (var block in tokens.OfType<BlockToken>().Where(b => b.Closed && b.HasTags))
        {
            foreach (var tag in block.Tags)
            {
                CheckTag(tag, script, enabled, issues);
            }
        }
    }

    private static void CheckTag(TagNode tag, AssaScript script, IReadOnlySet<RuleId> enabled, List<Issue> issues)
    {
        if (!tag.Known)
        {
            var lower = tag.Name.ToLowerInvariant();
            if (tag.Name.Length > 0 && lower != tag.Name && AssaTags.MatchName(lower, 0) != null)
            {
                var fixedRaw = "\\" + lower + tag.Argument;
                if (Report(enabled, issues, RuleId.UnknownTag, $"{tag.Raw.Trim()} should be {fixedRaw.Trim()} - tag names are case-sensitive", true))
                {
                    tag.Raw = fixedRaw;
                }

                return;
            }

            if (Report(enabled, issues, RuleId.UnknownTag, $"Unknown tag {Shorten(tag.Raw.Trim())}", true))
            {
                tag.Removed = true;
            }

            return;
        }

        var spec = tag.Spec!;
        var value = tag.Value;
        switch (spec.Kind)
        {
            case TagKind.Number:
                if (value.Length == 0 ? !spec.AllowEmpty : !AssaTags.IsNumber(value))
                {
                    Report(enabled, issues, RuleId.InvalidValue, value.Length == 0
                        ? $"\\{tag.Name} needs a value"
                        : $"\\{tag.Name} needs a number, not \"{Shorten(value)}\"", false);
                }

                break;

            case TagKind.Choice:
                if (value.Length == 0 && spec.AllowEmpty)
                {
                    break;
                }

                if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var choice) || !spec.Choices!.Contains(choice))
                {
                    Report(enabled, issues, RuleId.InvalidValue,
                        $"\\{tag.Name}{value} - \\{tag.Name} takes {string.Join(", ", spec.Choices!)}", false);
                }

                break;

            case TagKind.Color:
                if (value.Length == 0)
                {
                    break;
                }

                if (HtmlColor.Match(value) is { Success: true } html)
                {
                    var assa = $"&H{html.Groups[3].Value}{html.Groups[2].Value}{html.Groups[1].Value}&".ToUpperInvariant();
                    if (Report(enabled, issues, RuleId.HtmlColor, $"\\{tag.Name}{value} is an HTML color - ASSA writes it {assa} (blue, green, red)", true))
                    {
                        tag.Raw = "\\" + tag.Name + assa;
                    }
                }
                else if (!AssaTags.TryParseHex(value, out _))
                {
                    Report(enabled, issues, RuleId.InvalidValue, $"\\{tag.Name} needs a color like &H0000FF&, not \"{Shorten(value)}\"", false);
                }

                break;

            case TagKind.Alpha:
                if (value.Length > 0 && !AssaTags.TryParseHex(value, out _))
                {
                    Report(enabled, issues, RuleId.InvalidValue, $"\\{tag.Name} needs a value like &H80&, not \"{Shorten(value)}\"", false);
                }

                break;

            case TagKind.Text:
                if (tag.Name == "r" && value.Length > 0 && script.HasStyles && script.FindStyle(value, ignoreCase: false) == null)
                {
                    var match = script.FindStyle(value, ignoreCase: true);
                    if (match != null)
                    {
                        if (Report(enabled, issues, RuleId.UndefinedStyle, $"\\r{value} - the style is called \"{match.Name}\" (names are case-sensitive)", true))
                        {
                            tag.Raw = "\\r" + match.Name;
                        }
                    }
                    else if (Report(enabled, issues, RuleId.UndefinedStyle, $"\\r{value} - there is no style \"{value}\", players reset to the line's style", true))
                    {
                        tag.Raw = "\\r";
                    }
                }

                break;

            case TagKind.Transform:
                if (!tag.Argument.TrimStart().StartsWith('('))
                {
                    Report(enabled, issues, RuleId.InvalidValue, "\\t needs parentheses: \\t(start,end,\\tags)", false);
                }
                else if (!tag.ParenthesisClosed && Report(enabled, issues, RuleId.MissingParenthesis, $"Missing ) in {Shorten(tag.Raw)}", true))
                {
                    tag.Raw = tag.Raw.TrimEnd() + ")";
                }

                break;

            case TagKind.Parenthesis:
                CheckParenthesisTag(tag, spec, enabled, issues);
                break;
        }
    }

    private static void CheckParenthesisTag(TagNode tag, TagSpec spec, IReadOnlySet<RuleId> enabled, List<Issue> issues)
    {
        var counts = spec.ArgumentCounts!;
        var argument = tag.Argument.Trim();
        var countText = string.Join(" or ", counts);

        if (!argument.StartsWith('('))
        {
            var bare = NumberInText.Matches(argument).Select(m => m.Value).ToList();
            var fixedRaw = $"\\{tag.Name}({string.Join(",", bare)})";
            if (NumbersOnly.IsMatch(argument) && counts.Contains(bare.Count))
            {
                if (Report(enabled, issues, RuleId.MissingParenthesis, $"\\{tag.Name}{argument} should be {fixedRaw}", true))
                {
                    tag.Raw = fixedRaw;
                }
            }
            else
            {
                Report(enabled, issues, RuleId.InvalidValue, $"\\{tag.Name} needs {countText} values in parentheses", false);
            }

            return;
        }

        var inner = tag.Value;
        var rest = string.Empty;
        if (!tag.ParenthesisClosed)
        {
            // The parenthesis ran into the next tags: "\pos(10,20\i1".
            var nextTag = inner.IndexOf('\\');
            if (nextTag >= 0)
            {
                rest = inner[nextTag..];
                inner = inner[..nextTag];
            }
        }

        var isDrawing = tag.Name is "clip" or "iclip" && inner.Any(char.IsLetter);
        var parts = inner.Split(',');
        var valid = isDrawing || parts.All(AssaTags.IsNumber);
        var numbers = NumberInText.Matches(inner).Select(m => m.Value).ToList();
        var canNormalize = !valid && NumbersOnly.IsMatch(inner) && counts.Contains(numbers.Count);
        var normalizedInner = canNormalize ? string.Join(",", numbers) : inner.Trim();

        if (!tag.ParenthesisClosed)
        {
            var fixedRaw = $"\\{tag.Name}({normalizedInner}){rest}";
            if (Report(enabled, issues, RuleId.MissingParenthesis, $"Missing ) in \\{tag.Name}({Shorten(inner.Trim())}", true))
            {
                tag.Raw = fixedRaw;
            }
        }
        else if (canNormalize)
        {
            if (Report(enabled, issues, RuleId.ArgumentSeparator, $"\\{tag.Name}({inner}) - values are separated by commas: \\{tag.Name}({normalizedInner})", true))
            {
                tag.Raw = $"\\{tag.Name}({normalizedInner})" + TrailingAfterParenthesis(tag.Argument);
            }
        }

        if (isDrawing)
        {
            return;
        }

        if (valid && !counts.Contains(parts.Length))
        {
            Report(enabled, issues, RuleId.ArgumentCount, $"\\{tag.Name} takes {countText} values, not {parts.Length}: {Shorten(tag.Raw.Trim())}", false);
        }
        else if (!valid && !canNormalize)
        {
            Report(enabled, issues, RuleId.InvalidValue, $"\\{tag.Name} needs {countText} numbers: {Shorten(tag.Raw.Trim())}", false);
        }
    }

    private static string TrailingAfterParenthesis(string argument)
    {
        var close = argument.LastIndexOf(')');
        return close >= 0 ? argument[(close + 1)..] : string.Empty;
    }

    // ------------------------------------------------------------------ placement

    private static void CheckPlacement(List<Token> tokens, double durationMs, AssaScript script, IReadOnlySet<RuleId> enabled, List<Issue> issues)
    {
        var firsts = new Dictionary<string, TagNode>();
        foreach (var tag in tokens.OfType<BlockToken>().Where(b => b.Closed).SelectMany(b => b.Tags).Where(t => t.Known))
        {
            var group = tag.Name switch
            {
                "pos" or "move" => "\\pos/\\move",
                "an" or "a" => "\\an",
                "org" => "\\org",
                "fad" or "fade" => "\\fad",
                _ => null,
            };
            if (group == null)
            {
                continue;
            }

            if (firsts.TryGetValue(group, out var first))
            {
                if (Report(enabled, issues, RuleId.FirstWins, $"Only the first {group} is used ({first.Raw.Trim()}) - {tag.Raw.Trim()} does nothing", true))
                {
                    tag.Removed = true;
                }

                continue;
            }

            firsts[group] = tag;
            var values = tag.Value.Split(',').Select(v => AssaTags.TryParseNumber(v, out var n) ? n : (double?)null).ToList();
            if (values.Any(v => v == null))
            {
                continue;
            }

            if (tag.Name == "fad" && values.Count == 2 && durationMs > 0 && values[0]!.Value + values[1]!.Value > durationMs + 0.5)
            {
                var fadeIn = values[0]!.Value;
                var fadeOut = values[1]!.Value;
                var newIn = Math.Round(fadeIn * durationMs / (fadeIn + fadeOut));
                var newOut = Math.Max(0, Math.Floor(durationMs) - newIn);
                var fixedRaw = $"\\fad({AssaTags.FormatNumber(newIn)},{AssaTags.FormatNumber(newOut)})";
                if (Report(enabled, issues, RuleId.FadeTooLong,
                        $"{tag.Raw.Trim()} fades for {AssaTags.FormatNumber(fadeIn + fadeOut)} ms, but the line is only {AssaTags.FormatNumber(Math.Round(durationMs))} ms - {fixedRaw}", true))
                {
                    tag.Raw = fixedRaw;
                }
            }
            else if (tag.Name == "pos" && values.Count == 2 && script.PlayResX is { } resX && script.PlayResY is { } resY &&
                     (values[0] < 0 || values[1] < 0 || values[0] > resX || values[1] > resY))
            {
                Report(enabled, issues, RuleId.PositionOffScreen, $"{tag.Raw.Trim()} is outside the script resolution {resX}x{resY}", false);
            }
        }
    }

    private static string? CheckStyle(string? style, AssaScript script, IReadOnlySet<RuleId> enabled, List<Issue> issues)
    {
        if (!script.HasStyles || string.IsNullOrWhiteSpace(style) || script.FindStyle(style, ignoreCase: false) != null)
        {
            return style;
        }

        var match = script.FindStyle(style, ignoreCase: true);
        if (match != null)
        {
            return Report(enabled, issues, RuleId.UndefinedStyle, $"Style \"{style}\" is called \"{match.Name}\" - style names are case-sensitive", true)
                ? match.Name
                : style;
        }

        var fallback = script.DefaultStyle;
        if (fallback == null)
        {
            return style;
        }

        return Report(enabled, issues, RuleId.UndefinedStyle, $"Style \"{style}\" does not exist - players draw the line with \"{fallback.Name}\"", true)
            ? fallback.Name
            : style;
    }

    // ------------------------------------------------------------------ tidy

    private sealed class Analysis
    {
        public List<object> Dead { get; } = new();
        public List<object> Redundant { get; } = new();
    }

    private static void Tidy(List<Token> tokens, AssaStyle? lineStyle, AssaScript script, IReadOnlySet<RuleId> enabled, List<Issue> issues)
    {
        var removed = new List<(RuleId Rule, string Text)>();
        var tidyTags = enabled.Contains(RuleId.UnneededEndStart) || enabled.Contains(RuleId.Overridden) || enabled.Contains(RuleId.AlreadyInEffect);
        if (tidyTags && !HasDrawing(tokens) && !tokens.OfType<BlockToken>().Any(b => !b.Closed))
        {
            for (var pass = 0; pass < 12; pass++)
            {
                var analysis = Analyze(tokens, lineStyle, script);
                var dead = analysis.Dead.Where(o => enabled.Contains(IsToggle(o) ? RuleId.UnneededEndStart : RuleId.Overridden)).ToList();
                var target = dead.Count > 0
                    ? dead
                    : analysis.Redundant.Where(o => enabled.Contains(IsToggle(o) ? RuleId.UnneededEndStart : RuleId.AlreadyInEffect)).ToList();
                if (target.Count == 0)
                {
                    break;
                }

                foreach (var owner in target)
                {
                    var rule = IsToggle(owner) ? RuleId.UnneededEndStart : dead.Count > 0 ? RuleId.Overridden : RuleId.AlreadyInEffect;
                    removed.Add((rule, Display(owner)));
                    Remove(owner);
                }
            }
        }

        foreach (var group in removed.GroupBy(r => r.Rule))
        {
            issues.Add(new Issue(group.Key, "Removes " + string.Join("  ", group.Select(r => r.Text)), true));
        }

        if (enabled.Contains(RuleId.MergeBlocks))
        {
            MergeBlocks(tokens, issues);
        }
    }

    private static Analysis Analyze(List<Token> tokens, AssaStyle? lineStyle, AssaScript script)
    {
        var analysis = new Analysis();
        var baseState = InitialState(lineStyle);
        var state = new Dictionary<string, string?>(baseState);
        var pending = new Dictionary<string, object>();
        var ownerKeys = new Dictionary<object, List<string>>();
        var overwritten = new Dictionary<object, HashSet<string>>();

        void Process(object owner, List<(string Key, string? Value)> effects)
        {
            if (effects.All(e => e.Value != null && state.GetValueOrDefault(e.Key) == e.Value))
            {
                analysis.Redundant.Add(owner);
                return;
            }

            foreach (var (key, _) in effects)
            {
                if (pending.TryGetValue(key, out var previous) && previous != owner)
                {
                    overwritten[previous].Add(key);
                }
            }

            foreach (var (key, value) in effects)
            {
                state[key] = value;
                pending[key] = owner;
            }

            ownerKeys[owner] = effects.Select(e => e.Key).ToList();
            overwritten[owner] = new HashSet<string>();
        }

        foreach (var token in tokens.Where(t => !t.Removed))
        {
            switch (token)
            {
                case TextToken text when text.Text.Length > 0:
                    var whitespace = string.IsNullOrWhiteSpace(text.Text);
                    foreach (var key in pending.Keys.ToList())
                    {
                        // Italic or bold on a space can't be seen; underline or a color change on it can.
                        if (!whitespace || key is not ("i" or "b"))
                        {
                            pending.Remove(key);
                        }
                    }

                    break;

                case HtmlToken { Name: "font" }:
                    foreach (var key in HtmlFontKeys)
                    {
                        state[key] = null;
                        pending.Remove(key);
                    }

                    break;

                case HtmlToken html:
                    Process(html, new List<(string, string?)> { (html.Name, html.Closing ? "0" : "1") });
                    break;

                case BlockToken { Closed: true } block:
                    foreach (var tag in block.LiveTags.Where(t => t.Known))
                    {
                        if (tag.Name == "t")
                        {
                            // An animation changes values over time - stop judging.
                            state.Clear();
                            pending.Clear();
                            continue;
                        }

                        if (tag.Name == "r")
                        {
                            var resetStyle = tag.Value.Length == 0 ? lineStyle : script.FindStyle(tag.Value, ignoreCase: true) ?? lineStyle;
                            baseState = InitialState(resetStyle);
                            state = new Dictionary<string, string?>(baseState);
                            pending.Clear();
                            continue;
                        }

                        var effects = Effects(tag, baseState);
                        if (effects.Count > 0)
                        {
                            Process(tag, effects);
                        }
                    }

                    break;
            }
        }

        foreach (var (owner, keys) in ownerKeys)
        {
            if (keys.Count > 0 && keys.All(overwritten[owner].Contains))
            {
                analysis.Dead.Add(owner);
            }
        }

        return analysis;
    }

    /// <summary>The state keys a tag sets and their normalized values (null = can't tell).</summary>
    private static List<(string Key, string? Value)> Effects(TagNode tag, IReadOnlyDictionary<string, string?> baseState)
    {
        var value = tag.Value;
        var empty = value.Length == 0;
        var result = new List<(string, string?)>();

        string? Base(string key) => baseState.GetValueOrDefault(key);
        string? Number() => AssaTags.TryParseNumber(value, out var n) ? AssaTags.FormatNumber(n) : null;
        string? Hex(uint mask, string format) => AssaTags.TryParseHex(value, out var h) ? (h & mask).ToString(format, CultureInfo.InvariantCulture) : null;

        switch (tag.Name)
        {
            case "i":
            case "u":
            case "s":
                result.Add((tag.Name, empty ? Base(tag.Name) : AssaTags.TryParseNumber(value, out var flag) ? (flag == 0 ? "0" : "1") : null));
                break;
            case "b":
                result.Add(("b", empty ? Base("b") : Number()));
                break;
            case "fn":
                result.Add(("fn", empty ? Base("fn") : value.ToLowerInvariant()));
                break;
            case "fs":
                result.Add(("fs", empty ? Base("fs") : value[0] is '+' or '-' ? null : Number()));
                break;
            case "fscx":
            case "fscy":
            case "fsp":
            case "frx":
            case "fry":
            case "fax":
            case "fay":
            case "be":
            case "blur":
                result.Add((tag.Name, empty ? Base(tag.Name) : Number()));
                break;
            case "fr":
            case "frz":
                result.Add(("frz", empty ? Base("frz") : Number()));
                break;
            case "bord":
            case "shad":
                var x = "x" + tag.Name;
                var y = "y" + tag.Name;
                result.Add((x, empty ? Base(x) : Number()));
                result.Add((y, empty ? Base(y) : Number()));
                break;
            case "xbord":
            case "ybord":
            case "xshad":
            case "yshad":
                result.Add((tag.Name, empty ? Base(tag.Name) : Number()));
                break;
            case "c":
            case "1c":
            case "2c":
            case "3c":
            case "4c":
                var colorKey = tag.Name == "c" ? "1c" : tag.Name;
                result.Add((colorKey, empty ? Base(colorKey) : Hex(0xFFFFFF, "X6")));
                break;
            case "alpha":
                foreach (var alphaKey in new[] { "1a", "2a", "3a", "4a" })
                {
                    result.Add((alphaKey, empty ? Base(alphaKey) : Hex(0xFF, "X2")));
                }

                break;
            case "1a":
            case "2a":
            case "3a":
            case "4a":
                result.Add((tag.Name, empty ? Base(tag.Name) : Hex(0xFF, "X2")));
                break;
        }

        return result;
    }

    private static Dictionary<string, string?> InitialState(AssaStyle? style)
    {
        var state = new Dictionary<string, string?>();
        if (style == null)
        {
            return state;
        }

        string? Flag(string field) => style.Field(field) switch
        {
            "-1" or "1" => "1",
            "0" => "0",
            _ => null,
        };

        string? Number(string field) => AssaTags.TryParseNumber(style.Field(field), out var n) ? AssaTags.FormatNumber(n) : null;

        state["i"] = Flag("Italic");
        state["b"] = Flag("Bold");
        state["u"] = Flag("Underline");
        state["s"] = Flag("StrikeOut");
        state["fn"] = style.FontName.Length > 0 ? style.FontName.ToLowerInvariant() : null;
        state["fs"] = Number("Fontsize");
        state["fscx"] = Number("ScaleX");
        state["fscy"] = Number("ScaleY");
        state["fsp"] = Number("Spacing");
        state["frz"] = Number("Angle");
        state["xbord"] = state["ybord"] = Number("Outline");
        state["xshad"] = state["yshad"] = Number("Shadow");
        state["frx"] = state["fry"] = state["fax"] = state["fay"] = state["be"] = state["blur"] = "0";

        var colors = new[] { "PrimaryColour", "SecondaryColour", "OutlineColour", "BackColour" };
        for (var i = 0; i < colors.Length; i++)
        {
            var raw = style.Field(colors[i]);
            uint? color = raw.StartsWith("&H", StringComparison.OrdinalIgnoreCase) && AssaTags.TryParseHex(raw, out var hex) ? hex
                : long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var dec) ? (uint)dec : null;
            state[$"{i + 1}c"] = color is { } c ? (c & 0xFFFFFF).ToString("X6", CultureInfo.InvariantCulture) : null;
            state[$"{i + 1}a"] = color is { } a && raw.StartsWith("&H", StringComparison.OrdinalIgnoreCase)
                ? ((a >> 24) & 0xFF).ToString("X2", CultureInfo.InvariantCulture)
                : null;
        }

        return state;
    }

    private static void MergeBlocks(List<Token> tokens, List<Issue> issues)
    {
        BlockToken? previous = null;
        var merged = new List<string>();
        foreach (var token in tokens)
        {
            var text = token.Serialize();
            if (text.Length == 0)
            {
                continue; // removed, or a block whose tags were all removed
            }

            if (token is BlockToken { Closed: true, HasTags: true, Leading.Length: 0 } block)
            {
                if (previous != null)
                {
                    merged.Add(previous.Serialize() + text);
                    previous.Tags.AddRange(block.LiveTags.ToList());
                    block.Removed = true;
                    continue;
                }

                previous = block;
                continue;
            }

            previous = null;
        }

        if (merged.Count > 0)
        {
            issues.Add(new Issue(RuleId.MergeBlocks, "Joins " + string.Join("  ", merged), true));
        }
    }

    private static bool HasDrawing(List<Token> tokens) =>
        tokens.OfType<BlockToken>().SelectMany(b => b.LiveTags)
            .Any(t => t.Name == "p" && t.Known && AssaTags.TryParseNumber(t.Value, out var n) && n != 0);

    private static bool IsToggle(object owner) => owner switch
    {
        HtmlToken html => html.Name != "font",
        TagNode tag => ToggleKeys.Contains(tag.Name),
        _ => false,
    };

    private static string Display(object owner) => owner switch
    {
        HtmlToken html => html.Raw,
        TagNode tag => tag.Raw.Trim(),
        _ => string.Empty,
    };

    private static void Remove(object owner)
    {
        switch (owner)
        {
            case HtmlToken html:
                html.Removed = true;
                break;
            case TagNode tag:
                tag.Removed = true;
                break;
        }
    }

    // ------------------------------------------------------------------ helpers

    /// <summary>Adds the issue if the rule is on. Returns true when the caller should apply the fix.</summary>
    private static bool Report(IReadOnlySet<RuleId> enabled, List<Issue> issues, RuleId rule, string message, bool fixable)
    {
        if (!enabled.Contains(rule))
        {
            return false;
        }

        issues.Add(new Issue(rule, message, fixable));
        return fixable;
    }

    private static string Shorten(string text) => text.Length > 40 ? text[..37] + "..." : text;

    private static string Lines(int count) => count == 1 ? "1 line" : $"{count:N0} lines";
}
