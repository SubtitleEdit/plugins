using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using SubtitleEdit.Plugins.Shared;
using System.Diagnostics;

namespace SubtitleEdit.Plugins.PersianErrors;

public partial class MainWindow : Window
{
    private const string HomepageUrl = "https://github.com/msasanmh/PersianSubtitleFixes";
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

    /// <summary>Clicking anywhere on a group row toggles it, not only the switch.</summary>
    private void OnGroupPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is Control { DataContext: GroupOption group } source &&
            source.FindAncestorOfType<ToggleSwitch>(includeSelf: true) == null)
        {
            group.IsEnabled = !group.IsEnabled;
            e.Handled = true;
        }
    }

    private void OnOpenHomepage(object? sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(HomepageUrl) { UseShellExecute = true });
        }
        catch (Exception exception)
        {
            Debug.WriteLine("Could not open homepage: " + exception.Message);
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
            App.Response = new PluginResponse { Status = PluginStatus.Error, Message = "Persian Subtitle Fixes failed: " + exception.Message };
        }

        Close();
    }
}
