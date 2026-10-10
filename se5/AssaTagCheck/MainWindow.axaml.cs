using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using SubtitleEdit.Plugins.Shared;

namespace SubtitleEdit.Plugins.AssaTagCheck;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;

    public MainWindow() : this(new PluginRequest()) { }

    public MainWindow(PluginRequest request)
    {
        AvaloniaXamlLoader.Load(this);
        _vm = new MainViewModel(request, IsFontInstalled());
        DataContext = _vm;
        App.Response = _vm.BuildCancelResponse();
        KeyDown += OnKeyDown;
    }

    /// <summary>Font family names installed on this computer.</summary>
    private static Func<string, bool> IsFontInstalled()
    {
        try
        {
            var installed = FontManager.Current.SystemFonts.Select(f => f.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            return installed.Contains;
        }
        catch (Exception)
        {
            return _ => true; // can't tell - don't report fonts as missing
        }
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

    /// <summary>Clicking a rule row toggles it, not only the switch; clicking its count shows only its lines.</summary>
    private void OnRulePressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is not Control { DataContext: RuleOption rule } source ||
            source.FindAncestorOfType<ToggleSwitch>(includeSelf: true) != null)
        {
            return;
        }

        if (source.GetSelfAndVisualAncestors().OfType<Border>().Any(b => b.Classes.Contains("count")))
        {
            _vm.FilterByRule(rule);
            e.Handled = true;
            return;
        }

        rule.IsEnabled = !rule.IsEnabled;
        e.Handled = true;
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
            App.Response = new PluginResponse { Status = PluginStatus.Error, Message = "ASSA tag check failed: " + exception.Message };
        }

        Close();
    }
}
