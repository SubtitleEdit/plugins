using CommunityToolkit.Mvvm.ComponentModel;

namespace SubtitleEdit.Plugins.SplitDialogs;

public partial class DialogRow : ObservableObject
{
    [ObservableProperty] private bool _include = true;
    [ObservableProperty] private string _afterText = string.Empty;

    public Cue Cue { get; }

    /// <summary>One text per speaker, dashes removed, lines joined with '\n'.</summary>
    public IReadOnlyList<string> Texts { get; }

    public IReadOnlyList<SplitPart> Parts { get; private set; } = Array.Empty<SplitPart>();

    /// <summary>1-based, as in Subtitle Edit's grid.</summary>
    public string LineNumber { get; }

    public string BeforeText { get; }

    public DialogRow(Cue cue, IReadOnlyList<string> texts)
    {
        Cue = cue;
        Texts = texts;
        LineNumber = (cue.Index + 1).ToString();
        BeforeText = SplitTiming.FormatTime(cue.StartMs) + "\n" + string.Join("\n", cue.Lines);
    }

    public void UpdateParts(int gapMs)
    {
        Parts = SplitTiming.Distribute(Cue.StartMs, Cue.EndMs, Texts, gapMs);
        AfterText = string.Join("\n", Parts.Select(p => SplitTiming.FormatTime(p.StartMs) + "  " + p.Text.Replace("\n", " / ")));
    }
}
