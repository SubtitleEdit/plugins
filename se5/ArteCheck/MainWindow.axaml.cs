using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using SubtitleEdit.Plugins.Shared;

namespace SubtitleEdit.Plugins.ArteCheck;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;

    public MainWindow() : this(new PluginRequest()) { }

    public MainWindow(PluginRequest request)
    {
        AvaloniaXamlLoader.Load(this);
        _vm = new MainViewModel(request);
        DataContext = _vm;
        App.Response = _vm.BuildCancelResponse();
        KeyDown += OnKeyDown;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        this.BringToForeground();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            OnCancel(sender, e);
        }
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        App.Response = _vm.BuildCancelResponse();
        Close();
    }

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        try
        {
            App.Response = _vm.BuildResponse();
        }
        catch (Exception exception)
        {
            App.Response = new PluginResponse { Status = PluginStatus.Error, Message = "ARTE check failed: " + exception.Message };
        }

        Close();
    }

    private async void OnSaveReport(object? sender, RoutedEventArgs e)
    {
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save ARTE report",
            SuggestedFileName = "arte_report.txt",
            DefaultExtension = "txt",
            FileTypeChoices = new[] { new FilePickerFileType("Text") { Patterns = new[] { "*.txt" } } },
        });

        if (file == null)
        {
            return;
        }

        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(_vm.BuildReport());
    }
}
