using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public partial class MainForm : Form
    {
        public string FixedSubtitle { get; private set; }

        private readonly Configs _configs;

        private Subtitle _subtitle;

        public MainForm(Subtitle sub, string name, string ver)
        {
            InitializeComponent();
            _subtitle = sub;

            string settingFile = Path.Combine(FileUtils.PluginDirectory, "hicolor.xml");
            if (File.Exists(settingFile))
            {
                _configs = Settings<Configs>.LoadConfiguration(settingFile);

                // set configuration file
                if (string.IsNullOrEmpty(_configs.SettingFile))
                {
                    _configs.SettingFile = settingFile;
                }
            }
            else
            {
                _configs = new Configs()
                {
                    Narrator = Color.GreenYellow.ToArgb(),
                    Moods = Color.Maroon.ToArgb(),
                    SettingFile = settingFile
                };
                _configs.SaveConfigurations();
            }
            UpdateUIOnColorChange();
        }

        private void UpdateUIOnColorChange()
        {
            labelNarratorsColor.Text = HtmlUtils.ColorToHtml(Color.FromArgb(_configs.Narrator));
            labelMoodsColor.Text = HtmlUtils.ColorToHtml(Color.FromArgb(_configs.Moods));
            labelNarratorsColor.BackColor = Color.FromArgb(_configs.Narrator);
            labelMoodsColor.BackColor = Color.FromArgb(_configs.Moods);
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
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    _configs.Narrator = colorDialog1.Color.ToArgb();
                }
            }
            else
            {
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    _configs.Moods = colorDialog1.Color.ToArgb();
                }
            }
            UpdateUIOnColorChange();
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count == 0)
                return;
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                var text = p.Text;
                if (!text.Contains("<font", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var oldText = text;
                text = HtmlUtils.RemoveOpenCloseTags(text, HtmlUtils.TagFont);
                if (text != oldText)
                {
                    p.Text = text;
                }
            }
            FixedSubtitle = _subtitle.ToText();
            DialogResult = DialogResult.OK;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (!checkBoxEnabledMoods.Checked && !checkBoxEnabledNarrator.Checked)
            {
                DialogResult = DialogResult.Cancel;
            }
            else
            {
                FindHearingImpairedNotation();
                FixedSubtitle = _subtitle.ToText();
                if (string.IsNullOrEmpty(FixedSubtitle))
                {
                    DialogResult = DialogResult.Cancel;
                }
                DialogResult = DialogResult.OK;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void FindHearingImpairedNotation()
        {
            if (_subtitle == null || _subtitle.Paragraphs.Count == 0)
            {
                return;
            }

            Func<string, char, char, string> brackesType = (text, bOpen, bClose) =>
            {
                int idx = text.IndexOf(bOpen);
                while (idx >= 0)
                {
                    var endIdx = text.IndexOf(bClose, idx + 1);
                    if (endIdx < idx + 1)
                    {
                        break;
                    }

                    var t = text.Substring(idx, endIdx - idx + 1);
                    t = SetHtmlColorCode(Color.FromArgb(_configs.Moods), t);
                    text = text.Remove(idx, endIdx - idx + 1).Insert(idx, t);
                    idx = text.IndexOf(bOpen, idx + t.Length);
                }
                return text;
            };

            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                string text = p.Text;
                string oldText = text;
                if (text.Contains("<font", StringComparison.OrdinalIgnoreCase))
                    text = HtmlUtils.RemoveTags(text);

                if (Regex.IsMatch(text, ":\\B") && checkBoxEnabledNarrator.Checked)
                {
                    text = SetColorForNarrator(text);
                }
                if (checkBoxEnabledMoods.Checked)
                {
                    if (text.Contains('('))
                        text = brackesType.Invoke(text, '(', ')');
                    if (text.Contains('['))
                        text = brackesType.Invoke(text, '[', ']');
                }

                if (text.Length != oldText.Length)
                    p.Text = text;
            }
        }

        internal static string SetHtmlColorCode(Color color, string text)
        {
            const string writeFormat = "<font color=\"{0}\">{1}</font>";
            return string.Format(writeFormat, HtmlUtils.ColorToHtml(color).ToUpperInvariant(), text.Trim());
        }

        private string SetColorForNarrator(string text)
        {
            var noTagText = HtmlUtils.RemoveTags(text);
            int index = noTagText.IndexOf(':');
            if (index + 1 == noTagText.Length)
                return text;

            string htmlColor = HtmlUtils.ColorToHtml(Color.FromArgb(_configs.Narrator));
            const string writeFormat = "<font color=\"{0}\">{1}</font>";
            Func<string, string> SetColor = (narrator) =>
            {
                string narratorLower = narrator.ToLower();
                if (narratorLower.Contains("by") || narratorLower.Contains("http"))
                    return narrator;
                return string.Format(writeFormat, htmlColor, narrator.Trim());
            };

            var lines = text.Replace(Environment.NewLine, "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                //TODO: if text contains 2 hearing text
                var cleanText = HtmlUtils.RemoveTags(lines[i], true).TrimEnd('"', '\'').TrimEnd();
                index = cleanText.IndexOf(':');

                if ((index + 1 < cleanText.Length && char.IsDigit(cleanText[index + 1])) ||// filtered above \B
                    (i > 0 && index + 1 == cleanText.Length))  // return if : is second line last char
                {
                    continue;
                }

                index = lines[i].IndexOf(':');
                if (index > 0)
                {
                    var line = lines[i];
                    string pre = line.Substring(0, index).TrimStart();
                    if (pre.Length == 0 || pre.Contains('(') || pre.Contains('(') || pre.Contains('('))
                    {
                        continue;
                    }

                    //- MAN: Baby, I put it right over there.
                    //- JUNE: No, you did not.
                    if (HtmlUtils.RemoveTags(pre, true).Trim().Length > 1)
                    {
                        // <i> i shall be \w that is way (?<!<)
                        string firstChr = Regex.Match(pre, "(?<!<)\\w", RegexOptions.Compiled).Value;
                        if (string.IsNullOrEmpty(firstChr))
                            continue;
                        int idx = pre.IndexOf(firstChr, System.StringComparison.Ordinal);
                        var narrator = pre.Substring(idx, index - idx).Trim();

                        narrator = SetColor(narrator);
                        pre = pre.Remove(idx, (index - idx)).Insert(idx, narrator);
                        line = line.Remove(0, index).Insert(0, pre);
                        if (line == lines[i]) continue;
                        lines[i] = line;
                    }

                }
                text = string.Join(Environment.NewLine, lines);
            }
            return text;
        }

        private string MoveClosingTagsToEnd(string pre)
        {
            // <i>Foobar</i> Foobar:
            var idx = pre.IndexOf("</", StringComparison.Ordinal);
            if (idx > 1)
            {
                var closeIdx = pre.IndexOf('>', idx + 2);
                if (closeIdx < idx)
                    return pre;
                var endTag = pre.Substring(idx, closeIdx - idx + 1);
                pre = pre.Remove(idx, closeIdx - idx + 1).Insert(pre.Length, endTag);
            }
            return pre;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _configs.SaveConfigurations();
        }

        //private void LoadingSettings()
        //{
        //    string fileName = GetSettingsFileName();
        //    if (!Path.IsPathRooted(fileName))
        //    {
        //        return;
        //    }

        //    try
        //    {
        //        XmlDocument doc = new XmlDocument();
        //        doc.Load(fileName);

        //        int argNarrator = Convert.ToInt32(doc.DocumentElement.SelectSingleNode("ColorNarrator").InnerText);
        //        int argMoods = Convert.ToInt32(doc.DocumentElement.SelectSingleNode("ColorMoods").InnerText);
        //        _moodsColor = Color.FromArgb(argMoods);
        //        _narratorColor = Color.FromArgb(argNarrator);
        //        if (!_moodsColor.IsEmpty)
        //        {
        //            labelMoodsColor.BackColor = _moodsColor;
        //            labelMoodsColor.Text = HtmlUtils.ColorToHtml(_moodsColor);
        //        }
        //        if (!_narratorColor.IsEmpty)
        //        {
        //            labelNarratorsColor.BackColor = _narratorColor;
        //            labelNarratorsColor.Text = HtmlUtils.ColorToHtml(_narratorColor);
        //        }
        //    }
        //    catch { }
        //}

        /*private void SaveSettings()
        {
            string fileName = GetSettingsFileName();

            // Store settings in xml file.
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<ColorSettings><ColorNarrator/><ColorMoods/></ColorSettings>");
                doc.DocumentElement.SelectSingleNode("ColorNarrator").InnerText = Convert.ToString(_narratorColor.ToArgb());
                doc.DocumentElement.SelectSingleNode("ColorMoods").InnerText = Convert.ToString(_moodsColor.ToArgb());
                doc.Save(fileName);
            }
            catch
            {

            }

#if DEBUG
            // Use Windows-Registry to store Configurations.
            using (var regkey = Registry.CurrentUser.OpenSubKey("Software", true))
            {
                RegistryKey pluginsRegKey = regkey.OpenSubKey(@"Subtitle Edit\Plugins\HIColorer\Settings", true);
                if (pluginsRegKey == null)
                {
                    pluginsRegKey = regkey.CreateSubKey(@"Subtitle Edit\Plugins\HIColorer\Settings");
                }
                pluginsRegKey.SetValue("ColorNarrator", _narratorColor.ToArgb(), RegistryValueKind.DWord);
                pluginsRegKey.SetValue("ColorMoods", _moodsColor.ToArgb(), RegistryValueKind.DWord);
                pluginsRegKey.Dispose();
            }
#endif
        }*/

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
