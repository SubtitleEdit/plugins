using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Globalization;

namespace SubtitleEdit.Plugins.TypewriterEffect;

/// <summary>
/// Turns the <see cref="PluginThemeColors"/> sent by Subtitle Edit into Avalonia
/// <see cref="Styles"/> so the plugin's window looks like an SE window.
/// Mirrors <c>UiTheme.ApplyLighterDark</c> in Subtitle Edit.
/// </summary>
public static class PluginThemeStyles
{
    public static void Apply(PluginThemeColors? colors)
    {
        if (colors is null || Application.Current is null)
        {
            return;
        }

        var bg = ParseHex(colors.BackgroundColor);
        var fg = ParseHex(colors.ForegroundColor);
        if (bg is null || fg is null)
        {
            return;
        }

        var bgLighter = ParseHex(colors.BackgroundColorLighter) ?? bg.Value;
        var bgBrush = new SolidColorBrush(bg.Value);
        var fgBrush = new SolidColorBrush(fg.Value);
        var bgLighterBrush = new SolidColorBrush(bgLighter);

        if (colors.IsDark)
        {
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                ["TextControlForeground"] = fgBrush,
                ["TextControlForegroundPointerOver"] = fgBrush,
                ["TextControlForegroundFocused"] = fgBrush,
                ["TextControlForegroundDisabled"] = fgBrush,
            });
        }

        Application.Current.Styles.Add(new Styles
        {
            new Style(x => x.OfType<Window>())
            {
                Setters =
                {
                    new Setter(Window.BackgroundProperty, bgBrush),
                    new Setter(Window.ForegroundProperty, fgBrush),
                },
            },
            new Style(x => x.OfType<TextBox>())
            {
                Setters =
                {
                    new Setter(TextBox.BackgroundProperty, bgBrush),
                    new Setter(TextBox.ForegroundProperty, fgBrush),
                },
            },
            new Style(x => x.OfType<TextBox>().Class(":focus").Template().OfType<Border>().Name("PART_BorderElement"))
            {
                Setters = { new Setter(Border.BackgroundProperty, bgBrush) },
            },
            new Style(x => x.OfType<TextBox>().Class(":pointerover").Template().OfType<Border>().Name("PART_BorderElement"))
            {
                Setters = { new Setter(Border.BackgroundProperty, bgLighterBrush) },
            },
            new Style(x => x.OfType<Button>())
            {
                Setters = { new Setter(Button.ForegroundProperty, fgBrush) },
            },
            new Style(x => x.OfType<NumericUpDown>())
            {
                Setters =
                {
                    new Setter(NumericUpDown.BackgroundProperty, bgBrush),
                    new Setter(NumericUpDown.ForegroundProperty, fgBrush),
                },
            },
            new Style(x => x.OfType<ComboBox>())
            {
                Setters = { new Setter(ComboBox.ForegroundProperty, fgBrush) },
            },
            new Style(x => x.OfType<CheckBox>())
            {
                Setters = { new Setter(CheckBox.ForegroundProperty, fgBrush) },
            },
            new Style(x => x.OfType<RadioButton>())
            {
                Setters = { new Setter(RadioButton.ForegroundProperty, fgBrush) },
            },
            new Style(x => x.OfType<ListBox>())
            {
                Setters = { new Setter(ListBox.ForegroundProperty, fgBrush) },
            },
            new Style(x => x.OfType<Label>())
            {
                Setters = { new Setter(Label.ForegroundProperty, fgBrush) },
            },
            new Style(x => x.OfType<TextBlock>())
            {
                Setters = { new Setter(TextBlock.ForegroundProperty, fgBrush) },
            },
            new Style(x => x.OfType<MenuItem>())
            {
                Setters = { new Setter(MenuItem.ForegroundProperty, fgBrush) },
            },
            new Style(x => x.OfType<ContextMenu>())
            {
                Setters =
                {
                    new Setter(TemplatedControl.BackgroundProperty, bgBrush),
                    new Setter(TemplatedControl.ForegroundProperty, fgBrush),
                },
            },
            new Style(x => x.OfType<ButtonSpinner>())
            {
                Setters =
                {
                    new Setter(ButtonSpinner.BackgroundProperty, bgBrush),
                    new Setter(ButtonSpinner.ForegroundProperty, fgBrush),
                },
            },
        });
    }

    private static Color? ParseHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return null;
        }

        var s = hex.StartsWith("#", StringComparison.Ordinal) ? hex.Substring(1) : hex;
        try
        {
            if (s.Length == 6)
            {
                return Color.FromArgb(
                    255,
                    byte.Parse(s.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                    byte.Parse(s.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                    byte.Parse(s.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
            }
            if (s.Length == 8)
            {
                return Color.FromArgb(
                    byte.Parse(s.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                    byte.Parse(s.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                    byte.Parse(s.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                    byte.Parse(s.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
            }
        }
        catch (FormatException) { }
        catch (OverflowException) { }

        return null;
    }
}
