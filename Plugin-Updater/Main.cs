using Octokit;
using Plugin_Updater.Helpers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Plugin_Updater
{
    public partial class Main : Form
    {
        private string _metaFile = string.Empty;
        private XDocument _xDoc;

        private DateTime _trackDate;

        public Main()
        {
            InitializeComponent();

            listViewPluginInfo.OwnerDraw = true;
            listViewPluginInfo.DrawItem += ListViewPluginInfo_DrawItem;
            listViewPluginInfo.DrawSubItem += ListViewPluginInfo_DrawSubItem;
            listViewPluginInfo.DrawColumnHeader += ListViewPluginInfo_DrawColumnHeader;

            // hook handlers
            Resize += delegate
            {
                listViewPluginInfo.Columns[listViewPluginInfo.Columns.Count - 1].Width = -2;
            };
            buttonGithubUpload.Click += (sender, e) =>
            {
                using (var uploadToGithub = new UploadToGithub())
                {
                    uploadToGithub.ShowDialog(this);
                }
            };

            OnResize(EventArgs.Empty);
            listViewPluginInfo.SelectedIndexChanged += ListViewPluginInfo_SelectedIndexChanged;
            //TryLocatingMetadataFile();
            _metaFile = Utils.GetMetaFile();
            textBoxMetaFilePath.Text = _metaFile;
            _xDoc = XDocument.Load(_metaFile);

            // init columns order
            foreach (ColumnHeader ch in listViewPluginInfo.Columns)
            {
                ch.Tag = new SortContext
                {
                    SortOrder = SortOrder.Ascending,
                    SortType = (SortType)Enum.Parse(typeof(SortType), ch.Text)
                };
            }

            LoadInfoFromMetadata(SortContext.DefaultContext);
            string url = "https://github.com/SubtitleEdit/plugins";
            linkLabel2.Links.Add(new LinkLabel.Link(0, linkLabel2.Text.Length, url)
            {
                Name = "Subtitle Edit (Plugins)"
            });
            linkLabel1.Click += (sender, e) =>
            {
                System.Diagnostics.Process.Start("https://www.github.com/ivandrofly");
            };
            linkLabel2.LinkClicked += LinkLabel2_LinkClicked;
        }

        private void LinkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //e.Link.Visited = true;
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }

        private void ListViewPluginInfo_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void ListViewPluginInfo_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            using (var sf = new StringFormat())
            {
                switch (e.Header.TextAlign)
                {
                    case HorizontalAlignment.Left:
                        sf.Alignment = StringAlignment.Near;
                        break;
                    case HorizontalAlignment.Right:
                        sf.Alignment = StringAlignment.Far;
                        break;
                    case HorizontalAlignment.Center:
                        sf.Alignment = StringAlignment.Center;
                        break;
                }

                if (e.Item.Selected)
                {
                    e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds);

                    TextRenderer.DrawText(e.Graphics, e.Item.SubItems[e.ColumnIndex].Text, listViewPluginInfo.Font,
                        new Point(e.Bounds.Left + 3, e.Bounds.Top + 2), e.Item.ForeColor, TextFormatFlags.RightToLeft);

                    //e.Graphics.DrawString("", null, null, null, new StringFormat(StringFormatFlags.))

                }
                else
                {
                    e.DrawDefault = true;
                }
            }
        }


        private void ListViewPluginInfo_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            // listview focus
            if (listViewPluginInfo.Focused)
            {
                e.DrawDefault = true;
                return;
            }
            // items focus
            if (e.Item.Focused)
            {
                e.DrawFocusRectangle();
            }
        }

        //private void TryLocatingMetadataFile()
        //{
        //    // try get Plugin4.xml location
        //    // C:\Users\{user-name}\...\plugins\Plugin-Updater\bin\Debug
        //    string debugFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        //    string[] paths = debugFolder.Split(Path.DirectorySeparatorChar);
        //    var sb = new StringBuilder();

        //    string separator = string.Empty;
        //    for (int i = 0; i < paths.Length; i++)
        //    {
        //        string path = paths[i];
        //        if (i > 0)
        //        {
        //            separator = Path.DirectorySeparatorChar.ToString();
        //        }
        //        sb.Append(separator + path);
        //        if (path.Equals("plugins", StringComparison.OrdinalIgnoreCase))
        //        {
        //            break;
        //        }
        //    }

        //    string plugin4xml = Path.Combine(sb.ToString(), "Plugins4.xml");
        //    if (File.Exists(plugin4xml))
        //    {
        //        _metaFile = plugin4xml;
        //        textBoxMetaFilePath.Text = _metaFile;
        //        //ReadContent(_metaFile);
        //    }

        //}

        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            using (var fileDialog = new OpenFileDialog())
            {
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    _metaFile = fileDialog.FileName;
                }
                else
                {
                    return;
                }
            }

            textBoxMetaFilePath.Text = _metaFile;
            _xDoc = XDocument.Load(_metaFile);
            LoadInfoFromMetadata(SortContext.DefaultContext);
        }

        //private void ReadContent(string metaFile)
        //{
        //    if (!Path.IsPathRooted(metaFile))
        //    {
        //        return;
        //    }

        //    _xDoc = XDocument.Load(metaFile);
        //    _plugins = new List<PluginInfo>();
        //    foreach (XElement el in _xDoc.Root.Elements("Plugin"))
        //    {
        //        var pluginInfo = new PluginInfo
        //        {
        //            Name = el.Element(nameof(PluginInfo.Name)).Value,
        //            Description = el.Element(nameof(PluginInfo.Description)).Value,
        //            Version = Convert.ToDecimal(el.Element(nameof(PluginInfo.Version)).Value),
        //            Date = Convert.ToDateTime(el.Element(nameof(PluginInfo.Date)).Value.Replace('-', '/')),
        //            Url = new Uri(el.Element(nameof(PluginInfo.Url)).Value),
        //            Author = el.Element(nameof(PluginInfo.Author))?.Value,
        //            Element = el
        //        };

        //        _plugins.Add(pluginInfo);
        //    }

        //    PushToListView(_plugins);
        //}

        //private void PushToListView(IList<PluginInfo> plugins)
        //{
        //    listViewPluginInfo.BeginUpdate();
        //    foreach (PluginInfo pluginInfo in plugins.OrderBy(plugin => plugin.Name))
        //    {
        //        var lvi = new ListViewItem(pluginInfo.Name) { Tag = pluginInfo };
        //        lvi.SubItems.Add(pluginInfo.Description);
        //        lvi.SubItems.Add(pluginInfo.Version.ToString());
        //        lvi.SubItems.Add(pluginInfo.Date.ToString("yyyy-MM-dd"));
        //        lvi.SubItems.Add(pluginInfo.Author);
        //        lvi.SubItems.Add(pluginInfo.Url.ToString());
        //        listViewPluginInfo.Items.Add(lvi);
        //    }
        //    listViewPluginInfo.EndUpdate();
        //}

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

            _trackDate = pluginInfo.Date;
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
                if (_trackDate.Equals(dateTimePicker1.Value))
                {
                    if (MessageBox.Show("Update release date with the current date?", "Update release date",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        dateTimePicker1.Value = DateTime.Now;
                    }
                }

                ListViewItem lvi = listViewPluginInfo.SelectedItems[0];
                PluginInfo pluginInfo = (PluginInfo)lvi.Tag;

                UpdateModel(pluginInfo);
                UpdateView(lvi, pluginInfo);

                MessageBox.Show("Plugin model updated, to save the change to Plugin4.xml, click button \"Save\"", "Model updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // ignore update on error
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateModel(PluginInfo pluginInfo)
        {
            pluginInfo.Name = textBoxName.Text;
            pluginInfo.Description = textBoxDescription.Text;
            pluginInfo.Version = numericUpDownVersion.Value;
            pluginInfo.Date = dateTimePicker1.Value;
            pluginInfo.Url = new Uri(textBoxUrl.Text);
            pluginInfo.Author = textBoxAuthor.Text.Trim();
        }

        private static void UpdateView(ListViewItem lvi, PluginInfo pluginInfo)
        {
            // update listview
            lvi.Text = pluginInfo.Name;
            lvi.SubItems[1].Text = pluginInfo.Description;
            lvi.SubItems[2].Text = pluginInfo.Version.ToString("#0.00");
            lvi.SubItems[3].Text = pluginInfo.Date.ToString("yyyy-MM-dd");
            lvi.SubItems[4].Text = pluginInfo.Author;
            lvi.SubItems[5].Text = pluginInfo.Url.ToString();
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            SaveChanges();
        }

        private void SaveChanges()
        {
            try
            {
                foreach (ListViewItem item in listViewPluginInfo.Items)
                {
                    PluginInfo pluginInfo = (PluginInfo)item.Tag;
                    pluginInfo.UpdateXElement();
                }
                _xDoc.Save(_metaFile);
                MessageBox.Show("Changes saved to Plugins4.xml", "Changes saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // just do nothing if anything bad happened
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            PluginInfo pluginInfo;
            try
            {
                pluginInfo = new PluginInfo
                {
                    Name = textBoxName.Text,
                    Author = textBoxAuthor.Text,
                    Version = numericUpDownVersion.Value,
                    Date = dateTimePicker1.Value,
                    Description = textBoxDescription.Text,
                    Url = new Uri(textBoxUrl.Text),
                };
            }
            catch (Exception ex)
            {
                // one or more field is not set
                MessageBox.Show(ex.Message);
                return;
            }

            var pluginInfoEls = pluginInfo.GetType()
                .GetProperties()
                .Where(p => p.GetType() != typeof(XElement))
                .Select(p => new XElement(p.Name, p.GetValue(pluginInfo))).ToList();

            // check if plugin with save name doesn't already exits
            if (_xDoc.Root.Elements("Plugin").Any(p => p.Element("Name").Value.Equals(pluginInfo.Name, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Plugin already exits with same name!", "Trying to add duplicated plugin",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _xDoc.Root.Add(new XElement("Plugin", pluginInfoEls));

            try
            {
                _xDoc.Save(_metaFile);
                MessageBox.Show("Plugin added with success!", "New plugin added!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            LoadInfoFromMetadata(new SortContext { SortType = SortType.Name, SortOrder = SortOrder.Ascending });
        }

        private void LoadInfoFromMetadata(SortContext sortContext)
        {
            listViewPluginInfo.BeginUpdate();
            listViewPluginInfo.Items.Clear();

            var pluginsInfo = _xDoc.Root.Elements("Plugin").Select(el => new PluginInfo
            {
                Name = el.Element("Name").Value,
                Description = el.Element("Description").Value,
                Version = Convert.ToDecimal(el.Element("Version").Value),
                Date = Convert.ToDateTime(el.Element("Date").Value),
                Author = el.Element("Author").Value,
                Url = new Uri(el.Element("Url").Value),
                Element = el
            });

            IOrderedEnumerable<PluginInfo> orderedList = null;
            //if (sortContext.SortOrder == SortOrder.Ascending)
            //{
            //    orderedList = pluginsInfo.OrderBy(p => GetSortProp(p, sortContext));
            //}
            //else
            //{
            //    orderedList = pluginsInfo.OrderByDescending(p => GetSortProp(p, sortContext));
            //}

            // # 2 version
            switch (sortContext.SortType)
            {
                case SortType.Name:
                    switch (sortContext.SortOrder)
                    {
                        case SortOrder.Ascending:
                            orderedList = pluginsInfo.OrderBy(p => p.Name);
                            break;
                        case SortOrder.Descending:
                            orderedList = pluginsInfo.OrderByDescending(p => p.Name);
                            break;
                    }
                    break;

                case SortType.Description:
                    switch (sortContext.SortOrder)
                    {
                        case SortOrder.Ascending:
                            orderedList = pluginsInfo.OrderBy(p => p.Description);
                            break;
                        case SortOrder.Descending:
                            orderedList = pluginsInfo.OrderByDescending(p => p.Description);
                            break;
                    }
                    break;

                case SortType.Version:
                    switch (sortContext.SortOrder)
                    {
                        case SortOrder.Ascending:
                            orderedList = pluginsInfo.OrderBy(p => p.Version);
                            break;
                        case SortOrder.Descending:
                            orderedList = pluginsInfo.OrderByDescending(p => p.Version);
                            break;
                    }
                    break;

                case SortType.Author:
                    switch (sortContext.SortOrder)
                    {
                        case SortOrder.Ascending:
                            orderedList = pluginsInfo.OrderBy(p => p.Author);
                            break;
                        case SortOrder.Descending:
                            orderedList = pluginsInfo.OrderByDescending(p => p.Author);
                            break;
                    }
                    break;

                case SortType.Date:
                    switch (sortContext.SortOrder)
                    {
                        case SortOrder.Ascending:
                            orderedList = pluginsInfo.OrderBy(p => p.Date);
                            break;
                        case SortOrder.Descending:
                            orderedList = pluginsInfo.OrderByDescending(p => p.Date);
                            break;
                    }
                    break;
                case SortType.Url:
                    switch (sortContext.SortOrder)
                    {
                        case SortOrder.Ascending:
                            orderedList = pluginsInfo.OrderBy(p => p.Url.AbsolutePath);
                            break;
                        case SortOrder.Descending:
                            orderedList = pluginsInfo.OrderByDescending(p => p.Url.AbsolutePath);
                            break;
                    }
                    break;
            }

            ListViewItem[] lvItems = orderedList.Select(p => new ListViewItem(p.Name)
            {
                SubItems =
                {
                    p.Description, p.Version.ToString("#0.00"), p.Date.ToString("yyy-MM-dd"), p.Author, p.Url.ToString()
                },
                Tag = p
            }).ToArray();

            listViewPluginInfo.Items.AddRange(lvItems);
            listViewPluginInfo.EndUpdate();
        }

        private string GetSortProp(PluginInfo p, SortContext sc)
        {
            //ColumnHeader column = listViewPluginInfo.Columns[sc.ColumnIdx];

            // map clicked column with PluginInfo type property
            //PropertyInfo propInfo = typeof(PluginInfo).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            //    .FirstOrDefault(prop => prop.Name.StartsWith(column.Text, StringComparison.OrdinalIgnoreCase));

            // UNDONE! THERE IS NOT ONLY TYPE STRING IN LISTVIEW
            // HANDLE DATETIME, VERSION, URL...
            //return propInfo.GetValue(p) as string;

            throw new NotImplementedException();
        }

        private void ButtonRemove_Click(object sender, EventArgs e)
        {
            ListViewItem lvi = listViewPluginInfo.SelectedItems.Cast<ListViewItem>().FirstOrDefault();
            if (lvi == null)
            {
                return;
            }

            // remove from metafile
            var elToRemove = _xDoc.Root.Elements("Plugin")
                .FirstOrDefault(el => el.Element("Name").Value
                .Equals(((PluginInfo)lvi.Tag).Name, StringComparison.OrdinalIgnoreCase));

            elToRemove?.Remove();

            // remove from listview
            listViewPluginInfo.BeginUpdate();
            listViewPluginInfo.Items.Remove(lvi);
            listViewPluginInfo.EndUpdate();
        }

        private void ListViewPluginInfo_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ColumnHeader column = listViewPluginInfo.Columns[e.Column];
            SortContext sc = (SortContext)column.Tag;
            //var newOrder = (SortOrder)(sortOrder + 1 % 2);
            //listViewPluginInfo.Columns[e.Column].Tag = newOrder;

            sc.SortOrder = sc.SortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            LoadInfoFromMetadata(sc);
        }

        private void ButtonCopy_Click(object sender, EventArgs e)
        {
            if (!ValidateUrl())
            {
                return;
            }
            Clipboard.SetText(textBoxUrl.Text);
        }

        private void ButtonDownload_Click(object sender, EventArgs e)
        {
            if (!ValidateUrl())
            {
                return;
            }
            Process.Start(textBoxUrl.Text);
        }

        private bool ValidateUrl()
        {
            string url = textBoxUrl.Text;
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }
            return Uri.TryCreate(url, UriKind.Absolute, out Uri result);
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            if (MessageBox.Show("Save model from view", "Update model", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                SaveChanges();
            }

            Close();
        }

        private void buttonDllInfo_Click(object sender, EventArgs e)
        {
            using (var fd = new OpenFileDialog())
            {
                if (fd.ShowDialog(this) == DialogResult.OK)
                {
                    // todo: read info from ddl file
                    // select file
                    // create instance
                    // get value from properies

                    // invalid file
                    if (!Path.IsPathRooted(fd.FileName))
                    {
                        return;
                    }
                    // Nikse.SubtitleEdit.PluginLogic.ExportAllFormats

                    string typeFullName = "Nikse.SubtitleEdit.PluginLogic.ExportAllFormats";
                    var assembly = Assembly.LoadFile(fd.FileName);
                    var pluginType = assembly.GetType(typeFullName);
                    var instance = Activator.CreateInstance(pluginType);

                    // failed to create instance of pluing
                    if (instance is null)
                    {
                        throw new NullReferenceException(nameof(instance));
                    }

                    var bindingFlag = BindingFlags.Instance | BindingFlags.Public;

                    // load plugin metadata information
                    string name = (string)instance.GetType().GetProperty("Name", bindingFlag).GetValue(instance);
                    string desc = (string)instance.GetType().GetProperty("Description", bindingFlag).GetValue(instance);
                    decimal ver = (decimal)instance.GetType().GetProperty("Version", bindingFlag).GetValue(instance);
                    // https://github.com/SubtitleEdit/plugins/releases/download/v.1.1/Ispravi.zip
                    string url = $"https://github.com/SubtitleEdit/plugins/releases/download/v.1.1/{Path.GetFileNameWithoutExtension(fd.FileName)}.zip";

                    textBoxName.Text = name;
                    textBoxDescription.Text = desc;
                    numericUpDownVersion.Value = ver;
                    textBoxAuthor.Text = "ivandrofly";
                    textBoxUrl.Text = url;
                    dateTimePicker1.Value = DateTime.Now;

                }
            }
        }
    }

    internal class SortContext
    {
        public static SortContext DefaultContext;

        static SortContext()
        {
            DefaultContext = new SortContext
            {
                SortType = SortType.Name,
                SortOrder = SortOrder.Ascending,
            };
        }

        public SortType SortType { get; set; }

        public SortOrder SortOrder { get; set; }
    }

    internal enum SortType
    {
        None,
        Name,
        Description,
        Version,
        Author,
        Date,
        Url,
    }
}
