using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.PluginLogic.Common;

namespace Nikse.SubtitleEdit.PluginLogic.Converters.Strategies;

public class TitleWordsConverter : IConverterStrategy
{
    private readonly ChunkReader _chunkReader;
    public string Name => "Title case (word)";
    public string Example => "foobar foobar => Foobar Foobar";
        
    private readonly char[] _wordSplitChars = {' ' };

    public TitleWordsConverter(ChunkReader chunkReader)
    {
        _chunkReader = chunkReader;
    }

    public string Execute(string input)
    {
        var sb = new StringBuilder();
        const string whiteSpace = " ";
        foreach (var chunk in _chunkReader.Read(input))
        {
            var content = input.Substring(chunk.Start, chunk.End - chunk.Start);
            if (chunk.IsTag)
            {
                sb.Append(content);
            }
            else
            {
                var procContent = string.Join(whiteSpace, content.Split(_wordSplitChars)
                    .Select(word => word.CapitalizeFirstLetter()));

                sb.Append(procContent);
            }
        }

        return sb.ToString();
    }

    public override string ToString() => Name;
}