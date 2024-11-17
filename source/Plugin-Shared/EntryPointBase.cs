using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public abstract class EntryPointBase : IPlugin
    {
        public string Name { get; }
        public string Text { get; }
        public decimal Version { get; }
        public string Description { get; }
        public string ActionType { get; }
        public string Shortcut { get; }

        private IList<string> _lines;
        protected Subtitle _subtitle;

        protected EntryPointBase(string name, string text, decimal version, string description, string actionType, string shortcut)
        {
            Name = name;
            Text = text;
            Version = version;
            Description = description;
            ActionType = actionType;
            Shortcut = shortcut;
        }

        public abstract string DoAction(Form parentForm, string srtText, double frameRate, string uiLineBreak, string file, string videoFile, string rawText);

        public void Init(string srtText, string uiLineBreak, string file)
        {
            if (!string.IsNullOrEmpty(uiLineBreak))
            {
                Options.UILineBreak = uiLineBreak;
            }

            _lines = new List<string>(srtText.SplitToLines());
            var subrip = new SubRip();
            _subtitle = new Subtitle(subrip);
            subrip.LoadSubtitle(_subtitle, _lines, file);
        }
    }
}
