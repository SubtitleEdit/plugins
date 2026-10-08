using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using System.Text;

namespace SubtitleEdit.Plugins.ItalicEachLine;

/// <summary>
/// Shows one side of a <see cref="TextDiff"/>. With <see cref="Rendered"/> off the raw text is shown with
/// tags colored and changed text tinted; with it on the tags are hidden and italic text is drawn in italics.
/// </summary>
public sealed class SubtitleTextBlock : TextBlock
{
    public static readonly StyledProperty<IReadOnlyList<DiffSegment>?> SegmentsProperty =
        AvaloniaProperty.Register<SubtitleTextBlock, IReadOnlyList<DiffSegment>?>(nameof(Segments));

    public static readonly StyledProperty<bool> RenderedProperty =
        AvaloniaProperty.Register<SubtitleTextBlock, bool>(nameof(Rendered));

    private static readonly IBrush RemovedBrush = new SolidColorBrush(Color.Parse("#55E5484D"));
    private static readonly IBrush AddedBrush = new SolidColorBrush(Color.Parse("#552FA36B"));
    private static readonly IBrush TagBrush = new SolidColorBrush(Color.Parse("#8B5CF6"));
    private static readonly IBrush ItalicBrush = new SolidColorBrush(Color.Parse("#228B5CF6"));

    static SubtitleTextBlock()
    {
        SegmentsProperty.Changed.AddClassHandler<SubtitleTextBlock>((block, _) => block.Rebuild());
        RenderedProperty.Changed.AddClassHandler<SubtitleTextBlock>((block, _) => block.Rebuild());
    }

    public IReadOnlyList<DiffSegment>? Segments
    {
        get => GetValue(SegmentsProperty);
        set => SetValue(SegmentsProperty, value);
    }

    public bool Rendered
    {
        get => GetValue(RenderedProperty);
        set => SetValue(RenderedProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(TextBlock);

    private void Rebuild()
    {
        var segments = Segments ?? Array.Empty<DiffSegment>();
        Inlines = Rendered ? BuildRendered(string.Concat(segments.Select(s => s.Text))) : BuildRaw(segments);
    }

    /// <summary>Raw text: tags in the tag color, removed/added text on a tint.</summary>
    private static InlineCollection BuildRaw(IReadOnlyList<DiffSegment> segments)
    {
        var inlines = new InlineCollection();
        var inTag = false;
        foreach (var segment in segments)
        {
            var background = segment.Kind switch
            {
                DiffKind.Removed => RemovedBrush,
                DiffKind.Added => AddedBrush,
                _ => null,
            };

            var run = new StringBuilder();
            var runIsTag = inTag;
            void Flush()
            {
                if (run.Length > 0)
                {
                    var inline = new Run(run.ToString()) { Background = background };
                    if (runIsTag)
                    {
                        inline.Foreground = TagBrush;
                        inline.FontWeight = FontWeight.SemiBold;
                    }

                    inlines.Add(inline);
                    run.Clear();
                }
            }

            foreach (var c in segment.Text)
            {
                if (c == '\r')
                {
                    continue;
                }

                if (c == '\n')
                {
                    Flush();
                    if (background != null)
                    {
                        // A changed line break would otherwise be invisible.
                        inlines.Add(new Run("↵") { Background = background, Foreground = TagBrush });
                    }

                    inlines.Add(new LineBreak());
                    continue;
                }

                var isTag = inTag || c == '<';
                if (isTag != runIsTag)
                {
                    Flush();
                    runIsTag = isTag;
                }

                run.Append(c);
                if (c == '<')
                {
                    inTag = true;
                }
                else if (c == '>')
                {
                    inTag = false;
                    Flush();
                    runIsTag = false;
                }
            }

            Flush();
        }

        return inlines;
    }

    /// <summary>Rendered text: tags hidden, &lt;i&gt; drawn in italics on a soft tint, &lt;b&gt;/&lt;u&gt; applied too.</summary>
    private static InlineCollection BuildRendered(string text)
    {
        var inlines = new InlineCollection();
        var italic = 0;
        var bold = 0;
        var underline = 0;
        var run = new StringBuilder();
        void Flush()
        {
            if (run.Length > 0)
            {
                var inline = new Run(run.ToString())
                {
                    FontStyle = italic > 0 ? FontStyle.Italic : FontStyle.Normal,
                    FontWeight = bold > 0 ? FontWeight.Bold : FontWeight.Normal,
                };
                if (underline > 0)
                {
                    inline.TextDecorations = Avalonia.Media.TextDecorations.Underline;
                }

                if (italic > 0)
                {
                    inline.Background = ItalicBrush;
                }

                inlines.Add(inline);
                run.Clear();
            }
        }

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c == '\r')
            {
                continue;
            }

            if (c == '\n')
            {
                Flush();
                inlines.Add(new LineBreak());
                continue;
            }

            var end = c is '<' or '{' ? text.IndexOf(c == '<' ? '>' : '}', i) : -1;
            if (end < 0)
            {
                run.Append(c);
                continue;
            }

            Flush();
            var tag = text.Substring(i, end - i + 1).ToLowerInvariant();
            i = end;
            switch (tag)
            {
                case "<i>": italic++; break;
                case "</i>": italic = Math.Max(0, italic - 1); break;
                case "<b>": bold++; break;
                case "</b>": bold = Math.Max(0, bold - 1); break;
                case "<u>": underline++; break;
                case "</u>": underline = Math.Max(0, underline - 1); break;
            }
        }

        Flush();
        return inlines;
    }
}
