using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Styling;

namespace SubtitleEdit.Plugins.TypewriterEffect;

/// <summary>
/// Theme-independent visual defaults applied to the plugin window
/// (e.g. centered button text).
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
