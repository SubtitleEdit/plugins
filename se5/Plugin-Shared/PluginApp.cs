using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using System;

namespace SubtitleEdit.Plugins.Shared;

/// <summary>
/// Avalonia <see cref="Application"/> base for SE5 plugins.
/// Reads the request's theme, sets <see cref="Application.RequestedThemeVariant"/>
/// to match Subtitle Edit, and hands the request to <see cref="CreateMainWindow"/>.
/// </summary>
public abstract class PluginApp : Application
{
    /// <summary>Set by <see cref="PluginBootstrap.Run{TApp}"/> before Avalonia starts.</summary>
    public static PluginRequest? PendingRequest;

    /// <summary>Filled in by the plugin's window when the user clicks OK/Cancel/Apply.</summary>
    public static PluginResponse? Response;

    public override void OnFrameworkInitializationCompleted()
    {
        if (PendingRequest is not null)
        {
            RequestedThemeVariant = ResolveThemeVariant(PendingRequest.Theme);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = CreateMainWindow(PendingRequest);
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>Map the SE theme string ("Dark" / "Light" / "System" / ...) to an Avalonia <see cref="ThemeVariant"/>.</summary>
    public static ThemeVariant ResolveThemeVariant(string? seThemeName)
    {
        return string.Equals(seThemeName, "Dark", StringComparison.OrdinalIgnoreCase)
            ? ThemeVariant.Dark
            : ThemeVariant.Light;
    }

    protected abstract Window CreateMainWindow(PluginRequest request);
}
