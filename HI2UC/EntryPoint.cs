using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic;

// dll file name must "<classname>.dll" - e.g. "Haxor.dll"
public class HI2UC : IPlugin
{
    public string ActionType => "tool";

    public string Description => "Convert Hearing Impaired text to Uppercase";

    public string Name => "HI2UC";

    public string Shortcut => string.Empty;

    // the text that will be displaying when subtitle edit context-menu
    public string Text => "Hearing Impaired to Uppercase";

    public decimal Version => 4.6m;

    public string DoAction(Form parentForm, string srtText, double frame, string uiLineBreak, string file,
        string videoFile, string rawText)
    {
        
#if DEBUG
        /// <summary>
        /// Launch and attach a debugger to the process if it's not already attached.
        /// This is helpful when a need arises to debug code in the startup sequence of an application
        /// or to debug issues that occur when a debugger isn't already attached.
        /// </summary>
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif
        // Make sure subtitle isn't null or empty
        if (string.IsNullOrWhiteSpace(srtText))
        {
            MessageBox.Show("No subtitle loaded", parentForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return string.Empty;
        }

        // Use custom separator for list view new lines
        if (!string.IsNullOrEmpty(uiLineBreak))
        {
            Options.UILineBreak = uiLineBreak;
        }

        // Get subtitle raw lines
        var list = new List<string>(srtText.SplitToLines());
        var srt = new SubRip();
        var sub = new Subtitle(srt);

        // Load raws subtitle lines into Subtitle object
        srt.LoadSubtitle(sub, list, file);

        IPlugin hi2Uc = this;
        using (var form = new PluginForm(sub, hi2Uc.Name, hi2Uc.Description))
        {
            if (form.ShowDialog(parentForm) == DialogResult.OK)
            {
                return form.Subtitle;
            }
        }

        return string.Empty;
    }

}