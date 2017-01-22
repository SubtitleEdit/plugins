using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class ReverseLines : IPlugin // dll file name must "<classname>.dll" - e.g. "Haxor.dll"
    {
        string IPlugin.Name
        {
            get { return "Reverse lines"; }
        }

        string IPlugin.Text
        {
            get { return "Reverse lines"; }
        }

        decimal IPlugin.Version
        {
            get { return 0.9M; }
        }

        string IPlugin.Description
        {
            get { return "Reverse context of each text line"; }
        }

        string IPlugin.ActionType // Can be one of these: file, tool, sync, translate, spellcheck
        {
            get { return "tool"; }
        }

        string IPlugin.Shortcut
        {
            get { return string.Empty; }
        }

        public static string ReverseStringUnicodeSafe(string input)
        {
            char[] output = new char[input.Length];
            for (int outputIndex = 0, inputIndex = input.Length - 1; outputIndex < input.Length; outputIndex++, inputIndex--)
            {
                // check for surrogate pair
                if (input[inputIndex] >= 0xDC00 && input[inputIndex] <= 0xDFFF &&
                    inputIndex > 0 && input[inputIndex - 1] >= 0xD800 && input[inputIndex - 1] <= 0xDBFF)
                {
                    // preserve the order of the surrogate pair code units
                    output[outputIndex + 1] = input[inputIndex];
                    output[outputIndex] = input[inputIndex - 1];
                    outputIndex++;
                    inputIndex--;
                }
                else
                {
                    output[outputIndex] = input[inputIndex];
                }
            }

            return new string(output);
        }

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            subtitle = subtitle.Trim();
            if (string.IsNullOrEmpty(subtitle))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }

            var list = subtitle.Replace(Environment.NewLine, "\n").Split('\n').ToList();

            var sub = new Subtitle();
            var srt = new SubRip();
            srt.LoadSubtitle(sub, list, subtitleFileName);

            foreach (var paragraph in sub.Paragraphs)
            {
                string pre = string.Empty;
                if (paragraph.Text.StartsWith("{\\") && paragraph.Text.Contains("}"))
                {
                    var idx = paragraph.Text.IndexOf("}", 2, StringComparison.Ordinal) + 1;
                    pre = paragraph.Text.Substring(0, idx);
                    paragraph.Text = paragraph.Text.Remove(0, idx);
                }
                var lines = paragraph.Text.SplitToLines();
                var sb = new StringBuilder(paragraph.Text.Length);
                foreach (var line in lines)
                {
                    sb.AppendLine(ReverseStringUnicodeSafe(line));
                }
                paragraph.Text = pre + sb.ToString().TrimEnd().Replace(">i<", "<i>").Replace(">i/<", "</i>");
            }

            return srt.ToText(sub, string.Empty);
        }

    }
}