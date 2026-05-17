using CommunityToolkit.Mvvm.ComponentModel;

namespace SubtitleEdit.Plugins.AmericanToBritish;

public partial class ChangeProposal : ObservableObject
{
    [ObservableProperty] private bool _include = true;

    public int LineIndex { get; }
    public string LineNumber => (LineIndex + 1).ToString();
    public string OriginalText { get; }
    public string ConvertedText { get; }

    public ChangeProposal(int lineIndex, string originalText, string convertedText)
    {
        LineIndex = lineIndex;
        OriginalText = originalText;
        ConvertedText = convertedText;
    }
}
