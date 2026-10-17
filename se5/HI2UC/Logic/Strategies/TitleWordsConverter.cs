using System.Text;

namespace SubtitleEdit.Plugins.HI2UC.Logic.Strategies;

public class TitleWordsConverter : IConverterStrategy
{
    private static readonly char[] WordSplitChars = { ' ' };
    private readonly ChunkReader _chunkReader;

    public TitleWordsConverter(ChunkReader chunkReader) => _chunkReader = chunkReader;

    public string Name => "Title case";
    public string Example => "foobar foobar → Foobar Foobar";

    public string Execute(string input)
    {
        var sb = new StringBuilder();
        foreach (var chunk in _chunkReader.Read(input))
        {
            var content = input.Substring(chunk.Start, chunk.End - chunk.Start);
            sb.Append(chunk.IsTag
                ? content
                : string.Join(" ", content.Split(WordSplitChars).Select(word => word.CapitalizeFirstLetter())));
        }

        return sb.ToString();
    }

    public override string ToString() => Name;
}
