namespace Nikse.SubtitleEdit.PluginLogic.Converters.Strategies;

public class NoneConverterStrategy : IConverterStrategy
{
    public string Name => "None";
    public string Example => "(Not available)";
    public string Execute(string input) => input;
    public override string ToString() => Name;
}