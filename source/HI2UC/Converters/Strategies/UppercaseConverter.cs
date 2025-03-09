using System.Globalization;
using System.Text;
using Nikse.SubtitleEdit.PluginLogic.Common;

namespace Nikse.SubtitleEdit.PluginLogic.Converters.Strategies;

public class UppercaseConverter : IConverterStrategy
{
    private readonly ChunkReader _chunkReader;
    public string Name => "Uppercase";
    public string Example => "Foobar => FOOBAR";

    public override string ToString() => Name;

    public UppercaseConverter(ChunkReader chunkReader) => _chunkReader = chunkReader;

    public string Execute(string input)
    {
        // Chunk(c) => Array(chunk) => 
        var sb = new StringBuilder();
        foreach (var splitRange in _chunkReader.Read(input))
        {
            var content = input.Substring(splitRange.Start, splitRange.End - splitRange.Start);
            if (splitRange.IsTag)
            {
                sb.Append(content);
            }
            else
            {
                sb.Append(content.ToUpper(CultureInfo.CurrentCulture));
            }
        }

        return sb.ToString();
    }
}