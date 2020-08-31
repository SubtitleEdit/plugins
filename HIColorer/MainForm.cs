using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public partial class MainForm : Form, IConfigurable
    {
        public string Subtitle { get; private set; }

        private ColorConfig _configs;

        private Subtitle _subtitle;

        public MainForm(Subtitle sub, string name, string ver)
        {
            InitializeComponent();

            FormClosed += (s, e) =>
            {
                SaveConfigurations();
            };

            // donate handler
            pictureBoxDonate.Click += (s, e) =>
            {
                System.Diagnostics.Process.Start(StringUtils.DonateUrl);
            };

            _subtitle = sub;
            LoadConfigurations();
            UpdateUIOnColorChange();
        }

        private void UpdateUIOnColorChange()
        {
            labelNarratorsColor.Text = HtmlUtils.ColorToHtml(Color.FromArgb(_configs.Narrator));
            labelMoodsColor.Text = HtmlUtils.ColorToHtml(Color.FromArgb(_configs.Moods));
            labelNarratorsColor.BackColor = Color.FromArgb(_configs.Narrator);
            labelMoodsColor.BackColor = Color.FromArgb(_configs.Moods);
        }

        private void ChangeColorHandler(object sender, EventArgs e)
        {
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
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                var text = p.Text;
                if (!text.ContainsColor())
                {
                    continue;
                }
                text = HtmlUtils.RemoveOpenCloseTags(text, HtmlUtils.TagFont);
                if (text.ContainsColor() == false)
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

        private void LinkLabelIvandrofly_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
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

        public void LoadConfigurations()
        {
            // TODO: Get rid of this in up next version
            try
            {
                File.Delete("hicolor.xml");
            }
            catch
            {
            }

            string configFile = Path.Combine(FileUtils.Plugins, "hicolor-config.xml");
            if (File.Exists(configFile))
            {
                _configs = Configuration<ColorConfig>.LoadConfiguration(configFile);
            }
            else
            {
                _configs = new ColorConfig(configFile);
                _configs.SaveConfigurations();
            }
        }

        public void SaveConfigurations() => _configs.SaveConfigurations();
    }
}
