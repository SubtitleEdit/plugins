using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Dropbox.Api;
using SeDropBoxLoad;
using System.Linq;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class PluginForm : Form
    {
        private const string SeAppKey = "CLIENT_KEY";
        private const string SeAppsecret = "CLIENT_SECRET";
        private OAuth2Token _oAuth2token;
        private Stack<string> _folder = new Stack<string>();

        public string LoadedSubtitle { get; private set; }

        private DropboxFile _fileList;
        private int _connectTries = 0;

        private string GetSettingsFileName()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path != null && path.StartsWith("file:\\", StringComparison.Ordinal))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            return Path.Combine(path, "SeDropbox.xml");
        }

        private static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.Unicode.GetBytes(toEncode);
            return Convert.ToBase64String(toEncodeAsBytes);
        }

        public static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);
            return Encoding.Unicode.GetString(encodedDataAsBytes);
        }

        private OAuth2Token GetSavedToken()
        {
            string fileName = GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.Load(fileName);
                string accessToken = DecodeFrom64(doc.DocumentElement.SelectSingleNode("Accesstoken").InnerText);
                string uid = DecodeFrom64(doc.DocumentElement.SelectSingleNode("UId").InnerText);
                string accountId = DecodeFrom64(doc.DocumentElement.SelectSingleNode("AccountId").InnerText);
                string tokenType = DecodeFrom64(doc.DocumentElement.SelectSingleNode("TokenType").InnerText);
                return new OAuth2Token()
                {
                    access_token = accessToken,
                    uid = uid,
                    account_id = accountId,
                    token_type = tokenType
                };
            }
            catch
            {
                return null;
            }
        }

        private void SaveToken(OAuth2Token token)
        {
            string fileName = GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml("<SeDropbox><Accesstoken/><UId/><AccountId/><TokenType/></SeDropbox>");
                doc.DocumentElement.SelectSingleNode("Accesstoken").InnerText = EncodeTo64(token.access_token);
                doc.DocumentElement.SelectSingleNode("UId").InnerText = EncodeTo64(token.uid);
                doc.DocumentElement.SelectSingleNode("AccountId").InnerText = EncodeTo64(token.account_id);
                doc.DocumentElement.SelectSingleNode("TokenType").InnerText = EncodeTo64(token.token_type);
                doc.Save(fileName);
            }
            catch
            {
            }
        }

        internal PluginForm(string name, string description)
        {
            InitializeComponent();
            this.Text = name;
            labelDescription.Text = "Connecting to Dropbox...";
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (listViewFiles.SelectedItems.Count < 1)
                return;

            string fileName = listViewFiles.SelectedItems[0].Text;

            this.Refresh();
            try
            {
                Cursor = Cursors.WaitCursor;
                var f = listViewFiles.SelectedItems[0].Tag as DropboxFile;
                if (f != null)
                {
                    var api = new DropboxApi(SeAppKey, SeAppsecret, _oAuth2token);
                    if (f.IsDirectory)
                    {
                        labelInfo.Text = "Getting file list...";
                        labelDescription.Text = f.Path;
                        this.Refresh();
                        _fileList = api.GetFiles(f.Path);
                        if (f.Description == "..")
                        {
                            if (_folder.Count > 0)
                            {
                                var path = _folder.Pop();
                                var list = _fileList.Contents.ToList();
                                if (path != string.Empty)
                                {
                                    list.Insert(0, new DropboxFile { Path = GetWithoutPart(path), IsDirectory = true, Description = ".." });
                                }
                                _fileList.Contents = list;
                            }
                        }
                        else
                        {
                            string s = GetWithoutPart(f.Path);
                            _folder.Push(s);
                            var list = _fileList.Contents.ToList();
                            list.Insert(0, new DropboxFile { Path = s, IsDirectory = true, Description = ".." });
                            _fileList.Contents = list;
                        }

                        FillListView();
                    }
                    else
                    {
                        labelInfo.Text = "Downloading...";
                        this.Refresh();
                        var fileDown = api.DownloadFile(listViewFiles.SelectedItems[0].Tag as DropboxFile);
                        LoadedSubtitle = Encoding.UTF8.GetString(fileDown.Data);
                        DialogResult = DialogResult.OK;
                    }
                    Cursor = Cursors.Default;
                }
                labelInfo.Text = string.Empty;
                this.Refresh();
            }
            catch (Exception exception)
            {
                Cursor = Cursors.Default;
                MessageBox.Show(exception.Message);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void PluginForm_Shown(object sender, EventArgs e)
        {
            this.Refresh();
            _oAuth2token = GetSavedToken();
            var api = new DropboxApi(SeAppKey, SeAppsecret, _oAuth2token);
            if (_oAuth2token == null)
            {
                try
                {
                    Process.Start(api.GetPromptForCodeUrl());
                    using (var form = new GetDropBoxCode())
                    {
                        var result = form.ShowDialog(this);
                        if (result == DialogResult.OK && form.Code.Length > 10)
                        {
                            _oAuth2token = api.GetAccessToken(form.Code);
                        }
                        else
                        {
                            MessageBox.Show("Code skipped - no DropBox :(");
                            return;
                        }
                    }
                    labelInfo.Text = string.Empty;
                    SaveToken(_oAuth2token);
                }
                catch (Exception exception)
                {
                    labelInfo.Text = string.Empty;
                    labelDescription.Text = string.Empty;
                    _connectTries++;
                    if (_connectTries < 2)
                    {
                        PluginForm_Shown(sender, e);
                        return;
                    }
                    MessageBox.Show(exception.Message);
                    buttonOK.Enabled = false;
                    return;
                }
            }
            labelDescription.Text = string.Empty;
            labelInfo.Text = "Getting file list...";
            Cursor = Cursors.WaitCursor;
            this.Refresh();
            try
            {
                _fileList = api.GetFiles("");
                Cursor = Cursors.Default;
            }
            catch (Exception exception)
            {
                Cursor = Cursors.Default;
                _connectTries++;
                _connectTries++;
                if (_connectTries < 2)
                {
                    PluginForm_Shown(sender, e);
                    return;
                }
                MessageBox.Show(exception.Message);
                buttonOK.Enabled = false;
                return;
            }
            labelInfo.Text = string.Empty;
            FillListView();
        }

        private string GetLastPart(string path)
        {
            var idx = path.LastIndexOf("/");
            if (idx < 0)
            {
                return path;
            }
            return path.Remove(0, idx).TrimStart('/');
        }

        private string GetWithoutPart(string path)
        {
            var idx = path.LastIndexOf("/");
            if (idx > 0)
                return path.Substring(0, idx);
            else
                return string.Empty;
        }

        private void FillListView()
        {
            listViewFiles.BeginUpdate();
            listViewFiles.Items.Clear();
            foreach (DropboxFile f in _fileList.Contents)
            {
                string name = f.Path;
                if (f.IsDirectory && f.Description == "..")
                {
                    name = "..";
                }
                ListViewItem item = new ListViewItem(GetLastPart(name));
                item.Tag = f;
                if (f.IsDirectory)
                    item.ImageIndex = 1;
                else
                    item.ImageIndex = 0;
                item.SubItems.Add(f.Modified.ToShortDateString() + f.Modified.ToShortTimeString());
                item.SubItems.Add(f.Description);
                listViewFiles.Items.Add(item);
            }
            listViewFiles.EndUpdate();
            if (listViewFiles.Items.Count > 0)
                listViewFiles.Items[0].Selected = true;
        }

        private void listViewFiles_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            buttonOK_Click(sender, e);
        }

        private void listViewFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonOK_Click(sender, e);
            }
            else if (e.KeyCode == Keys.Back)
            {
                if (listViewFiles.Items.Count > 0)
                {
                    var f = listViewFiles.Items[0].Tag as DropboxFile;
                    if (f.IsDirectory && f.Description == "..")
                    {
                        listViewFiles.BeginUpdate();
                        for (int i = 1; i < listViewFiles.Items.Count; i++)
                        {
                            listViewFiles.Items[i].Selected = false;
                        }
                        listViewFiles.Items[0].Selected = true;
                        listViewFiles.Items[0].Focused = true;
                        listViewFiles.EndUpdate();
                        buttonOK_Click(sender, e);
                    }
                }
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

        private void PluginForm_Resize(object sender, EventArgs e)
        {
            var w = listViewFiles.Width - 120 - 100;
            if (w > 0)
            {
                listViewFiles.Columns[0].Width = w;
                listViewFiles.Columns[1].Width = 120;
            }
        }
    }
}
