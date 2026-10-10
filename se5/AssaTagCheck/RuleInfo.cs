namespace SubtitleEdit.Plugins.AssaTagCheck;

/// <summary>How a rule is shown: name, explanation, icon and an example line.</summary>
public sealed record RuleInfo(RuleId Rule, string Name, string Description, string Icon, string Example, string Style = "Default", double DurationMs = 2000)
{
    public static IReadOnlyList<RuleInfo> All { get; } = new RuleInfo[]
    {
        new(RuleId.UnclosedBlock, "Unclosed { block",
            "An override block without its closing brace. Players hide the rest of the line or show the tags as text. The } is added where the tags end.",
            "mdi-code-braces", "{\\an8Look up!"),
        new(RuleId.StrayBrace, "Stray } brace",
            "A } without its {: a doubled }} or tags typed without the opening brace.",
            "mdi-code-braces-box", "\\an8}Look up!"),
        new(RuleId.MissingBackslash, "Missing backslash",
            "{an8} or {/an8} is a comment, not a tag, so it does nothing. The backslash is added.",
            "mdi-slash-forward", "{an8}Look up!"),
        new(RuleId.MissingParenthesis, "Missing parenthesis",
            "\\pos, \\move, \\fad, \\clip and \\t need their values in ( ). The missing parenthesis is added.",
            "mdi-code-parentheses", "{\\pos(960,80}Look up!"),
        new(RuleId.ArgumentSeparator, "Wrong separators",
            "Values in ( ) must be separated by commas, not spaces or semicolons.",
            "mdi-comma-circle-outline", "{\\pos(960 80)}Look up!"),
        new(RuleId.ArgumentCount, "Wrong number of values",
            "\\pos takes 2 values, \\move 4 or 6, \\fad 2, \\fade 7, \\clip 4. Players ignore the tag otherwise.",
            "mdi-counter", "{\\move(0,0,960)}Fly in"),
        new(RuleId.HtmlColor, "HTML color",
            "ASSA colors are &HBBGGRR& - blue, green, red. A #RRGGBB color draws black. Converted to the ASSA form.",
            "mdi-palette-outline", "{\\c#FF6633}Orange"),
        new(RuleId.UnknownTag, "Unknown tag",
            "A tag players don't know. Tag names are case-sensitive: \\I1 is lower-cased to \\i1, others are removed.",
            "mdi-tag-hidden", "{\\I1}Italic? {\\glow3}No."),
        new(RuleId.InvalidValue, "Invalid value",
            "A tag value players can't read, like \\fsbig or \\an0. Shown only - fix it by hand.",
            "mdi-alert-octagon-outline", "{\\fsbig}Huge"),

        new(RuleId.UndefinedStyle, "Undefined style",
            "The line or a \\r tag uses a style that isn't in the script. Style names are case-sensitive; a near match is used, otherwise Default.",
            "mdi-format-paint", "Who's there?", Style: "Sign"),
        new(RuleId.FirstWins, "Duplicate position",
            "Only the first \\pos/\\move, \\an, \\org and \\fad of a line is used. The others are removed.",
            "mdi-numeric-1-box-multiple-outline", "{\\an8}Top {\\an2}bottom?"),
        new(RuleId.FadeTooLong, "Fade longer than line",
            "\\fad in + out is longer than the line is shown. The fade is shortened, keeping its proportions.",
            "mdi-transition", "{\\fad(800,800)}Quick line", DurationMs: 1000),
        new(RuleId.PositionOffScreen, "Position off screen",
            "\\pos is outside the script resolution (PlayResX x PlayResY) - often a sign of a wrong resample. Shown only.",
            "mdi-image-filter-center-focus-weak", "{\\pos(2400,1300)}Where?"),

        new(RuleId.UnneededEndStart, "Unneeded end/start tags",
            "Italic, bold, underline or strikeout ended and started again right away, started twice, or ended when not on. Removing them changes nothing on screen.",
            "mdi-tag-off-outline", "<i>One</i> <i>line</i>{\\b0}"),
        new(RuleId.Overridden, "Changed before use",
            "A tag set again before any text is drawn - the first value is never seen.",
            "mdi-layers-remove", "{\\fs40\\fs60}Big"),
        new(RuleId.AlreadyInEffect, "Already in effect",
            "A tag that sets what is already set, by the style or earlier in the line.",
            "mdi-equal-box", "{\\c&H0000FF&}Red, {\\c&H0000FF&}red"),
        new(RuleId.EmptyBlock, "Empty block",
            "An empty {} does nothing.",
            "mdi-checkbox-blank-off-outline", "{}Hello"),
        new(RuleId.MergeBlocks, "Merge adjacent blocks",
            "{\\an8}{\\pos(960,80)} is the same as {\\an8\\pos(960,80)} - one block is easier to read.",
            "mdi-merge", "{\\an8}{\\fs60}Title"),
    };

    public static RuleInfo For(RuleId rule) => All.First(r => r.Rule == rule);
}
