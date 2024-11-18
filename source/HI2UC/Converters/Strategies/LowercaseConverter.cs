using System.Globalization;

namespace Nikse.SubtitleEdit.PluginLogic.Converters.Strategies;

public class LowercaseConverter : IConverterStrategy
{
    public string Name => "Lowercase";
    public string Example => "Foobar => foobar";

    public string Execute(string input) => input.ToLower(CultureInfo.CurrentCulture);

    public override string ToString() => Name;
}