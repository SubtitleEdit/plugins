using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace SubtitleEdit.Plugins.PersianErrors;

/// <summary>
/// Shows one side of a <see cref="TextDiff"/>: unchanged text plainly, removed or added text on a
/// tint. Hidden characters (ZWNJ, direction marks, ...) can be drawn as small visible symbols.
/// </summary>
public sealed class DiffTextBlock : TextBlock
{
    public static readonly StyledProperty<IReadOnlyList<DiffSegment>?> SegmentsProperty =
        AvaloniaProperty.Register<DiffTextBlock, IReadOnlyList<DiffSegment>?>(nameof(Segments));

    public static readonly StyledProperty<bool> ShowHiddenProperty =
        AvaloniaProperty.Register<DiffTextBlock, bool>(nameof(ShowHidden), true);

    private static readonly IBrush RemovedBrush = new SolidColorBrush(Color.Parse("#55E5484D"));
    private static readonly IBrush AddedBrush = new SolidColorBrush(Color.Parse("#552FA36B"));
    private static readonly IBrush HiddenBrush = new SolidColorBrush(Color.Parse("#7C5CFF"));

    /// <summary>Visible stand-ins for characters that take no space.</summary>
    public static readonly IReadOnlyDictionary<char, string> HiddenSymbols = new Dictionary<char, string>
    {
        ['\u200C'] = "·",
        ['‍'] = "ZWJ",
        ['‎'] = "LRM",
        ['‏'] = "RLM",
        ['‪'] = "LRE",
        ['\u202B'] = "RLE",
        ['‬'] = "PDF",
        ['‭'] = "LRO",
        ['‮'] = "RLO",
        [' '] = "°",
        ['﻿'] = "BOM",
    };

    static DiffTextBlock()
    {
        SegmentsProperty.Changed.AddClassHandler<DiffTextBlock>((block, _) => block.Rebuild());
        ShowHiddenProperty.Changed.AddClassHandler<DiffTextBlock>((block, _) => block.Rebuild());
    }

    public IReadOnlyList<DiffSegment>? Segments
    {
        get => GetValue(SegmentsProperty);
        set => SetValue(SegmentsProperty, value);
    }

    public bool ShowHidden
    {
        get => GetValue(ShowHiddenProperty);
        set => SetValue(ShowHiddenProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(TextBlock);

    private void Rebuild()
    {
        var inlines = new InlineCollection();
        foreach (var segment in Segments ?? Array.Empty<DiffSegment>())
        {
            var background = segment.Kind switch
            {
                DiffKind.Removed => RemovedBrush,
                DiffKind.Added => AddedBrush,
                _ => null,
            };
            AddText(inlines, segment.Text, background);
        }

        Inlines = inlines;
    }

    private void AddText(InlineCollection inlines, string text, IBrush? background)
    {
        var start = 0;
        for (var i = 0; i <= text.Length; i++)
        {
            var atEnd = i == text.Length;
            var c = atEnd ? '\0' : text[i];
            var isBreak = c == '\n' || c == '\r';
            var isHidden = !atEnd && ShowHidden && HiddenSymbols.ContainsKey(c);
            if (!atEnd && !isBreak && !isHidden)
            {
                continue;
            }

            if (i > start)
            {
                inlines.Add(new Run(text[start..i]) { Background = background });
            }

            if (isHidden)
            {
                var symbol = HiddenSymbols[c];
                inlines.Add(new Run(symbol)
                {
                    Foreground = HiddenBrush,
                    Background = background,
                    FontWeight = FontWeight.Bold,
                    FontSize = symbol.Length > 1 ? FontSize * 0.55 : FontSize,
                    BaselineAlignment = symbol.Length > 1 ? BaselineAlignment.Superscript : BaselineAlignment.Baseline,
                });
            }
            else if (c == '\n' || (c == '\r' && (i + 1 >= text.Length || text[i + 1] != '\n')))
            {
                if (background != null)
                {
                    // A changed line break would otherwise be invisible.
                    inlines.Add(new Run("↵") { Background = background, Foreground = HiddenBrush });
                }

                inlines.Add(new LineBreak());
            }

            start = i + 1;
        }
    }
}
