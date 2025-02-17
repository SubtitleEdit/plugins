using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public partial class MainForm : Form //, IConfigurable
    {
        public string Subtitle { get; private set; }

        private ColorConfig _configs;

        private Subtitle _subtitle;

        public MainForm(Subtitle sub, string name, string ver)
        {
            InitializeComponent();

            // save configure
            FormClosed += (s, e) => _configs.Save();

            // donate handler
            pictureBoxDonate.Click += (s, e) => { System.Diagnostics.Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=9EFVREKVKC2VJ&source=url"); };

            _subtitle = sub;
            _configs = ColorConfig.LoadOrCreateConfigurations();
            UpdateUIOnColorChange();
        }

        private void UpdateUIOnColorChange()
        {
            labelNarratorsColor.Text = Utilities.ColorToHex(Color.FromArgb(_configs.Narrator));
            labelMoodsColor.Text = Utilities.ColorToHex(Color.FromArgb(_configs.Moods));
            labelMusicColor.Text = Utilities.ColorToHex(Color.FromArgb(_configs.Music));

            labelNarratorsColor.BackColor = Color.FromArgb(_configs.Narrator);
            labelMoodsColor.BackColor = Color.FromArgb(_configs.Moods);
            labelMusicColor.BackColor = Color.FromArgb(_configs.Music);
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

        private void buttonPickMusicColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                _configs.Music = colorDialog1.Color.ToArgb();
                UpdateUIOnColorChange();
            }
        }

        private void ButtonRemove_Click(object sender, EventArgs e)
        {
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                var text = p.Text;
                if (!text.HasColor())
                {
                    continue;
                }

                p.Text = HtmlUtil.RemoveOpenCloseTags(text, HtmlUtil.TagFont);
            }

            Subtitle = _subtitle.ToText(new SubRip());
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
            foreach (var p in _subtitle.Paragraphs)
            {
                p.Text = HtmlUtil.RemoveOpenCloseTags(p.Text, HtmlUtil.TagFont);
            }

            foreach (Artist artist in GetArtists())
            {
                artist.Paint(_subtitle);
            }

            Subtitle = _subtitle.ToText(new SubRip());
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
                yield return new MoodsArtist(GetPaletteFromArgb(_configs.Moods));
            }

            if (checkBoxEnabledNarrator.Checked)
            {
                yield return new NarratorArtist(GetPaletteFromArgb(_configs.Narrator));
            }

            if (checkBoxEnabledNarrator.Checked)
            {
                yield return new MusicArtist(GetPaletteFromArgb(_configs.Music));
            }
        }

        private static Palette GetPaletteFromArgb(int argbCode)
        {
            return new Palette()
            {
                Color = Color.FromArgb(argbCode)
            };
        }
    }
}