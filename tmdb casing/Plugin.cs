using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class TmdbCasing : IPlugin
    {
        string IPlugin.Name
        {
            get { return "TmdbCasing"; }
        }

        string IPlugin.Text
        {
            get { return "TmdbCasing"; }
        }

        decimal IPlugin.Version
        {
            get { return 0.1M; }
        }

        string IPlugin.Description
        {
            get { return "Change TmdbCasing using TMDB"; }
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
                try
                {
                    using (var form = new MovieSeacher())
                    {
                        if (form.ShowDialog(parentForm) == DialogResult.OK || form.Characters.Count > 0)
                        {
                            using (var mForm = new MainForm(sub, (this as IPlugin).Name, (this as IPlugin).Description, parentForm, form.Characters))
                            {
                                if (mForm.ShowDialog() == DialogResult.OK)
                                {
                                    return mForm.FixedSubtitle;
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    
                    throw;
                }
                return string.Empty;
            }
            MessageBox.Show("No subtitle loaded", parentForm.Text,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return string.Empty;
        }
    }
}