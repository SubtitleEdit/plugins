using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SubtitleEdit.Plugins.Shared;

namespace SubtitleEdit.Plugins.BritishToAmerican;

public partial class App : PluginApp
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    protected override Window CreateMainWindow(PluginRequest request) => new MainWindow(request);
}
