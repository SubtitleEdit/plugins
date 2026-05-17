using CommunityToolkit.Mvvm.ComponentModel;

namespace SubtitleEdit.Plugins.WordCensor;

public partial class ChangeProposal : ObservableObject
{
    [ObservableProperty] private bool _include = true;
    [ObservableProperty] private string _censoredText;

    public int LineIndex { get; }
    public string LineNumber => (LineIndex + 1).ToString();
    public string OriginalText { get; }

    public ChangeProposal(int lineIndex, string originalText, string censoredText)
    {
        LineIndex = lineIndex;
        OriginalText = originalText;
        _censoredText = censoredText;
    }
}
