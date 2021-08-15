using System.Windows.Forms;
using SubtitleEdit.Logic;
using AssaDraw;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class AssaDraw : IPlugin // dll file name must "<classname>.dll" - e.g. "Haxor.dll"
    {
        string IPlugin.Name => "ASSA Draw";

        string IPlugin.Text => "ASSA Draw..."; // will be used in context menu item

        decimal IPlugin.Version => 0.4M;

        string IPlugin.Description => "Draw shapes for Advanced Sub Station Alpha";

        // Can be one of these: File, Tool, Sync, Translate, SpellCheck, AssaTool
        string IPlugin.ActionType => "AssaTool";

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

            var sub = new Subtitle();
            var assa = new AdvancedSubStationAlpha();
            assa.LoadSubtitle(sub, rawText.SplitToLines(), subtitleFileName);
            var text = string.Empty;
            int layer = 0;
            Paragraph p = null;
            if (string.IsNullOrEmpty(sub.Header))
            {
                sub.Header = AdvancedSubStationAlpha.DefaultHeader;
            }

            var selectedLines = AdvancedSubStationAlpha.GetTag("SelectedLines", "[Script Info]", sub.Header);
            if (!string.IsNullOrEmpty(selectedLines))
            {
                var arr = selectedLines.Split(new char[] { ',', ':' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length > 1 && int.TryParse(arr[1], out var index))
                {
                    p = sub.GetParagraphOrDefault(index);
                    if (p != null)
                    {
                        text = p.Text;
                        layer = p.Layer;
                    }
                }
            }

            var width = 0;
            var playResX = AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResX", "[Script Info]", sub.Header);
            if (int.TryParse(playResX, out var w))
            {
                width = w;
            }

            var height = 0;
            var playResY = AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResY", "[Script Info]", sub.Header);
            if (int.TryParse(playResY, out var h))
            {
                height = h;
            }

            using (var form = new FormAssaDrawMain(text, layer, width, height))
            {
                if (form.ShowDialog(parentForm) == DialogResult.OK)
                {
                    if (p != null && form.AssaDrawCodes.Paragraphs.Count > 0)
                    {
                        p.Text = form.AssaDrawCodes.Paragraphs[0].Text;
                        p.Extra = "AssaDraw";
                        var styles = AdvancedSubStationAlpha.GetStylesFromHeader(sub.Header);
                        if (!styles.Contains(p.Extra))
                        {
                            var assaDrawStyle = new SsaStyle
                            {
                                Name = p.Extra,
                                Alignment = "7",
                                MarginVertical = 0,
                                MarginLeft = 0,
                                MarginRight = 0
                            };
                            sub.Header = AdvancedSubStationAlpha.AddSsaStyle(assaDrawStyle, sub.Header);
                        }

                        var idx = sub.Paragraphs.IndexOf(p);
                        for (int i = 1; i < form.AssaDrawCodes.Paragraphs.Count; i++)
                        {
                            var newP = new Paragraph(p);
                            p.Text = form.AssaDrawCodes.Paragraphs[i].Text;
                            newP.Layer = form.AssaDrawCodes.Paragraphs[i].Layer;
                            sub.Paragraphs.Insert(idx + i - 1, newP);
                        }
                    }

                    return sub.ToText(assa);
                }
            }

            return string.Empty;
        }
    }
}