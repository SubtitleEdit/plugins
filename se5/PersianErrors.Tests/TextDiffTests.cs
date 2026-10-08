using SubtitleEdit.Plugins.PersianErrors;

namespace PersianErrors.Tests;

public class TextDiffTests
{
    [Theory]
    [InlineData("سلام،عزیزم", "سلام، عزیزم")]
    [InlineData("-سلام، خوبی؟\r\n-ممنون", "سلام، خوبی؟ -\r\nممنون -")]
    [InlineData("abc", "xyz")]
    [InlineData("", "new")]
    [InlineData("one\ntwo", "one two")]
    public void SegmentsRebuildBothTexts(string before, string after)
    {
        var (b, a) = TextDiff.Compute(before, after);

        Assert.Equal(before, string.Concat(b.Select(s => s.Text)));
        Assert.Equal(after, string.Concat(a.Select(s => s.Text)));
        Assert.DoesNotContain(b, s => s.Kind == DiffKind.Added);
        Assert.DoesNotContain(a, s => s.Kind == DiffKind.Removed);
    }

    [Fact]
    public void MarksOnlyTheInsertedSpace()
    {
        var (_, after) = TextDiff.Compute("سلام،عزیزم", "سلام، عزیزم");

        Assert.Equal(new[] { new DiffSegment("سلام،", DiffKind.Same), new DiffSegment(" ", DiffKind.Added), new DiffSegment("عزیزم", DiffKind.Same) }, after);
    }

    [Fact]
    public void LineBreaksStayUnchangedWhenTheLineCountMatches()
    {
        var (before, after) = TextDiff.Compute("-a\n-b", "a -\nb -");

        Assert.Contains(before, s => s.Kind == DiffKind.Same && s.Text.Contains('\n'));
        Assert.Contains(after, s => s.Kind == DiffKind.Same && s.Text.Contains('\n'));
    }
}
