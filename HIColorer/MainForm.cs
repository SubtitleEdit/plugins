using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public partial class MainForm : Form
    {
        public string FixedSubtitle { get; private set; }

        private Color _narratorColor = Color.Empty;
        private Color _moodsColor = Color.Empty;
        private Subtitle _subtitle;

        public MainForm()
        {
            InitializeComponent();
        }

        internal MainForm(Subtitle sub, string name, string ver)
            : this()
        {
            this._subtitle = sub;
            this.Text = "Hearing Impaired Colorer";
            GetSetting();
        }

        private void buttonNarratorColor_Click(object sender, EventArgs e)
        {
            SetColor(sender);
        }


        private void buttonMoodsColor_Click(object sender, EventArgs e)
        {
            SetColor(sender);
        }

        private void SetColor(object sender)
        {
            if (sender == null) return;
            if (sender == buttonNarratorColor || sender == labelNarratorsColor)
            {
                if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _narratorColor = colorDialog1.Color;
                    this.labelNarratorsColor.BackColor = _narratorColor;
                    this.labelNarratorsColor.Text = Utilities.GetHtmlColorCode(_narratorColor);
                }
            }
            else
            {
                if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _moodsColor = colorDialog1.Color;
                    this.labelMoodsColor.BackColor = _moodsColor;
                    // TODO: When the backcolor is to drak/lighty fix the forecolor
                    this.labelMoodsColor.Text = Utilities.GetHtmlColorCode(_moodsColor);
                }
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count == 0)
                return;
            string text = string.Empty;
            string oldText = string.Empty;
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                if (!p.Text.Contains("<font"))
                    continue;
                text = p.Text;
                oldText = text;
                text = Utilities.RemoveHtmlColorTags(text);
                if (!text.Equals(oldText))
                {
                    p.Text = text;
                }
            }
            FixedSubtitle = _subtitle.ToText(new SubRip());
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (!checkBoxEnabledMoods.Checked && !checkBoxEnabledNarrator.Checked)
                DialogResult = System.Windows.Forms.DialogResult.Cancel;
            FindHearingImpairedNotation();
            FixedSubtitle = _subtitle.ToText(new SubRip());
            if (string.IsNullOrEmpty(FixedSubtitle))
                DialogResult = System.Windows.Forms.DialogResult.Cancel;
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void FindHearingImpairedNotation()
        {
            if (_subtitle == null || _subtitle.Paragraphs.Count == 0)
                return;
            Func<string , char, char, string> brackesType = (text, bOpen, bClose) =>
            {
                int startBraces = text.IndexOf(bOpen);
                if (startBraces > -1)
                {
                    int endBraces = text.IndexOf(bClose, startBraces + 1);
                    if (endBraces < startBraces + 1)
                    {
                        // don't continue this if text contains something like <(i> and - Ivandro: hello world! it won't change the narrator
                        return text;
                    }
                    // ( ( )
                    int next = text.IndexOf(bOpen, startBraces + 1);
                    if ((next > -1) && (next < endBraces))
                    {
                        startBraces = Math.Max(next, startBraces);
                    }

                    endBraces = -1;
                    while (startBraces > -1)
                    {
                        endBraces = text.IndexOf(bClose, startBraces + 1);
                        if (endBraces > startBraces && startBraces > -1)
                        {
                            string t = text.Substring(startBraces, (endBraces - startBraces) + 1);
                            t = SetHtmlColorCode(_moodsColor, t);
                            text = text.Remove(startBraces, (endBraces - startBraces) + 1).Insert(startBraces, t);
                        }
                        startBraces = text.IndexOf(bOpen, (endBraces + 30)); // HACKED: Warning!
                    }
                }
                return text;
            };

            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                string text = p.Text;
                string oldText = text;
                if (text.ToLower().Contains("<font"))
                    text = Utilities.RemoveHtmlFontTag(text);

                if (Regex.IsMatch(text, ":\\B") && checkBoxEnabledNarrator.Checked)
                {
                    text = SetColorForNarrator(text, p);
                }
                if (checkBoxEnabledMoods.Checked)
                {
                    if (text.Contains("("))
                        text = brackesType.Invoke(text, '(', ')');
                    if (text.Contains("["))
                        text = brackesType.Invoke(text, '[', ']');
                }

            End_Point:
                if (text != oldText)
                {
                    text = text.Replace(" " + Environment.NewLine, Environment.NewLine).Trim();
                    text = text.Replace(Environment.NewLine + " ", Environment.NewLine).Trim();
                    p.Text = text;
                }
            }
        }

        internal static string SetHtmlColorCode(Color color, string text)
        {
            string colorCode = string.Format("#{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B);
            string writeFormat = "<font color=\"{0}\">{1}</font>";
            return string.Format(writeFormat, colorCode.ToUpper(), text.Trim());
        }

        private string SetColorForNarrator(string text, Paragraph p)
        {
            var t = Utilities.RemoveHtmlTags(text);
            int index = t.IndexOf(":");
            if (index == t.Length - 1)
                return text;

            string htmlColor = string.Format("#{0:x2}{1:x2}{2:x2}", _narratorColor.R, _narratorColor.G, _narratorColor.B);
            string writeFormat = "<font color=\"{0}\">{1}</font>";
            Func<string, string> SetColor = (narrator) =>
            {
                if (narrator.ToLower().Contains("by") || narrator.ToLower().Contains("http"))
                    return narrator;
                return string.Format(writeFormat, htmlColor, narrator.Trim());
            };

            if (text.Contains(Environment.NewLine))
            {
                var lines = text.Replace(Environment.NewLine, "\n").Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    //TODO: if text contains 2 hearing text
                    string cleanText = Utilities.RemoveHtmlTags(lines[i]);
                    index = cleanText.IndexOf(":");

                    if ((index + 1 < cleanText.Length - 1) && char.IsDigit(cleanText[index + 1])) // filtered above \B
                    {
                        continue;
                    }

                    if (i > 0 && index == cleanText.Length - 1)
                    {
                        continue;
                    }
                    else
                    {
                        index = lines[i].IndexOf(":");
                        if (index > 0)
                        {
                            string temp = lines[i];
                            string pre = temp.Substring(0, index);
                            if (pre.Contains("(") || pre.Contains("[") || pre.Contains("{"))
                                continue;

                            //- MAN: Baby, I put it right over there.
                            //- JUNE: No, you did not.
                            if (Utilities.RemoveHtmlTags(pre).Trim().Length > 1)
                            {
                                // <i> i shall be \w that is way (?<!<)
                                string firstChr = Regex.Match(pre, "(?<!<)\\w", RegexOptions.Compiled).Value;
                                if (string.IsNullOrEmpty(firstChr))
                                    continue;
                                int idx = pre.IndexOf(firstChr);
                                string narrator = pre.Substring(idx, (index - idx));

                                // if it's not uppercase so skip it!
                                // if contains italic tag before the narrator this won't work:
                                // Readme: this will never match <i> or any other tags with contains one char
                                if (Utilities.RemoveHtmlTags(narrator).Trim().ToUpper() != Utilities.RemoveHtmlTags(narrator).Trim())
                                    continue;

                                narrator = SetColor(narrator);
                                pre = pre.Remove(idx, (index - idx)).Insert(idx, narrator);
                                temp = temp.Remove(0, index).Insert(0, pre);
                                if (temp != lines[i])
                                    lines[i] = temp;
                            }
                        }
                    }
                }
                text = string.Join(Environment.NewLine, lines);
            }
            else
            {
                string cleanText = Utilities.RemoveHtmlTags(text).Trim();
                index = cleanText.IndexOf(":");
                if (index < cleanText.Length - 1)
                {
                    index = text.IndexOf(":");
                    if (index > 0)
                    {
                        string pre = text.Substring(0, index);
                        // (ivandro ismael: djfalsdj)
                        if (pre.Contains("(") || pre.Contains("[") || pre.Contains("{"))
                            return text;

                        if (Utilities.RemoveHtmlTags(pre).Trim().Length > 0)
                        {
                            // <i>i shall be \w that is way (?<!<)
                            string firstChr = Regex.Match(pre, "(?<!<)\\w", RegexOptions.Compiled).Value;
                            int idx = pre.IndexOf(firstChr);
                            string narrator = pre.Substring(idx, (index - idx));
                            narrator = SetColor(narrator);
                            pre = pre.Remove(idx, index - idx).Insert(idx, narrator);
                            text = text.Remove(0, index).Insert(0, pre);
                        }
                    }
                }
            }
            return text;
        }

        private void RemoveColor(Paragraph p)
        {

        }

        private string GetSettingsFileName()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path.StartsWith("file:\\"))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            return Path.Combine(path, "HIColorer.xml");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private void GetSetting()
        {
            string fileName = GetSettingsFileName();
            if (!Path.IsPathRooted(fileName))
                return;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(fileName);

                int argNarrator = Convert.ToInt32(doc.DocumentElement.SelectSingleNode("ColorNarrator").InnerText);
                int argMoods = Convert.ToInt32(doc.DocumentElement.SelectSingleNode("ColorMoods").InnerText);
                _moodsColor = Color.FromArgb(argMoods);
                _narratorColor = Color.FromArgb(argNarrator);
                if (!_moodsColor.IsEmpty)
                {
                    this.labelMoodsColor.BackColor = _moodsColor;
                    this.labelMoodsColor.Text = Utilities.GetHtmlColorCode(_moodsColor);
                }
                if (!_narratorColor.IsEmpty)
                {
                    this.labelNarratorsColor.BackColor = _narratorColor;
                    this.labelNarratorsColor.Text = Utilities.GetHtmlColorCode(_narratorColor);
                }
            }
            catch { }
        }

        private void SaveSettings()
        {
            string fileName = GetSettingsFileName();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<ColorSettings><ColorNarrator/><ColorMoods/></ColorSettings>");
                doc.DocumentElement.SelectSingleNode("ColorNarrator").InnerText = Convert.ToString(_narratorColor.ToArgb());
                doc.DocumentElement.SelectSingleNode("ColorMoods").InnerText = Convert.ToString(_moodsColor.ToArgb());
                doc.Save(fileName);
            }
            catch { }
        }

        private void labelColor_DoubleClick(object sender, EventArgs e)
        {
            SetColor(sender);
        }

        private void linkLabelIvandrofly_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLabelIvandrofly.Tag.ToString());
        }
    }
}
