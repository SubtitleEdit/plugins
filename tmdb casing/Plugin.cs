using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.PluginLogic;
using System.Windows.Forms;

namespace tmdb_casing
{
    public class Plugin  : IPlugin
    {
        string IPlugin.Name
        {
            get { throw new NotImplementedException(); }
        }

        string IPlugin.Text
        {
            get { throw new NotImplementedException(); }
        }

        decimal IPlugin.Version
        {
            get { throw new NotImplementedException(); }
        }

        string IPlugin.Description
        {
            get { throw new NotImplementedException(); }
        }

        string IPlugin.ActionType
        {
            get { throw new NotImplementedException(); }
        }

        string IPlugin.Shortcut
        {
            get { throw new NotImplementedException(); }
        }

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            subtitle = subtitle.Trim();
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
                using (var form = new MainForm(sub, (this as IPlugin).Name, (this as IPlugin).Description, parentForm))
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
