using System;
using System.Globalization;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic.Models
{
    public class RemoveLineBreakResult
    {
        public Paragraph Paragraph { get; set; }
        public string BeforeText => Paragraph.Text;
        public string AfterText { get; set; }
    }

    public static class RemoveLineBreakResultExtensions
    {
        public static ListViewItem ToListViewItem(this RemoveLineBreakResult result)
        {
            var noTagOldText = HtmlUtils.RemoveTags(result.Paragraph.Text);
            // length of only visible characters
            var lineLength = noTagOldText.Length - StringUtils.CountTagInText(noTagOldText, Environment.NewLine) *
                Environment.NewLine.Length;

            var item = new ListViewItem(string.Empty)
            {
                Checked = true,
                UseItemStyleForSubItems = true,
                SubItems =
                {
                    result.Paragraph.Number.ToString(),
                    lineLength.ToString(CultureInfo.InvariantCulture),
                    StringUtils.GetListViewString(result.BeforeText, true),
                    StringUtils.GetListViewString(result.AfterText, true)
                },
                Tag = result
            };

            return item;
        }
    }
}