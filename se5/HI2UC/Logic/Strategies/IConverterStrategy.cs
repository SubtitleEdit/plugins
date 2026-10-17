namespace SubtitleEdit.Plugins.HI2UC.Logic.Strategies;

public interface IConverterStrategy
{
    string Name { get; }
    string Example { get; }
    string Execute(string input);
}
