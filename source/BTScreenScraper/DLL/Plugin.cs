using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WebViewTranslate;
using WebViewTranslate.Logic;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class BTScreenScraper : IPlugin // dll file name must "<classname>.dll" - e.g. "Haxor.dll"
    {
        string IPlugin.Name => "BT Screen-Scraper";

        string IPlugin.Text => "Translate with Bing screen-scraping...";

        decimal IPlugin.Version => 0.7M;

        string IPlugin.Description => "BT screen-scraper: .net 4.6.2+";

        string IPlugin.ActionType => "translate";  // Can be one of these: file, tool, sync, translate, spellcheck

        string IPlugin.Shortcut => string.Empty;

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            subtitle = subtitle.Trim();
            if (string.IsNullOrEmpty(subtitle))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(listViewLineSeparatorString))
                Configuration.ListViewLineSeparatorString = listViewLineSeparatorString;

            var list = new List<string>();
            foreach (string line in subtitle.Replace(Environment.NewLine, "\n").Split('\n'))
                list.Add(line);

            var sub = new Subtitle();
            var srt = new SubRip();
            srt.LoadSubtitle(sub, list, subtitleFileName);
            using (var form = new MainForm(sub, (this as IPlugin).Text, (this as IPlugin).Description, parentForm))
            {
                if (form.ShowDialog(parentForm) == DialogResult.OK)
                    return form.FixedSubtitle;
            }
            return string.Empty;
        }
    }
}