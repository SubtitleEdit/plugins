using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace SubtitleEdit.Plugins.TypewriterEffect;

public partial class MainWindow : Window
{
    private readonly PluginRequest _request;
    private TextBlock _infoLabel = null!;
    private NumericUpDown _endDelayInput = null!;
    private Control _batchSettingsPanel = null!;

    // Parameterless constructor only exists for the XAML designer.
    public MainWindow() : this(new PluginRequest()) { }

    public MainWindow(PluginRequest request)
    {
        _request = request;
        InitializeComponent();

        if (TypewriterResponses.IsConfigure(request))
        {
            // Opened from Batch convert's Configure button: there is no subtitle, only the setting.
            _infoLabel.IsVisible = false;
            _batchSettingsPanel.IsVisible = true;
        }
        else
        {
            var selectedCount = request.SelectedIndices.Count;
            _infoLabel.Text = selectedCount > 0
                ? $"Each of the {selectedCount} selected line(s) will be split into several short lines that progressively reveal the text."
                : "Every line will be split into several short lines that progressively reveal the text.";
        }

        _endDelayInput.Value = (decimal)TypewriterResponses.LoadEndDelaySetting(request.Settings, TypewriterResponses.DefaultEndDelay);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _infoLabel = this.FindControl<TextBlock>("InfoLabel")!;
        _endDelayInput = this.FindControl<NumericUpDown>("EndDelayInput")!;
        _batchSettingsPanel = this.FindControl<Control>("BatchSettingsPanel")!;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        // The plugin is a fresh process with no parent window relationship to
        // Subtitle Edit, so on macOS/Linux/sometimes Windows it can appear
        // behind SE. Toggling Topmost briefly forces the window to the top of
        // the z-order, and Activate() gives it focus.
        Topmost = true;
        Activate();
        Topmost = false;
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        App.Response = new PluginResponse { Status = "cancelled" };
        Close();
    }

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        var endDelaySec = (double)(_endDelayInput.Value ?? 0m);
        App.Response = TypewriterResponses.IsConfigure(_request)
            ? TypewriterResponses.BuildConfigureResponse(endDelaySec)
            : TypewriterResponses.BuildRunResponse(_request, endDelaySec);
        Close();
    }
}
