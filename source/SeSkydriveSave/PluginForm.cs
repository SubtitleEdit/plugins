using OneDriveSave;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace SeSkydriveSave
{
    public partial class PluginForm : Form
    {
        private const string ClientId = "737a5bf2-01f2-4ede-9429-e703d2598923";
        private const string ClientWebsite = "msal737a5bf2-01f2-4ede-9429-e703d2598923://auth";
        private OneDriveApi _api;

        public string LoadedSubtitle { get; set; }

        private Stack<string> _roots;
        private string _fileName;
        private string _content;
        private string _path;
        private string _pathId;

        public PluginForm(string name, string description, string fileName, string content)
        {
            InitializeComponent();
            Text = name;
            labelDescription.Text = description;
            labelInfo.Text = "Connecting to OneDrive...";
            _api = new OneDriveApi("files.readwrite", ClientId, ClientWebsite);
            _fileName = fileName;
            _content = content;
            comboBoxFileName.Text = Path.GetFileName(_fileName);
        }

        public static string FormatBytesToDisplayFileSize(long fileSize)
        {
            if (fileSize <= 1024)
                return string.Format("{0} bytes", fileSize);
            if (fileSize <= 1024 * 1024)
                return string.Format("{0} kb", fileSize / 1024);
            if (fileSize <= 1024 * 1024 * 1024)
                return string.Format("{0:0.0} mb", (float)fileSize / (1024 * 1024));
            return string.Format("{0:0.0} gb", (float)fileSize / (1024 * 1024 * 1024));
        }

        private string GetSettingsFileName()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path.StartsWith("file:\\"))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            return Path.Combine(path, "OneDrive.xml");
        }

        private static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.Encoding.Unicode.GetBytes(toEncode);
            return Convert.ToBase64String(toEncodeAsBytes);
        }

        public static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);
            return System.Text.Encoding.Unicode.GetString(encodedDataAsBytes);
        }

        private OAuth2Token GetSavedToken()
        {
            string fileName = GetSettingsFileName();
            try
            {
                var oAuth2Token = new OAuth2Token();
                var doc = new XmlDocument();
                doc.Load(fileName);
                oAuth2Token.access_token = DecodeFrom64(doc.DocumentElement.SelectSingleNode("AccessToken").InnerText);
                return oAuth2Token;
            }
            catch
            {
                return null;
            }
        }

        private void SaveToken(OAuth2Token oAuth2Token)
        {
            string fileName = GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml("<OneDrive><Expires/><RefreshToken/><AccessToken/></OneDrive>");
                doc.DocumentElement.SelectSingleNode("Expires").InnerText = EncodeTo64(oAuth2Token.expires_in.ToString(CultureInfo.InvariantCulture));
                doc.DocumentElement.SelectSingleNode("AccessToken").InnerText = EncodeTo64(oAuth2Token.access_token);
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
                var t = GetSavedToken();
                if (true) //t == null)  //TODO: use refresh token?
                {
                    var b = new Browser(_api.LoginUrl);
                    if (b.ShowDialog(this) == DialogResult.OK)
                    {
                        if (!string.IsNullOrEmpty(b.Code))
                        {
                            Cursor.Current = Cursors.WaitCursor;
                            _api.InitTokens(b.Code);
                            SaveToken(_api.OAuth2Token);
                            _path = LoadFiles(null);
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
                    _api.OAuth2Token = t;
                    Cursor = Cursors.WaitCursor;
                    _path = LoadFiles(null);
                    Cursor = Cursors.Default;
                }
                labelInfo.Text = string.Empty;
            }
            catch (Exception exception)
            {
                Cursor = Cursors.Default;
                MessageBox.Show(exception.Message);
                DialogResult = DialogResult.Cancel;
            }
        }

        private string LoadFiles(OneDriveContent oneDriveContent)
        {
            listViewFiles.Items.Clear();
            if (_roots.Count > 0)
            {
                var item = new ListViewItem("..");
                var sc = new OneDriveContent
                {
                    Type = "folder",
                    ParentId = _roots.Peek()
                };
                sc.Id = sc.ParentId;
                item.Tag = sc;
                item.ImageIndex = 1;
                listViewFiles.Items.Add(item);
            }
            string path = string.Empty;
            comboBoxFileName.Items.Clear();
            foreach (OneDriveContent f in _api.GetFiles(oneDriveContent, out _pathId))
            {
                path = f.Path;
                if (f.IsFile || f.IsFolder)
                {
                    ListViewItem item = new ListViewItem(f.Name);
                    item.SubItems.Add(f.UpdatedTime.ToShortDateString() + " " + f.UpdatedTime.ToShortTimeString());
                    item.SubItems.Add(FormatBytesToDisplayFileSize(f.Size));
                    item.Tag = f;
                    if (f.IsFile)
                    {
                        item.ImageIndex = 0;
                        comboBoxFileName.Items.Add(f.Name);
                    }
                    else
                    {
                        item.ImageIndex = 1;
                    }
                    listViewFiles.Items.Add(item);
                }
            }
            if (listViewFiles.Items.Count > 0)
                listViewFiles.Items[0].Selected = true;
            return path;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (comboBoxFileName.Text.Trim().Length < 1)
                return;

            _fileName = comboBoxFileName.Text;
            string parentId = string.Empty;
            if (_roots.Count > 0)
                parentId = _roots.Peek();
            try
            {
                labelInfo.Text = "Uploading...";
                Cursor = Cursors.WaitCursor;
                _api.UploadFile(_path, _pathId, Path.GetFileName(_fileName), _content);
                Cursor = Cursors.Default;
                DialogResult = DialogResult.OK;
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
            if (listViewFiles.SelectedItems.Count < 1)
                return;

            var oneDrivecontent = (OneDriveContent)listViewFiles.SelectedItems[0].Tag;
            try
            {
                if (listViewFiles.SelectedItems[0].Text == "..")
                {
                    labelInfo.Text = "Loading files...";
                    Cursor = Cursors.WaitCursor;
                    _roots.Pop();
                    _path = LoadFiles(oneDrivecontent);
                    Cursor = Cursors.Default;
                }
                else if (oneDrivecontent.IsFolder)
                {
                    labelInfo.Text = "Loading files...";
                    Cursor = Cursors.WaitCursor;
                    _roots.Push(oneDrivecontent.ParentId);
                    _path = LoadFiles(oneDrivecontent);
                    Cursor = Cursors.Default;
                }
                else if (oneDrivecontent.IsFile)
                {
                    buttonOK_Click(sender, e);
                    DialogResult = DialogResult.OK;
                }
                Cursor = Cursors.Default;
                labelInfo.Text = string.Empty;
                Refresh();
            }
            catch (Exception exception)
            {
                labelInfo.Text = string.Empty;
                Cursor = Cursors.Default;
                MessageBox.Show(exception.Message);
            }
        }

        private void PluginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void listViewFiles_MouseClick(object sender, MouseEventArgs e)
        {
            if (listViewFiles.SelectedItems.Count < 1)
                return;

            OneDriveContent sc = (OneDriveContent)listViewFiles.SelectedItems[0].Tag;
            if (sc.IsFile)
            {
                comboBoxFileName.Text = sc.Name;
            }
        }

        private void PluginForm_Resize(object sender, EventArgs e)
        {
            var w = listViewFiles.Width - 120 - 100;
            if (w > 0)
            {
                listViewFiles.Columns[0].Width = w;
                listViewFiles.Columns[1].Width = 120;
            }
        }

        private void PluginForm_ResizeEnd(object sender, EventArgs e)
        {
            var w = listViewFiles.Width - 120 - 100;
            if (w > 0)
            {
                listViewFiles.Columns[0].Width = w;
                listViewFiles.Columns[1].Width = 120;
            }

            listViewFiles.Columns[2].Width = -2;
        }

    }
}
