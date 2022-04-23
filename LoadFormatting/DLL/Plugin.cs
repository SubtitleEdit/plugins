using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class LoadFormatting : IPlugin // dll file name must "<classname>.dll" - e.g. "Haxor.dll"
    {
        string IPlugin.Name => "Load formatting";

        string IPlugin.Text => "Load formatting from other subtitle...";

        decimal IPlugin.Version => 0.1M;

        string IPlugin.Description => "Load formatting from other subtitle";

        string IPlugin.ActionType => "tool"; // Can be one of these: file, tool, sync, translate, spellcheck

        string IPlugin.Shortcut => string.Empty;

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            subtitle = subtitle.Trim();
            if (string.IsNullOrEmpty(subtitle))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(listViewLineSeparatorString))
            {
                Configuration.ListViewLineSeparatorString = listViewLineSeparatorString;
            }

            var list = new List<string>();
            foreach (var line in subtitle.Replace(Environment.NewLine, "\n").Split('\n'))
            {
                list.Add(line);
            }

            var sub = new Subtitle();
            var srt = new SubRip();
            srt.LoadSubtitle(sub, list, subtitleFileName);

            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = "Open subtitle file...";
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = "Subtitle files|*.srt;*.pac";
                openFileDialog1.FileName = string.Empty;
                if (openFileDialog1.ShowDialog() != DialogResult.OK)
                {
                    return string.Empty;
                }

                var subWithFormatting = LoadSubtitleFile(openFileDialog1.FileName);
                if (subWithFormatting != null && subWithFormatting.Paragraphs.Count == sub.Paragraphs.Count)
                {
                    return SetFormatting(sub, subWithFormatting);
                }

                MessageBox.Show($"Number of subtitles does not match: {subWithFormatting?.Paragraphs.Count} vs {sub.Paragraphs.Count}");
            }
            
            return string.Empty;
        }

        private static string SetFormatting(Subtitle targetSub, Subtitle subWithFormatting)
        {
            for (int i = 0; i < subWithFormatting.Paragraphs.Count; i++)
            {
                var sourceText = subWithFormatting.Paragraphs[i].Text;
                var pre = string.Empty;
                var post = string.Empty;
                if (sourceText.StartsWith("{\\", StringComparison.Ordinal) && sourceText.Contains('}'))
                {
                    var assaTag = sourceText.Substring(0, sourceText.IndexOf('}') + 1);
                    pre += assaTag;
                    sourceText = sourceText.Remove(0, assaTag.Length);
                }

                for (var j = 0; j < 3; j++)
                {
                    if (sourceText.StartsWith("<i>") && sourceText.EndsWith("</i>"))
                    {
                        pre += "<i>";
                        sourceText = sourceText.Remove(0, "<i>".Length);
                        post = "</i>" + post;
                    }

                    if (sourceText.StartsWith("<b>") && sourceText.EndsWith("</b>"))
                    {
                        pre += "<b>";
                        sourceText = sourceText.Remove(0, "<b>".Length);
                        post = "</b>" + post;
                    }

                    if (sourceText.StartsWith("<u>") && sourceText.EndsWith("</u>"))
                    {
                        pre += "<u>";
                        sourceText = sourceText.Remove(0, "<u>".Length);
                        post = "</u>" + post;
                    }

                    if (sourceText.StartsWith("<box>") && sourceText.EndsWith("</box>"))
                    {
                        pre += "<box>";
                        sourceText = sourceText.Remove(0, "<box>".Length);
                        post = "</box>" + post;
                    }

                    if (sourceText.StartsWith("<font ", StringComparison.OrdinalIgnoreCase) && sourceText.EndsWith("</font>", StringComparison.OrdinalIgnoreCase))
                    {
                        var fontTag = sourceText.Substring(0, sourceText.IndexOf('>') + 1);
                        pre += fontTag;
                        sourceText = sourceText.Remove(0, fontTag.Length);
                        post = "</font>" + post;
                    }
                }

                if (sourceText != subWithFormatting.Paragraphs[i].Text)
                {
                    var targetP = targetSub.Paragraphs[i];
                    targetP.Text = pre + Utilities.RemoveHtmlTags(targetP.Text, true) + post;
                }
            }

            return new SubRip().ToText(targetSub, string.Empty);
        }

        private static Subtitle LoadSubtitleFile(string fileName)
        {
            if (fileName.EndsWith(".pac", StringComparison.OrdinalIgnoreCase))
            {
                var pac = new Pac();
                var s = new Subtitle();
                var l = File.ReadAllText(fileName).SplitToLines().ToList();
                pac.LoadSubtitle(s, l, fileName);
                return s;
            }

            var srt = new SubRip();
            var subWithFormatting = new Subtitle();
            var list = File.ReadAllText(fileName).SplitToLines().ToList();
            srt.LoadSubtitle(subWithFormatting, list, fileName);
            return subWithFormatting;
        }
    }
}