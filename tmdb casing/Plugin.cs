using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.PluginLogic;
using System.Windows.Forms;

namespace tmdb_casing
{
    public class TMDBCASING : IPlugin
    {
        string IPlugin.Name
        {
            get { return "TMDBCASING"; }
        }

        string IPlugin.Text
        {
            get { return "TMDBCASING"; }
        }

        decimal IPlugin.Version
        {
            get { return 0.1M; }
        }

        string IPlugin.Description
        {
            get { return "Change names chansing by TMDB"; }
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
