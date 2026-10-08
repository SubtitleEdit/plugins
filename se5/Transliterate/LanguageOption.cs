using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using SubtitleEdit.Plugins.Transliterate.Engine;

namespace SubtitleEdit.Plugins.Transliterate;

/// <summary>A language in the sidebar.</summary>
public sealed partial class LanguageOption : ObservableObject
{
    private static readonly Dictionary<string, string> Colors = new()
    {
        ["sr"] = "#E5484D", ["cnr"] = "#D6409F", ["mk"] = "#F76B15", ["ru"] = "#3E63DD", ["uk"] = "#FFB224",
        ["be"] = "#30A46C", ["bg"] = "#12A594", ["kk"] = "#0090FF", ["uz"] = "#8E4EC6", ["el"] = "#0588F0", ["ko"] = "#E54666",
    };

    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isDetected;

    public LanguageOption(Transliterator language)
    {
        Language = language;
        var color = Color.Parse(Colors.TryGetValue(language.Id, out var hex) ? hex : "#8B5CF6");
        Brush = new SolidColorBrush(color);
        Tint = new SolidColorBrush(Color.FromArgb(0x26, color.R, color.G, color.B));
    }

    public Transliterator Language { get; }
    public string Name => Language.Name;
    public string NativeName => Language.NativeName;
    public string Badge => Language.Badge;
    public IBrush Brush { get; }
    public IBrush Tint { get; }

    public string ScriptText => Language.CanReverse
        ? $"{Language.Script} ↔ Latin"
        : $"{Language.Script} → Latin";
}

/// <summary>A romanization system button.</summary>
public sealed partial class SystemOption : ObservableObject
{
    [ObservableProperty] private bool _isSelected;

    public SystemOption(int index, TransliterationSystem system)
    {
        Index = index;
        Name = system.Name;
        Description = system.Description;
    }

    public int Index { get; }
    public string Name { get; }
    public string Description { get; }
}
