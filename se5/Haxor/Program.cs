using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using SubtitleEdit.Plugins.Haxor;

// A Subtitle Edit 5 plugin is just an executable:
//   1. read the request file (its path is the first command-line argument),
//   2. transform the subtitle,
//   3. write the response file (path is given in the request),
//   4. exit with code 0.

if (args.Length < 1)
{
    Console.Error.WriteLine("Usage: Haxor <requestFilePath>");
    return 1;
}

var jsonOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true,
};

PluginRequest? request;
try
{
    request = JsonSerializer.Deserialize<PluginRequest>(File.ReadAllText(args[0]), jsonOptions);
}
catch (Exception exception)
{
    Console.Error.WriteLine("Could not read request: " + exception.Message);
    return 1;
}

if (request is null || string.IsNullOrEmpty(request.ResponseFilePath))
{
    Console.Error.WriteLine("Invalid request.");
    return 1;
}

// Work on the SubRip representation - it is always provided in the request.
var srt = request.Subtitle.SubRip;
var selected = new HashSet<int>(request.SelectedIndices);

// A SubRip block is: number line, timecode line, then text lines up to the next block. Walk the lines
// rather than splitting on blank lines - a line whose text contains a blank line would split in two
// and shift every SelectedIndices entry below it.
var timeCodeLine = new Regex(@"^\s*-?\d+:\d{1,2}:\d{1,2}[,.]\d{1,3}\s*-->");
var lines = srt.Replace("\r\n", "\n").Split('\n');
var index = -1;
var lastChangedIndex = -1;
var count = 0;
for (var i = 0; i < lines.Length; i++)
{
    // A block starts at a number followed by a timecode line - at the start or after a blank line.
    if (i + 1 < lines.Length &&
        (index < 0 || lines[i - 1].Trim().Length == 0) &&
        lines[i].Trim() is { Length: > 0 } number && number.All(char.IsAsciiDigit) &&
        timeCodeLine.IsMatch(lines[i + 1]))
    {
        index++;
        i++;
        continue;
    }

    // An empty SelectedIndices means "apply to every line".
    if (index < 0 || lines[i].Trim().Length == 0 || (selected.Count > 0 && !selected.Contains(index)))
    {
        continue;
    }

    lines[i] = HaxorTranslator.Translate(lines[i]);
    if (lastChangedIndex != index)
    {
        lastChangedIndex = index;
        count++;
    }
}

var response = new PluginResponse
{
    Status = "ok",
    Message = count == 0 ? "No lines changed." : $"Translated {count} line(s) to haxor.",
    UndoDescription = "Haxor 1.0.0",
    Subtitle = new PluginSubtitle
    {
        Format = "SubRip",
        Native = string.Join("\r\n", lines),
    },
};

File.WriteAllText(request.ResponseFilePath, JsonSerializer.Serialize(response, jsonOptions));
return 0;
