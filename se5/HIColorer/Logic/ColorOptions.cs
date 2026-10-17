namespace SubtitleEdit.Plugins.HIColorer.Logic;

public sealed class ColorOptions
{
    public const string DefaultNarratorColor = "#ffff00";
    public const string DefaultMoodsColor = "#00ffff";
    public const string DefaultMusicColor = "#ff80ff";

    public bool Narrator { get; set; } = true;
    public string NarratorColor { get; set; } = DefaultNarratorColor;
    public bool Moods { get; set; } = true;
    public string MoodsColor { get; set; } = DefaultMoodsColor;
    public bool Music { get; set; } = true;
    public string MusicColor { get; set; } = DefaultMusicColor;
}
