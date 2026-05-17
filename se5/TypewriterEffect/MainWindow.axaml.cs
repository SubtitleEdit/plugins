using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Text.Json;

namespace SubtitleEdit.Plugins.TypewriterEffect;

public partial class MainWindow : Window
{
    private readonly PluginRequest _request;

    // Parameterless constructor only exists for the XAML designer.
    public MainWindow() : this(new PluginRequest()) { }

    public MainWindow(PluginRequest request)
    {
        InitializeComponent();
        _request = request;

        var selectedCount = request.SelectedIndices.Count;
        InfoLabel.Text = selectedCount > 0
            ? $"Each of the {selectedCount} selected line(s) will be split into several short lines that progressively reveal the text."
            : "Every line will be split into several short lines that progressively reveal the text.";

        EndDelayInput.Value = (decimal)LoadEndDelaySetting(request.Settings, defaultValue: 0.5);
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        App.Response = new PluginResponse { Status = "cancelled" };
        Close();
    }

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        var endDelaySec = (double)(EndDelayInput.Value ?? 0m);

        try
        {
            var blocks = SubRipParser.Parse(_request.Subtitle.SubRip);
            var selected = new HashSet<int>(_request.SelectedIndices);
            var (resultBlocks, changed) = TypewriterEngine.Apply(blocks, selected, endDelaySec);

            App.Response = new PluginResponse
            {
                Status = "ok",
                Message = changed == 0
                    ? "No lines changed."
                    : $"Typewriter applied to {changed} line(s).",
                UndoDescription = "Typewriter effect",
                Subtitle = new PluginSubtitle
                {
                    Format = "SubRip",
                    Native = SubRipParser.Serialize(resultBlocks),
                },
                Settings = BuildSettings(endDelaySec),
            };
        }
        catch (Exception ex)
        {
            App.Response = new PluginResponse { Status = "error", Message = ex.Message };
        }

        Close();
    }

    private static double LoadEndDelaySetting(JsonElement? settings, double defaultValue)
    {
        if (settings is null || settings.Value.ValueKind != JsonValueKind.Object)
        {
            return defaultValue;
        }
        if (settings.Value.TryGetProperty("endDelay", out var value) &&
            value.ValueKind == JsonValueKind.Number &&
            value.TryGetDouble(out var parsed))
        {
            return parsed;
        }
        return defaultValue;
    }

    private static JsonElement BuildSettings(double endDelay)
    {
        using var doc = JsonDocument.Parse($"{{\"endDelay\":{endDelay.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}}}");
        return doc.RootElement.Clone();
    }
}
