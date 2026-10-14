using Avalonia;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SubtitleEdit.Plugins.Shared;

/// <summary>
/// Entry-point glue for an SE5 plugin: parse the request JSON from the first
/// command-line argument, start Avalonia, write the response JSON when the
/// app exits, return 0. A "batch" request (Subtitle Edit's Batch convert) is handed to the
/// plugin's batch handler instead, without starting any UI.
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

    /// <param name="batchHandler">
    /// Handles a "batch" request headless: apply saved settings to all lines and return the
    /// response. Plugins that pass it should set <c>"batch": true</c> in plugin.json and also
    /// handle a "configure" request in their window (settings only, no subtitle in the response).
    /// </param>
    public static int Run<TApp>(string[] args, Func<PluginRequest, PluginResponse>? batchHandler = null) where TApp : Application, new()
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

        if (request.RequestType == PluginRequestType.Batch)
        {
            PluginResponse batchResponse;
            try
            {
                batchResponse = batchHandler?.Invoke(request)
                    ?? new PluginResponse { Status = PluginStatus.Error, Message = "This plugin does not support batch convert." };
            }
            catch (Exception exception)
            {
                batchResponse = new PluginResponse { Status = PluginStatus.Error, Message = exception.Message };
            }

            File.WriteAllText(request.ResponseFilePath, JsonSerializer.Serialize(batchResponse, JsonOpts));
            return 0;
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
