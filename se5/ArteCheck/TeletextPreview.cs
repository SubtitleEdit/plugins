using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.ArteCheck;

/// <summary>
/// Shows subtitle text the way a teletext decoder draws it: monospaced, on black, in the eight
/// teletext colors (white when uncolored), with the tags themselves hidden.
/// </summary>
public sealed partial class TeletextPreview : TextBlock
{
    public static readonly StyledProperty<string?> MarkupProperty =
        AvaloniaProperty.Register<TeletextPreview, string?>(nameof(Markup));

    [GeneratedRegex(@"<(/?)(font|i|b|u|box)\b([^>]*)>|\{\\[^}]*\}", RegexOptions.IgnoreCase)]
    private static partial Regex TokenRegex();

    static TeletextPreview()
    {
        MarkupProperty.Changed.AddClassHandler<TeletextPreview>((preview, _) => preview.Rebuild());
    }

    public string? Markup
    {
        get => GetValue(MarkupProperty);
        set => SetValue(MarkupProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(TextBlock);

    private static IBrush BrushFor(string? color) => (TeletextText.NearestTeletextColor(color ?? "white") ?? "White") switch
    {
        "Black" => new SolidColorBrush(Color.FromRgb(0x40, 0x40, 0x40)),
        "Red" => new SolidColorBrush(Color.FromRgb(0xFF, 0x4D, 0x4D)),
        "Green" => new SolidColorBrush(Color.FromRgb(0x3D, 0xF5, 0x5A)),
        "Yellow" => new SolidColorBrush(Color.FromRgb(0xFF, 0xF2, 0x3D)),
        "Blue" => new SolidColorBrush(Color.FromRgb(0x5C, 0x8D, 0xFF)),
        "Magenta" => new SolidColorBrush(Color.FromRgb(0xFF, 0x5C, 0xF0)),
        "Cyan" => new SolidColorBrush(Color.FromRgb(0x3D, 0xF5, 0xF5)),
        _ => Brushes.White,
    };

    private void Rebuild()
    {
        var inlines = new InlineCollection();
        var text = TeletextText.NormalizeNewLines(Markup ?? string.Empty);
        var colors = new Stack<string?>();
        string? color = null;
        var italic = false;
        var position = 0;

        void AddText(string value)
        {
            var lines = value.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                if (i > 0)
                {
                    inlines.Add(new LineBreak());
                }

                if (lines[i].Length > 0)
                {
                    inlines.Add(new Run(lines[i])
                    {
                        Foreground = BrushFor(color),
                        FontStyle = italic ? FontStyle.Italic : FontStyle.Normal,
                    });
                }
            }
        }

        foreach (Match match in TokenRegex().Matches(text))
        {
            AddText(text[position..match.Index]);
            position = match.Index + match.Length;
            var closing = match.Groups[1].Value == "/";
            switch (match.Groups[2].Value.ToLowerInvariant())
            {
                case "font" when closing:
                    color = colors.Count > 0 ? colors.Pop() : null;
                    break;
                case "font":
                    colors.Push(color);
                    var attribute = TeletextText.ColorAttribute.Match(match.Groups[3].Value);
                    if (attribute.Success)
                    {
                        color = attribute.Groups["quoted"].Success ? attribute.Groups["quoted"].Value :
                            attribute.Groups["single"].Success ? attribute.Groups["single"].Value : attribute.Groups["bare"].Value;
                    }

                    break;
                case "i":
                    italic = !closing;
                    break;
            }
        }

        AddText(text[position..]);
        Inlines = inlines;
    }
}
