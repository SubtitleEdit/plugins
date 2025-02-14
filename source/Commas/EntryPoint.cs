using System.Diagnostics;
using Commas;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.PluginLogic;

/// <summary>
/// The Commas class is a plugin designed for the Subtitle Edit application.
/// Its purpose is to process and fix comma-related issues in subtitle files.
/// </summary>
public class Commas : IPlugin
{
    public string Name => "Commas";
    public string Text => "Commas";
    public decimal Version => 1m;
    public string Description => "Fixes commas via LM Studio";
    public string ActionType => "tool";
    public string Shortcut => string.Empty;

    public Commas()
    {
        // AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        // {
        //     _ = AppDomain.CurrentDomain.GetAssemblies();
        //     return null;
        // };
    }

    public string DoAction(Form parentForm, string srtText, double frameRate, string uiLineBreak, string file, string videoFile, string rawText)
    {
#if DEBUG
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif

        var subtitle = Subtitle.Parse(srtText.SplitToLines(), ".srt");
        using var mainForm = new Main(subtitle);

        if (mainForm.ShowDialog() == DialogResult.OK)
        {
            return subtitle.ToText(new SubRip());
        }

        return "";
    }
}