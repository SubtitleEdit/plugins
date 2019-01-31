using Nikse.SubtitleEdit.PluginLogic.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;

namespace Nikse.SubtitleEdit.PluginLogic.Forms
{
    public partial class Main : Form
    {
        private const string SettingFile = "DictionariesProvider.json";

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

            foreach (var item in DictionariesInfo)
            {
                var group = new ListViewGroup(item.EnglishName);
                listView1.Groups.Add(group);
                var lItems = item.DownloadLinks.Select(di => new ListViewItem(di.Url.ToString())
                {
                    Group = group
                }).ToArray();
                //group.Items.AddRange(lItems);
                listView1.Items.AddRange(lItems);
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

        private void ButtonDownload_Click(object sender, EventArgs e)
        {

        }
    }
}
