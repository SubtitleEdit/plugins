using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.MaterialDesign;
using SubtitleEdit.Plugins.LinesUnbreaker.ViewModels;
using SubtitleEdit.Plugins.Shared;
using System;

namespace SubtitleEdit.Plugins.LinesUnbreaker;

public static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        IconProvider.Current.Register<MaterialDesignIconProvider>();
        return PluginBootstrap.Run<App>(args, request => new MainViewModel(request).BuildBatchResponse());
    }
}
