using SubtitleEdit.Plugins.Shared;
using System;

namespace SubtitleEdit.Plugins.BritishToAmerican;

public static class Program
{
    [STAThread]
    public static int Main(string[] args) => PluginBootstrap.Run<App>(args);
}
