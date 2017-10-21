using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
            this.listViewPluginInfo.SelectedIndexChanged += ListViewPluginInfo_SelectedIndexChanged;
            TryLocatingMetadataFile();
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

        private void buttonBrowse_Click(object sender, EventArgs e)
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
            //try
            //{
            _xDoc = XDocument.Load(metaFile);
            _plugins = new List<PluginInfo>();
            foreach (XElement el in _xDoc.Root.Elements("Plugin"))
            {
                var pluginInfo = new PluginInfo()
                {
                    Name = el.Element("Name").Value,
                    Description = el.Element("Description").Value,
                    Version = Convert.ToDecimal(el.Element("Version").Value),
                    Date = Convert.ToDateTime(el.Element("Date").Value.Replace('-', '/')),
                    Url = new Uri(el.Element("Url").Value),
                    Element = el
                };

                _plugins.Add(pluginInfo);
            }

            PushToListView(_plugins);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
        }

        private void PushToListView(IList<PluginInfo> plugins)
        {
            listViewPluginInfo.BeginUpdate();
            foreach (PluginInfo pluginInfo in plugins)
            {
                var lvi = new ListViewItem(pluginInfo.Name) { Tag = pluginInfo };
                // description
                lvi.SubItems.Add(pluginInfo.Description);
                // version
                lvi.SubItems.Add(pluginInfo.Version.ToString());
                // date
                lvi.SubItems.Add(pluginInfo.Date.ToString("yyyy-MM-dd"));
                // url
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
            numericUpDown1.Value = pluginInfo.Version;
            dateTimePicker1.Value = pluginInfo.Date;
            textBoxUrl.Text = pluginInfo.Url.ToString();
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            // really? :D
            if (listViewPluginInfo.SelectedItems.Count == 0)
            {
                return;
            }

            try
            {
                PluginInfo pluginInfo = (PluginInfo)listViewPluginInfo.SelectedItems[0].Tag;
                pluginInfo.Name = textBoxName.Text;
                pluginInfo.Description = textBoxDescription.Text;
                pluginInfo.Version = numericUpDown1.Value;
                pluginInfo.Date = dateTimePicker1.Value;
                pluginInfo.Url = new Uri(textBoxUrl.Text);
            }
            catch (Exception ex)
            {
                // ignore update on error
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
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
            }
            catch (Exception)
            {
                // just do nothing if anything bad happened
            }
            Application.Exit();
        }
    }
}
