using Plugin_Updater.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Plugin_Updater
{
    public partial class Main : Form
    {
        private string _metaFile = string.Empty;
        private XDocument _xDoc;
        private IList<PluginInfo> _plugins;

        public Main()
        {
            InitializeComponent();

            Resize += delegate
            {
                listViewPluginInfo.Columns[listViewPluginInfo.Columns.Count - 1].Width = -2;
            };

            listViewPluginInfo.SelectedIndexChanged += ListViewPluginInfo_SelectedIndexChanged;
            //TryLocatingMetadataFile();
            string metafile = Utils.GetMetaFile();
            LoadInfoFromMetadata(Utils.GetMetaFile());
        }

        private void TryLocatingMetadataFile()
        {
            // try get Plugin4.xml location
            // C:\Users\{user-name}\...\plugins\Plugin-Updater\bin\Debug
            string debugFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string[] paths = debugFolder.Split(Path.DirectorySeparatorChar);
            var sb = new StringBuilder();

            string separator = string.Empty;
            for (int i = 0; i < paths.Length; i++)
            {
                string path = paths[i];
                if (i > 0)
                {
                    separator = Path.DirectorySeparatorChar.ToString();
                }
                sb.Append(separator + path);
                if (path.Equals("plugins", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }

            string plugin4xml = Path.Combine(sb.ToString(), "Plugins4.xml");
            if (File.Exists(plugin4xml))
            {
                _metaFile = plugin4xml;
                textBoxMetaFilePath.Text = _metaFile;
                ReadContent(_metaFile);
            }

        }

        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            // only request to provide file if it couldn't be loaded
            // automatically
            if (!File.Exists(_metaFile))
            {
                using (var fileDialog = new OpenFileDialog())
                {
                    if (fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        _metaFile = fileDialog.FileName;
                    }
                }
            }
            textBoxMetaFilePath.Text = _metaFile;
            // read content from file
            ReadContent(_metaFile);
        }

        private void ReadContent(string metaFile)
        {
            if (!Path.IsPathRooted(metaFile))
            {
                return;
            }

            _xDoc = XDocument.Load(metaFile);
            _plugins = new List<PluginInfo>();
            foreach (XElement el in _xDoc.Root.Elements("Plugin"))
            {
                var pluginInfo = new PluginInfo
                {
                    Name = el.Element(nameof(PluginInfo.Name)).Value,
                    Description = el.Element(nameof(PluginInfo.Description)).Value,
                    Version = Convert.ToDecimal(el.Element(nameof(PluginInfo.Version)).Value),
                    Date = Convert.ToDateTime(el.Element(nameof(PluginInfo.Date)).Value.Replace('-', '/')),
                    Url = new Uri(el.Element(nameof(PluginInfo.Url)).Value),
                    Author = el.Element(nameof(PluginInfo.Author))?.Value,
                    Element = el
                };

                _plugins.Add(pluginInfo);
            }

            PushToListView(_plugins);
        }

        private void PushToListView(IList<PluginInfo> plugins)
        {
            listViewPluginInfo.BeginUpdate();
            foreach (PluginInfo pluginInfo in plugins.OrderBy(plugin => plugin.Name))
            {
                var lvi = new ListViewItem(pluginInfo.Name) { Tag = pluginInfo };
                lvi.SubItems.Add(pluginInfo.Description);
                lvi.SubItems.Add(pluginInfo.Version.ToString());
                lvi.SubItems.Add(pluginInfo.Date.ToString("yyyy-MM-dd"));
                lvi.SubItems.Add(pluginInfo.Author);
                lvi.SubItems.Add(pluginInfo.Url.ToString());
                listViewPluginInfo.Items.Add(lvi);
            }
            listViewPluginInfo.EndUpdate();
        }

        private void ListViewPluginInfo_SelectedIndexChanged(object sender, EventArgs e)
        {
            // really? :D
            if (listViewPluginInfo.SelectedItems.Count == 0)
            {
                return;
            }

            PluginInfo pluginInfo = (PluginInfo)listViewPluginInfo.SelectedItems[0].Tag;

            // push info to textbox
            textBoxName.Text = pluginInfo.Name;
            textBoxDescription.Text = pluginInfo.Description;
            numericUpDownVersion.Value = pluginInfo.Version;
            dateTimePicker1.Value = pluginInfo.Date;
            textBoxAuthor.Text = pluginInfo.Author;
            textBoxUrl.Text = pluginInfo.Url.ToString();
        }

        private void ButtonUpdate_Click(object sender, EventArgs e)
        {
            // really? :D
            if (listViewPluginInfo.SelectedItems.Count == 0)
            {
                return;
            }

            try
            {
                ListViewItem lvi = listViewPluginInfo.SelectedItems[0];
                PluginInfo pluginInfo = (PluginInfo)lvi.Tag;
                pluginInfo.Name = textBoxName.Text;
                pluginInfo.Description = textBoxDescription.Text;
                pluginInfo.Version = numericUpDownVersion.Value;
                pluginInfo.Date = dateTimePicker1.Value;
                pluginInfo.Url = new Uri(textBoxUrl.Text);
                pluginInfo.Author = textBoxAuthor.Text.Trim();

                // update listview
                lvi.Text = pluginInfo.Name;
                lvi.SubItems[1].Text = pluginInfo.Description;
                lvi.SubItems[2].Text = pluginInfo.Version.ToString();
                lvi.SubItems[3].Text = pluginInfo.Date.ToString("yyyy-MM-dd");
                lvi.SubItems[4].Text = pluginInfo.Author;
                lvi.SubItems[5].Text = pluginInfo.Url.ToString();

                MessageBox.Show("Plugin updated :)");
            }
            catch (Exception ex)
            {
                // ignore update on error
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            // leave the app
            if (_plugins == null)
            {
                return;
            }

            try
            {
                foreach (PluginInfo pluginInfo in _plugins)
                {
                    pluginInfo.UpdateXElement();
                }
                _xDoc?.Save(_metaFile);

                MessageBox.Show("Saved with success!", "Changes saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // just do nothing if anything bad happened
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            var pluginInfo = new PluginInfo
            {
                Name = textBoxName.Text,
                Author = textBoxAuthor.Text,
                Version = numericUpDownVersion.Value,
                Date = dateTimePicker1.Value,
                Description = textBoxDescription.Text,
                Url = new Uri(textBoxUrl.Text),
            };

            string metaFile = Utils.GetMetaFile();

            var pluginInfoEls = pluginInfo.GetType()
                .GetProperties()
                .Where(p => p.GetType() != typeof(XElement))
                .Select(p => new XElement(p.Name, p.GetValue(pluginInfo)));

            var xDoc = XDocument.Load(metaFile);

            // check if plugin with save name doesn't already exits
            if (xDoc.Root.Elements("Plugin").Any(p => p.Element("Name").Value.Equals(pluginInfo.Name, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Plugin already exits with same name!", "Trying to add duplicated plugin",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            xDoc.Root.Add(new XElement("Plugin", pluginInfoEls));

            try
            {
                xDoc.Save(metaFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            LoadInfoFromMetadata(metaFile);
        }

        private void LoadInfoFromMetadata(string metaFile)
        {
            listViewPluginInfo.BeginUpdate();
            listViewPluginInfo.Items.Clear();

            XDocument xDoc = XDocument.Load(metaFile);

            var pluginsInfo = xDoc.Root.Elements("Plugin").Select(el => new PluginInfo
            {
                Name = el.Element("Name").Value,
                Description = el.Element("Description").Value,
                Version = Convert.ToDecimal(el.Element("Version").Value),
                Date = Convert.ToDateTime(el.Element("Date").Value),
                Author = el.Element("Author").Value,
                Url = new Uri(el.Element("Url").Value),
            });

            var lvItems = pluginsInfo.Select(p => new ListViewItem(p.Name)
            {
                SubItems =
                {
                    p.Description, p.Version.ToString(), p.Date.ToString("yyy-MM-dd"), p.Author, p.Url.ToString()
                },
                Tag = p
            }).ToArray();

            listViewPluginInfo.Items.AddRange(lvItems);
            listViewPluginInfo.EndUpdate();
        }
    }
}