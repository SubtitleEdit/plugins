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

var count = 0;
var blocks = Regex.Split(srt.Replace("\r\n", "\n").Trim('\n'), @"\n[ \t]*\n");
for (var i = 0; i < blocks.Length; i++)
{
    // An empty SelectedIndices means "apply to every line".
    if (selected.Count > 0 && !selected.Contains(i))
    {
        continue;
    }

    // A SubRip block is: number line, timecode line, then one or more text lines.
    var lines = blocks[i].Split('\n');
    if (lines.Length < 3)
    {
        continue;
    }

    for (var t = 2; t < lines.Length; t++)
    {
        lines[t] = HaxorTranslator.Translate(lines[t]);
    }

    blocks[i] = string.Join('\n', lines);
    count++;
}

var response = new PluginResponse
{
    Status = "ok",
    Message = count == 0 ? "No lines changed." : $"Translated {count} line(s) to haxor.",
    UndoDescription = "Haxor 1.0.0",
    Subtitle = new PluginSubtitle
    {
        Format = "SubRip",
        Native = (string.Join("\n\n", blocks) + "\n").Replace("\n", "\r\n"),
    },
};

File.WriteAllText(request.ResponseFilePath, JsonSerializer.Serialize(response, jsonOptions));
return 0;
