using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic.LinesUnbreaker
{
    public class LinesUnbreaker : IPlugin
    {
        string IPlugin.Name
        {
            get { return "Lines Unbreaker"; }
        }

        string IPlugin.Text
        {
            get { return "Lines Unbreaker"; }
        }

        decimal IPlugin.Version
        {
            get { return 1.1M; }
        }

        string IPlugin.Description
        {
            get { return "Helps breaking short lines"; }
        }

        string IPlugin.ActionType
        {
            get { return "tool"; }
        }

        string IPlugin.Shortcut
        {
            get { return string.Empty; }
        }

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            subtitle = subtitle.Trim(); // if subtitle was send null this will rise exception!
            if (!string.IsNullOrEmpty(subtitle))
            {
                if (!string.IsNullOrEmpty(listViewLineSeparatorString))
                    Configuration.ListViewLineSeparatorString = listViewLineSeparatorString;

                var list = new List<string>();
                foreach (string line in subtitle.Replace(Environment.NewLine, "|").Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    list.Add(line);

                Subtitle sub = new Subtitle();
                SubRip srt = new SubRip();
                srt.LoadSubtitle(sub, list, subtitleFileName);
                using (var form = new PluginForm(sub, (this as IPlugin).Name, (this as IPlugin).Description, parentForm))
                {
                    if (form.ShowDialog(parentForm) == DialogResult.OK)
                        return form.FixedSubtitle;
                }
                return string.Empty;
            }

            MessageBox.Show("No subtitle loaded", parentForm.Text,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return string.Empty;
        }
    }
}
