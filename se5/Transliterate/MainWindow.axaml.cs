using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SubtitleEdit.Plugins.Shared;

namespace SubtitleEdit.Plugins.Transliterate;

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
            App.Response = new PluginResponse { Status = PluginStatus.Error, Message = "Transliterate failed: " + exception.Message };
        }

        Close();
    }
}
