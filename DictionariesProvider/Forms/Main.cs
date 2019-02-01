using Newtonsoft.Json;
using Nikse.SubtitleEdit.PluginLogic.Helpers;
using Nikse.SubtitleEdit.PluginLogic.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.PluginLogic.Forms
{
    public partial class Main : Form
    {
        private const string SettingFile = "DictionariesProvider.json";
        private WebUtils _webUtils;

        private List<DictionaryInfo> DictionariesInfo { get; set; }


        public Main()
        {
            InitializeComponent();
            LoadConfigs();

            // init ui

            linkLabelOpenDicFolder.Click += delegate
            {
                Process.Start(FileUtils.Dictionary);
            };
        }

        private void LoadConfigs()
        {
            string file = Path.Combine(FileUtils.Plugins, SettingFile);
            if (File.Exists(file))
            {
                string content = File.ReadAllText(file);
                DictionariesInfo = JsonConvert.DeserializeObject<List<DictionaryInfo>>(content);
                UpdateListView();
            }
            else
            {
                DictionariesInfo = new List<DictionaryInfo>();
            }
        }

        private void ButtonAddLink_Click(object sender, EventArgs e)
        {
            //GC.KeepAlive
            string newUrl = comboBoxDownloadLinks.Text;

            comboBoxDownloadLinks.Items.Add(newUrl);
            //if (comboBoxDownloadLinks.Items.Cast<string>().Any(url => newUrl.Equals(newUrl, StringComparison.OrdinalIgnoreCase)) == false)
            //{
            //}
            //else
            //{
            //    MessageBox.Show("Url already exits");
            //}

            comboBoxDownloadLinks.Text = string.Empty;
        }

        private void ButtonAddDictionary_Click(object sender, EventArgs e)
        {
            // todo: validation

            // make sure group with same name doesn't already exits

            DictionariesInfo.Add(new DictionaryInfo
            {
                EnglishName = textBoxEnglishName.Text,
                NativeName = textBoxNativeName.Text,
                Description = textBoxDescription.Text,
                DownloadLinks = comboBoxDownloadLinks.Items.Cast<string>()
                .Select(dl => new DownloadLink { Url = new Uri(dl) }).ToList()
            });

            UpdateListView();
            comboBoxDownloadLinks.Items.Clear();
        }

        private void UpdateListView()
        {
            if (DictionariesInfo.Count == 0)
            {
                return;
            }
            // get or create new group

            listView1.BeginUpdate();
            listView1.Items.Clear();

            foreach (DictionaryInfo di in DictionariesInfo)
            {
                var group = new ListViewGroup(di.EnglishName);
                listView1.Groups.Add(group);
                var downloadLinks = di.DownloadLinks.Select(dl => new ListViewItem(dl.Url.ToString())
                {
                    Group = group,
                    Tag = dl
                }).ToArray();
                //group.Items.AddRange(lItems);
                listView1.Items.AddRange(downloadLinks);
            }

            listView1.EndUpdate();
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            var jsonString = JsonConvert.SerializeObject(DictionariesInfo, Formatting.Indented);
            string file = Path.Combine(FileUtils.Plugins, SettingFile);
            File.WriteAllText(file, jsonString);
            DialogResult = DialogResult.OK;
        }

        private async void ButtonDownload_Click(object sender, EventArgs e)
        {
            _webUtils = _webUtils ?? new WebUtils(new System.Net.Http.HttpClient());

            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }

            var dl = (DownloadLink)listView1.SelectedItems[0].Tag;
            await _webUtils.Download(dl.Url.ToString()).ConfigureAwait(true);

        }


        private void ImportFormXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var openfile = new OpenFileDialog())
            {
                openfile.Filter = "(Dictionary files) (*.xml)|*.xml";

                if (openfile.ShowDialog() == DialogResult.OK)
                {
                    //                  < OpenOfficeDictionaries >
                    //< Dictionary >
                    //  < EnglishName > Afrikaans </ EnglishName >
                    //  < NativeName > Afrikaans </ NativeName >
                    //  < DownloadLink > http://downloads.sourceforge.net/project/aoo-extensions/1109/0/dict-af.oxt?r=http%3A%2F%2Fextensions.services.openoffice.org%2Fen%2Fproject%2Fafrikaans-spell-checker&amp;ts=1373917891&amp;use_mirror=kent</DownloadLink>
                    //  < Description > Afrikaans spell checker</ Description >
                    string xmlfile = openfile.FileName;

                    var xdoc = XDocument.Load(xmlfile);
                    var elDics = xdoc.Descendants("Dictionary");

                    DictionariesInfo = elDics.Select(el =>
                     new DictionaryInfo
                     {
                         NativeName = el.Element("NativeName")?.Value,
                         EnglishName = el.Element("EnglishName")?.Value,
                         Description = el.Element("Description")?.Value,
                         DownloadLinks = new List<DownloadLink>{
                              new DownloadLink
                              {
                                  Url = new Uri(el.Element("DownloadLink").Value),
                                  Status = string.Empty
                              }
                         }
                     }).ToList();

                    UpdateListView();
                }

            }
        }
    }
}
