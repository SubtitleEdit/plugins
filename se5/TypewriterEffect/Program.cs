using Avalonia;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SubtitleEdit.Plugins.TypewriterEffect;

// A Subtitle Edit 5 plugin is just an executable:
//   1. read the request file (its path is the first command-line argument),
//   2. show the Avalonia window,
//   3. write the response file (path is given in the request),
//   4. exit with code 0.
public static class Program
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
    };

    [STAThread]
    public static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("Usage: TypewriterEffect <requestFilePath>");
            return 1;
        }

        PluginRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<PluginRequest>(File.ReadAllText(args[0]), JsonOpts);
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

        App.PendingRequest = request;
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(Array.Empty<string>());

        var response = App.Response ?? new PluginResponse { Status = "cancelled" };
        File.WriteAllText(request.ResponseFilePath, JsonSerializer.Serialize(response, JsonOpts));
        return 0;
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
