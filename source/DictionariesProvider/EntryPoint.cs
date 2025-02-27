using Nikse.SubtitleEdit.PluginLogic.Forms;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class DictionariesProvider : IPlugin
    {
        public string Name => "Dictionaries provider";

        public string Text => Name;

        public decimal Version => 1m;

        public string Description => "Download and install dictionaries (by Ivandrofly)";

        public string ActionType => "spellcheck";

        public string Shortcut => string.Empty;

        public string DoAction(Form parentForm, string srtText, double frameRate, string uiLineBreak, string file, string videoFile, string rawText)
        {
            // AppDomain.CurrentDomain.AssemblyResolve += AssemblyUtils.CurrentDomainAssemblyResolve;

            using (var mainForm = new Main())
            {
                mainForm.Text = $"{Name} - v{Version:0.0}";
                mainForm.StartPosition = FormStartPosition.CenterParent;
                mainForm.ShowDialog(parentForm);
            }

            return string.Empty;
        }
    }

    public interface IPlugin
    {
        string Name { get; }
        string Text { get; }
        decimal Version { get; }
        string Description { get; }
        string ActionType { get; }
        string Shortcut { get; }
        string DoAction(Form parentForm, string srtText, double frameRate, string uiLineBreak, string file, string videoFile, string rawText);
    }
}