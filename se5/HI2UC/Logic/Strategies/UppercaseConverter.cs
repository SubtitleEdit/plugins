using System.Globalization;
using System.Text;

namespace SubtitleEdit.Plugins.HI2UC.Logic.Strategies;

public class UppercaseConverter : IConverterStrategy
{
    private readonly ChunkReader _chunkReader;

    public UppercaseConverter(ChunkReader chunkReader) => _chunkReader = chunkReader;

    public string Name => "Uppercase";
    public string Example => "Foobar → FOOBAR";

    public string Execute(string input)
    {
        var sb = new StringBuilder();
        foreach (var chunk in _chunkReader.Read(input))
        {
            var content = input.Substring(chunk.Start, chunk.End - chunk.Start);
            sb.Append(chunk.IsTag ? content : content.ToUpper(CultureInfo.CurrentCulture));
        }

        return sb.ToString();
    }

    public override string ToString() => Name;
}
