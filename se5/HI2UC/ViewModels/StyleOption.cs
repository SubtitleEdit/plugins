using SubtitleEdit.Plugins.HI2UC.Logic;

namespace SubtitleEdit.Plugins.HI2UC.ViewModels;

public sealed record StyleOption(HiStyle Style, string Name, string Example)
{
    public override string ToString() => Name;
}
