using Avalonia.Styling;
using SubtitleEdit.Plugins.Shared;

namespace AmericanToBritish.Tests;

/// <summary>Plugin-Shared's theme variant choice, shared by every plugin built on PluginApp.</summary>
public class PluginAppTests
{
    [Theory]
    [InlineData("Nord", true, "Dark")]
    [InlineData("System", true, "Dark")]
    [InlineData("Dark", false, "Light")]
    [InlineData("Light", false, "Light")]
    public void ThemeColors_IsDark_Wins(string theme, bool isDark, string expected)
    {
        var request = new PluginRequest { Theme = theme, ThemeColors = new PluginThemeColors { IsDark = isDark } };
        Assert.Equal(expected, PluginApp.ResolveThemeVariant(request).Key);
    }

    [Theory]
    [InlineData("Dark", "Dark")]
    [InlineData("dark", "Dark")]
    [InlineData("Nord", "Light")]
    [InlineData("", "Light")]
    public void NoThemeColors_FallsBackToName(string theme, string expected) =>
        Assert.Equal(expected, PluginApp.ResolveThemeVariant(new PluginRequest { Theme = theme }).Key);
}
