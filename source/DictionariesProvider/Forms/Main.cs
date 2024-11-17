using Newtonsoft.Json;
using Nikse.SubtitleEdit.PluginLogic.Helpers;
using Nikse.SubtitleEdit.PluginLogic.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
                Process.Start(FileUtils.Dictionaries);
            };

            listViewDownloadUrls.SelectedIndexChanged += (sender, e) =>
            {
                if (listViewDownloadUrls.SelectedIndices.Count == 0)
                {
                    return;
                }

                var lvi = listViewDownloadUrls.SelectedItems[0];
                var di = (DictionaryInfo)lvi.Group.Tag;

                textBoxEnglishName.Text = di.EnglishName;
                textBoxNativeName.Text = di.NativeName;
                textBoxDescription.Text = di.Description;

                comboBoxDownloadLinks.BeginUpdate();
                comboBoxDownloadLinks.Items.Clear();
                comboBoxDownloadLinks.Items.AddRange(di.DownloadLinks.Select(dl => dl.Url.OriginalString).ToArray());
                comboBoxDownloadLinks.EndUpdate();

                checkBoxStatus.Checked = ((DownloadLink)lvi.Tag).Status;
            };

            comboBoxDownloadLinks.SelectedIndexChanged += (sender, e) =>
            {
                comboBoxDownloadLinks.Text = comboBoxDownloadLinks.SelectedText;
            };
            _webUtils = new WebUtils(new System.Net.Http.HttpClient());
        }

        private void LoadConfigs()
        {
            string file = Path.Combine(FileUtils.Plugins, SettingFile);
            if (File.Exists(file))
            {
                string content = File.ReadAllText(file);
                try
                {
                    DictionariesInfo = JsonConvert.DeserializeObject<List<DictionaryInfo>>(content);
                }
                catch
                {
                    // maybe the saving configuration changes
                    File.Move(file, Path.GetDirectoryName(file) +
                        Path.GetFileNameWithoutExtension(file) + ".old" + Path.GetExtension(file));
                    DictionariesInfo = new List<DictionaryInfo>();
                }
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

            ((DictionaryInfo)listViewDownloadUrls.SelectedItems[0].Group.Tag).DownloadLinks.Add(new DownloadLink
            {
                Status = true,
                Url = new Uri(newUrl)
            });
            comboBoxDownloadLinks.Items.Add(newUrl);
            //if (comboBoxDownloadLinks.Items.Cast<string>().Any(url => newUrl.Equals(newUrl, StringComparison.OrdinalIgnoreCase)) == false)
            //{
            //}
            //else
            //{
            //    MessageBox.Show("Url already exits");
            //}

            comboBoxDownloadLinks.Text = string.Empty;
            UpdateListView();
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

            listViewDownloadUrls.BeginUpdate();
            listViewDownloadUrls.Items.Clear();

            foreach (DictionaryInfo di in DictionariesInfo)
            {
                var group = new ListViewGroup(di.EnglishName)
                {
                    Tag = di
                };
                listViewDownloadUrls.Groups.Add(group);
                var downloadLinks = di.DownloadLinks.Select(dl => new ListViewItem(dl.Url.OriginalString)
                {
                    Group = group,
                    Tag = dl
                }).ToArray();
                //group.Items.AddRange(lItems);
                listViewDownloadUrls.Items.AddRange(downloadLinks);
            }

            listViewDownloadUrls.EndUpdate();
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
            Cursor = Cursors.WaitCursor;
            if (listViewDownloadUrls.SelectedItems.Count == 0)
            {
                return;
            }

            var downloadUrls = listViewDownloadUrls.SelectedItems.Cast<ListViewItem>().Select(lvi => lvi.Text);
            // parallel asnc download
            await Task.WhenAll(downloadUrls.Select(url => _webUtils.Download(url)));

            Cursor = Cursors.Default;
            MessageBox.Show("Download completed");
        }

        private void ImportFormXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var openfile = new OpenFileDialog())
            {
                openfile.Filter = "(Dictionary files) (*.xml)|*.xml";

                if (openfile.ShowDialog() == DialogResult.OK)
                {
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
                                  Status = true
                              }
                         }
                     }).ToList();

                    UpdateListView();
                }

            }
        }

        private async void ButtonUpdateStatus_Click(object sender, EventArgs e)
        {
            buttonUpdateStatus.Enabled = false;
            //await System.Threading.Tasks.Task.Yield();
            await _webUtils.UpdateStateAsync(DictionariesInfo.SelectMany(di => di.DownloadLinks));//.ConfigureAwait(true);
            buttonUpdateStatus.Enabled = true;
        }
    }
}
