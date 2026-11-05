using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace SubtitleEdit.Plugins.TypewriterEffect;

public partial class App : Application
{
    /// <summary>Set by Program.Main before Avalonia starts.</summary>
    public static PluginRequest? PendingRequest;

    /// <summary>Filled in by MainWindow when the user clicks OK or Cancel.</summary>
    public static PluginResponse? Response;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        PluginDefaultStyles.Apply();

        if (PendingRequest is not null)
        {
            // IsDark covers every dark theme and "System" on a dark OS; older SE versions only send the name.
            RequestedThemeVariant = PendingRequest.ThemeColors is { } colors
                ? colors.IsDark ? ThemeVariant.Dark : ThemeVariant.Light
                : string.Equals(PendingRequest.Theme, "Dark", System.StringComparison.OrdinalIgnoreCase)
                    ? ThemeVariant.Dark
                    : ThemeVariant.Light;

            PluginThemeStyles.Apply(PendingRequest.ThemeColors);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow(PendingRequest);
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
