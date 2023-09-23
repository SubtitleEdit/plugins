namespace Nikse.SubtitleEdit.PluginLogic.Converters.Strategies;

public interface IConverterStrategy
{
    string Name { get; }
    string Example { get; }
    string Execute(string input);
}