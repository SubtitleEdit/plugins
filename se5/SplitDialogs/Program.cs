using SubtitleEdit.Plugins.Shared;
using System;

namespace SubtitleEdit.Plugins.SplitDialogs;

public static class Program
{
    [STAThread]
    public static int Main(string[] args) => PluginBootstrap.Run<App>(args);
}
