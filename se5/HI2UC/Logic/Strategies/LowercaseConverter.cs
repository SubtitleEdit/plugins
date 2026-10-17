using System.Globalization;

namespace SubtitleEdit.Plugins.HI2UC.Logic.Strategies;

public class LowercaseConverter : IConverterStrategy
{
    public string Name => "Lowercase";
    public string Example => "Foobar → foobar";

    public string Execute(string input) => input.ToLower(CultureInfo.CurrentCulture);

    public override string ToString() => Name;
}
