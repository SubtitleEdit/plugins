using AssaDraw;
using SubtitleEdit.Logic;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class AssaDraw : IPlugin // dll file name must "<classname>.dll" - e.g. "Haxor.dll"
    {
        string IPlugin.Name => "ASSA Draw";

        string IPlugin.Text => "ASSA Draw..."; // will be used in context menu item

        decimal IPlugin.Version => 0.24M;

        string IPlugin.Description => "Draw for Advanced Sub Station Alpha";

        string IPlugin.ActionType => "AssaTool"; // Can be one of these: File, Tool, Sync, Translate, SpellCheck, AssaTool

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
            if (string.IsNullOrEmpty(sub.Header))
            {
                sub.Header = AdvancedSubStationAlpha.DefaultHeader;
            }

            Paragraph firstParagraph = null;
            var selectedLines = AdvancedSubStationAlpha.GetTag("SelectedLines", "[Script Info]", sub.Header);
            var selectedParagraphs = new List<Paragraph>();
            if (!string.IsNullOrEmpty(selectedLines))
            {
                var arr = selectedLines.Split(new char[] { ',', ':' }, System.StringSplitOptions.RemoveEmptyEntries);
                for (int i = 1; i < arr.Length; i++)
                {
                    if (int.TryParse(arr[i], out var index))
                    {
                        var p = sub.GetParagraphOrDefault(index);
                        if (p != null)
                        {
                            selectedParagraphs.Add(p);
                            if (firstParagraph == null)
                            {
                                firstParagraph = p;
                            }
                        }
                    }
                }
            }

            var videoPosition = AdvancedSubStationAlpha.GetTag("VideoFilePositionMs", "[Script Info]", sub.Header);
            if (!string.IsNullOrEmpty(videoPosition))
            {
                var arr = videoPosition.Split(new char[] { ':' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length == 2 && long.TryParse(arr[1], out var ms))
                {
                    videoPosition = new TimeCode(ms).ToString(".");
                }
                else
                {
                    videoPosition = string.Empty;
                }
            }


            var tempSub = new Subtitle(selectedParagraphs) { Header = sub.Header };
            var text = new AdvancedSubStationAlpha().ToText(tempSub, string.Empty);

            using (var form = new FormAssaDrawMain(subtitle == "standalone" ? "standalone" : text, videoFileName, videoPosition, subtitleFileName))
            {
                if (form.ShowDialog(parentForm) == DialogResult.OK)
                {
                    if (firstParagraph != null && form.AssaDrawCodes.Paragraphs.Count > 0)
                    {
                        for (int i = 1; i < selectedParagraphs.Count; i++)
                        {
                            sub.Paragraphs.Remove(selectedParagraphs[i]);
                        }

                        firstParagraph.Text = form.AssaDrawCodes.Paragraphs[0].Text;
                        firstParagraph.Layer = form.AssaDrawCodes.Paragraphs[0].Layer;

                        firstParagraph.Extra = "AssaDraw";
                        var styles = AdvancedSubStationAlpha.GetStylesFromHeader(sub.Header);
                        if (!styles.Contains(firstParagraph.Extra))
                        {
                            var assaDrawStyle = new SsaStyle
                            {
                                Name = firstParagraph.Extra,
                                Alignment = "7",
                                MarginVertical = 0,
                                MarginLeft = 0,
                                MarginRight = 0,
                                ShadowWidth = 0,
                                OutlineWidth = 0,
                            };
                            sub.Header = AdvancedSubStationAlpha.AddSsaStyle(assaDrawStyle, sub.Header);
                        }

                        var idx = sub.Paragraphs.IndexOf(firstParagraph);
                        for (var i = 1; i < form.AssaDrawCodes.Paragraphs.Count; i++)
                        {
                            var newP = new Paragraph(firstParagraph)
                            {
                                Text = form.AssaDrawCodes.Paragraphs[i].Text,
                                Layer = form.AssaDrawCodes.Paragraphs[i].Layer
                            };
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