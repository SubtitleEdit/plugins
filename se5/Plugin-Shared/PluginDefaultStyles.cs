using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Styling;

namespace SubtitleEdit.Plugins.Shared;

/// <summary>
/// Theme-independent visual defaults applied to every SE5 plugin window
/// (e.g. centered button text). Run by <see cref="PluginApp"/> at startup.
/// </summary>
public static class PluginDefaultStyles
{
    public static void Apply()
    {
        if (Application.Current is null)
        {
            return;
        }

        Application.Current.Styles.Add(new Styles
        {
            new Style(x => x.OfType<Button>())
            {
                Setters =
                {
                    new Setter(ContentControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Center),
                    new Setter(ContentControl.VerticalContentAlignmentProperty, VerticalAlignment.Center),
                },
            },
        });
    }
}
