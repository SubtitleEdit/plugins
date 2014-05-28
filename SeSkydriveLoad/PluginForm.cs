using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace SeSkydriveLoad
{
    public partial class PluginForm : Form
    {
        private SkydriveApi _api;
        public string LoadedSubtitle { get; private set; }
        private System.Collections.Generic.Stack<string> _roots;

        public PluginForm(string name, string description)
        {
            InitializeComponent();
            this.Text = name;
            labelDescription.Text = description;
            labelInfo.Text = "Connecting to Skydrive...";
            _api = new SkydriveApi("wl.skydrive_update");
        }

        public static string FormatBytesToDisplayFileSize(long fileSize)
        {
            {
                if (fileSize <= 1024)
                    return string.Format("{0} bytes", fileSize);
                if (fileSize <= 1024 * 1024)
                    return string.Format("{0} kb", fileSize / 1024);
                if (fileSize <= 1024 * 1024 * 1024)
                    return string.Format("{0:0.0} mb", (float)fileSize / (1024 * 1024));
                return string.Format("{0:0.0} gb", (float)fileSize / (1024 * 1024 * 1024));
            }
        }

        private string GetSettingsFileName()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path.StartsWith("file:\\"))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            return Path.Combine(path, "SeSkydrive.xml");
        }

        private static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.Encoding.Unicode.GetBytes(toEncode);
            return System.Convert.ToBase64String(toEncodeAsBytes);
        }

        public static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            return System.Text.Encoding.Unicode.GetString(encodedDataAsBytes);
        }

        private string GetSavedToken()
        {
            string fileName = GetSettingsFileName();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(fileName);
                DateTime expires = Convert.ToDateTime(DecodeFrom64(doc.DocumentElement.SelectSingleNode("Expires").InnerText));
                if (expires.AddMinutes(5) < DateTime.Now)
                    return null; // token expired
                string token = DecodeFrom64(doc.DocumentElement.SelectSingleNode("Token").InnerText);
                return token;
            }
            catch
            {
                return null;
            }
        }

        private void SaveToken(string token, DateTime expires)
        {
            string fileName = GetSettingsFileName();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<SeSkydrive><Expires/><Token/></SeSkydrive>");
                doc.DocumentElement.SelectSingleNode("Expires").InnerText = EncodeTo64(expires.ToString("s"));
                doc.DocumentElement.SelectSingleNode("Token").InnerText = EncodeTo64(token);
                doc.Save(fileName);
            }
            catch
            {
            }
        }

        private void PluginForm_Shown(object sender, EventArgs e)
        {
            try
            {
                _roots = new Stack<string>();
                Refresh();
                string t = GetSavedToken();
                if (t == null)
                {
                    var b = new Browser(_api.LoginUrl);
                    if (b.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                    {
                        if (!string.IsNullOrEmpty(b.Token))
                        {
                            Cursor.Current = Cursors.WaitCursor;
                            _api.AccessToken = b.Token;
                            SaveToken(_api.AccessToken, b.Expires);
                            LoadFiles("me/skydrive");
                            Cursor.Current = Cursors.Default;
                        }
                        else
                        {
                            MessageBox.Show("Login failed");
                            DialogResult = DialogResult.Cancel;
                        }
                    }
                }
                else
                {
                    _api.AccessToken = t;
                    Cursor = Cursors.WaitCursor;
                    LoadFiles("me/skydrive");
                    Cursor = Cursors.Default;
                }
                labelInfo.Text = string.Empty;
            }
            catch (Exception exception)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show(exception.Message);
                DialogResult = DialogResult.Cancel;
            }
        }

        private void LoadFiles(string path)
        {
            listViewFiles.Items.Clear();
            if (_roots.Count > 0)
            {
                ListViewItem item = new ListViewItem("..");
                SkydriveContent sc = new SkydriveContent();
                sc.ParentId = _roots.Peek();
                item.Tag = sc;
                item.ImageIndex = 1;
                listViewFiles.Items.Add(item);
            }

            foreach (SkydriveContent f in _api.GetFiles(path))
            {
                if (f.IsFile || f.IsFolder)
                {
                    ListViewItem item = new ListViewItem(f.Name) { Tag = f };
                    item.SubItems.Add(f.UpdatedTime.ToShortDateString() + " " + f.UpdatedTime.ToShortTimeString());
                    item.SubItems.Add(FormatBytesToDisplayFileSize(f.Size));
                    if (f.IsFile)
                        item.ImageIndex = 0;
                    else
                        item.ImageIndex = 1;
                    listViewFiles.Items.Add(item);
                }
            }
            if (listViewFiles.Items.Count > 0)
                listViewFiles.Items[0].Selected = true;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (listViewFiles.SelectedItems.Count < 1)
                return;

            SkydriveContent sc = (SkydriveContent)listViewFiles.SelectedItems[0].Tag;
            try
            {
                if (listViewFiles.SelectedItems[0].Text == "..")
                {
                    labelInfo.Text = "Loading files...";
                    Cursor = Cursors.WaitCursor;
                    _roots.Pop();
                    LoadFiles(sc.ParentId);
                    Cursor = Cursors.Default;
                }
                else if (sc.IsFolder)
                {
                    labelInfo.Text = "Loading files...";
                    Cursor = Cursors.WaitCursor;
                    _roots.Push(sc.ParentId);
                    LoadFiles(sc.Id);
                    Cursor = Cursors.Default;
                }
                else if (sc.IsFile)
                {
                    labelInfo.Text = "Downloading...";
                    this.Refresh();
                    Cursor = Cursors.WaitCursor;
                    var fileDown = _api.DownloadFile(sc);
                    LoadedSubtitle = Encoding.UTF8.GetString(fileDown).Trim();
                    DialogResult = DialogResult.OK;
                }
                Cursor = Cursors.Default;
                labelInfo.Text = string.Empty;
                this.Refresh();
            }
            catch (Exception exception)
            {
                labelInfo.Text = string.Empty;
                Cursor = Cursors.Default;
                MessageBox.Show(exception.Message);
            }
        }

        private void listViewFiles_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            buttonOK_Click(sender, e);
        }

        private void PluginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }
    }
}