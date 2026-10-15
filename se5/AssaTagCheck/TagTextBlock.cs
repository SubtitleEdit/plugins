using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using System.Text;

namespace SubtitleEdit.Plugins.AssaTagCheck;

/// <summary>
/// Shows one side of a <see cref="TextDiff"/> with override blocks and &lt;tags&gt; colored,
/// and removed/added text on a tint.
/// </summary>
public sealed class TagTextBlock : TextBlock
{
    public static readonly StyledProperty<IReadOnlyList<DiffSegment>?> SegmentsProperty =
        AvaloniaProperty.Register<TagTextBlock, IReadOnlyList<DiffSegment>?>(nameof(Segments));

    private static readonly IBrush RemovedBrush = new SolidColorBrush(Color.Parse("#55E5484D"));
    private static readonly IBrush AddedBrush = new SolidColorBrush(Color.Parse("#5512A594"));
    private static readonly IBrush TagBrush = new SolidColorBrush(Color.Parse("#7C5CFF"));
    private static readonly IBrush CommentBrush = new SolidColorBrush(Color.Parse("#8A8F98"));

    static TagTextBlock()
    {
        SegmentsProperty.Changed.AddClassHandler<TagTextBlock>((block, _) => block.Rebuild());
    }

    public IReadOnlyList<DiffSegment>? Segments
    {
        get => GetValue(SegmentsProperty);
        set => SetValue(SegmentsProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(TextBlock);

    private enum Mode
    {
        Text,
        Block,
        Html,
    }

    private void Rebuild()
    {
        var inlines = new InlineCollection();
        var mode = Mode.Text;
        var blockIsComment = false;
        var segments = Segments ?? Array.Empty<DiffSegment>();
        var all = string.Concat(segments.Select(s => s.Text));
        var position = 0;

        foreach (var segment in segments)
        {
            var background = segment.Kind switch
            {
                DiffKind.Removed => RemovedBrush,
                DiffKind.Added => AddedBrush,
                _ => null,
            };

            var run = new StringBuilder();
            var runMode = mode;
            var runComment = blockIsComment;

            void Flush()
            {
                if (run.Length == 0)
                {
                    return;
                }

                var inline = new Run(run.ToString()) { Background = background };
                if (runMode != Mode.Text)
                {
                    inline.Foreground = runMode == Mode.Block && runComment ? CommentBrush : TagBrush;
                    inline.FontWeight = runComment ? FontWeight.Normal : FontWeight.SemiBold;
                    inline.FontStyle = runComment ? FontStyle.Italic : FontStyle.Normal;
                }

                inlines.Add(inline);
                run.Clear();
            }

            foreach (var c in segment.Text)
            {
                position++;
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

                var opens = mode == Mode.Text && (c == '{' || (c == '<' && LooksLikeTag(all, position - 1)));
                if (opens)
                {
                    Flush();
                    mode = c == '{' ? Mode.Block : Mode.Html;
                    blockIsComment = c == '{' && !NextNonSpaceIsBackslash(all, position);
                    runMode = mode;
                    runComment = blockIsComment;
                }

                run.Append(c);
                if ((mode == Mode.Block && c == '}') || (mode == Mode.Html && c == '>'))
                {
                    Flush();
                    mode = Mode.Text;
                    blockIsComment = false;
                    runMode = mode;
                    runComment = false;
                }
            }

            Flush();
        }

        Inlines = inlines;
    }

    private static bool LooksLikeTag(string text, int index)
    {
        var close = text.IndexOf('>', index);
        if (close < 0)
        {
            return false;
        }

        var inner = text[(index + 1)..close].TrimStart('/').ToLowerInvariant();
        return inner is "i" or "b" or "u" or "s" || inner.StartsWith("font", StringComparison.Ordinal);
    }

    private static bool NextNonSpaceIsBackslash(string text, int index)
    {
        for (var i = index; i < text.Length && text[i] != '}'; i++)
        {
            if (text[i] == '\\')
            {
                return true;
            }
        }

        return false;
    }
}
