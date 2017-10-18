using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public partial class MainForm : Form
    {
        public string Subtitle { get; private set; }

        private readonly Configs _configs;

        private Subtitle _subtitle;

        public MainForm(Subtitle sub, string name, string ver)
        {
            InitializeComponent();
            _subtitle = sub;

            string settingFile = Path.Combine(FileUtils.PluginDirectory, "hi-color.xml");
            if (File.Exists(settingFile))
            {
                // TODO: try reading previous config file and delete the old file

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

        private void ButtonRemove_Click(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count == 0)
            {
                return;
            }

            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                var text = p.Text;
                if (!text.ContainsColor())
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
            Subtitle = _subtitle.ToText();
            DialogResult = DialogResult.OK;
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (!checkBoxEnabledMoods.Checked && !checkBoxEnabledNarrator.Checked)
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

            // take out all <font tags
            // (warning: this may cause some problems if subtitle already contain 'font face' attribute)
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                p.Text = HtmlUtils.RemoveOpenCloseTags(p.Text, HtmlUtils.TagFont);
            }

            foreach (Artist artist in GetArtists())
            {
                artist.Paint(_subtitle);
            }

            Subtitle = _subtitle.ToText();
            DialogResult = DialogResult.OK;
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

        private IEnumerable<Artist> GetArtists()
        {
            if (checkBoxEnabledMoods.Checked)
            {
                yield return new MoodsArtist(GetPaletteFromARGB(_configs.Moods));
            }
            if (checkBoxEnabledNarrator.Checked)
            {
                yield return new NarratorArtist(GetPaletteFromARGB(_configs.Narrator));
            }
        }

        private static Palette GetPaletteFromARGB(int argbCode)
        {
            return new Palette()
            {
                Color = Color.FromArgb(argbCode)
            };
        }
    }
}
