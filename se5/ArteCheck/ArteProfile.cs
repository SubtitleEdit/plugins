namespace SubtitleEdit.Plugins.ArteCheck;

/// <summary>An ARTE delivery profile: the EBU language code it is delivered with, and whether it is SDH.</summary>
public sealed record ArteProfile(string Code, string Name, string Description, string LanguageCode, bool IsSdh)
{
    public static readonly IReadOnlyList<ArteProfile> All = new[]
    {
        new ArteProfile("STA", "ARTE STA", "German subtitles", "08", false),
        new ArteProfile("STF", "ARTE STF", "French subtitles", "0F", false),
        new ArteProfile("HG-DEU", "ARTE HG DEU", "German SDH (VA-MAL)", "2D", true),
        new ArteProfile("HG-FRA", "ARTE HG FRA", "French SDH (VF-MAL)", "2F", true),
    };

    public static ArteProfile FromLanguageCode(string? languageCode) =>
        All.FirstOrDefault(p => p.LanguageCode == languageCode) ?? All[0];

    public override string ToString() => $"{Name} - {Description}";
}
