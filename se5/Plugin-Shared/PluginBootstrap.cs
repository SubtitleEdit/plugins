using Avalonia;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SubtitleEdit.Plugins.Shared;

/// <summary>
/// Entry-point glue for an SE5 plugin: parse the request JSON from the first
/// command-line argument, start Avalonia, write the response JSON when the
/// app exits, return 0.
/// </summary>
public static class PluginBootstrap
{
    public static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
    };

    public static int Run<TApp>(string[] args) where TApp : Application, new()
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine($"Usage: {AppDomain.CurrentDomain.FriendlyName} <requestFilePath>");
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

        PluginApp.PendingRequest = request;
        BuildAvaloniaApp<TApp>().StartWithClassicDesktopLifetime(Array.Empty<string>());

        var response = PluginApp.Response ?? new PluginResponse { Status = PluginStatus.Cancelled };
        File.WriteAllText(request.ResponseFilePath, JsonSerializer.Serialize(response, JsonOpts));
        return 0;
    }

    public static AppBuilder BuildAvaloniaApp<TApp>() where TApp : Application, new() =>
        AppBuilder.Configure<TApp>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
