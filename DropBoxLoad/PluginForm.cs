using Dropbox.Api;
using SeDropBoxLoad;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal sealed partial class PluginForm : Form
    {
        private const string SeAppKey = "CLIENT_KEY";
        private const string SeAppsecret = "CLIENT_SECRET";
        private OAuth2Token _oAuth2Token;
        private readonly Stack<string> _folder = new Stack<string>();

        public string LoadedSubtitle { get; private set; }

        private DropboxFile _fileList;
        private int _connectTries = 0;

        private static string GetSettingsFileName()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path != null && path.StartsWith("file:\\", StringComparison.Ordinal))
            {
                path = path.Remove(0, 6);
            }

            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
            {
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            }

            return Path.Combine(path, "SeDropbox.xml");
        }

        private static string EncodeTo64(string toEncode)
        {
            var toEncodeAsBytes = Encoding.Unicode.GetBytes(toEncode);
            return Convert.ToBase64String(toEncodeAsBytes);
        }

        public static string DecodeFrom64(string encodedData)
        {
            var encodedDataAsBytes = Convert.FromBase64String(encodedData);
            return Encoding.Unicode.GetString(encodedDataAsBytes);
        }

        private OAuth2Token GetSavedToken()
        {
            var fileName = GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.Load(fileName);
                var accessToken = DecodeFrom64(doc.DocumentElement.SelectSingleNode("Accesstoken").InnerText);
                var uid = DecodeFrom64(doc.DocumentElement.SelectSingleNode("UId").InnerText);
                var accountId = DecodeFrom64(doc.DocumentElement.SelectSingleNode("AccountId").InnerText);
                var tokenType = DecodeFrom64(doc.DocumentElement.SelectSingleNode("TokenType").InnerText);
                return new OAuth2Token
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

        private static void SaveToken(OAuth2Token token)
        {
            var fileName = GetSettingsFileName();
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
                // ignored
            }
        }

        internal PluginForm(string name, string description)
        {
            InitializeComponent();
            Text = name;
            labelDescription.Text = "Connecting to Dropbox...";
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (listViewFiles.SelectedItems.Count < 1)
            {
                return;
            }

            Refresh();
            try
            {
                Cursor = Cursors.WaitCursor;
                var f = listViewFiles.SelectedItems[0].Tag as DropboxFile;
                if (f != null)
                {
                    var api = new DropboxApi(SeAppKey, SeAppsecret, _oAuth2Token);
                    if (f.IsDirectory)
                    {
                        labelInfo.Text = "Getting file list...";
                        labelDescription.Text = f.Path;
                        Refresh();
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
                            var s = GetWithoutPart(f.Path);
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
                        Refresh();
                        var fileDown = api.DownloadFile(listViewFiles.SelectedItems[0].Tag as DropboxFile);
                        LoadedSubtitle = Encoding.UTF8.GetString(fileDown.Data);
                        DialogResult = DialogResult.OK;
                    }
                    Cursor = Cursors.Default;
                }
                labelInfo.Text = string.Empty;
                Refresh();
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
            Refresh();
            _oAuth2Token = GetSavedToken();
            var api = new DropboxApi(SeAppKey, SeAppsecret, _oAuth2Token);
            if (_oAuth2Token == null)
            {
                try
                {
                    Process.Start(api.GetPromptForCodeUrl());
                    using (var form = new GetDropBoxCode())
                    {
                        var result = form.ShowDialog(this);
                        if (result == DialogResult.OK && form.Code.Length > 10)
                        {
                            _oAuth2Token = api.GetAccessToken(form.Code);
                        }
                        else
                        {
                            MessageBox.Show("Code skipped - no Dropbox :(");
                            return;
                        }
                    }
                    labelInfo.Text = string.Empty;
                    SaveToken(_oAuth2Token);
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
            Refresh();
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
            return idx > 0 ? path.Substring(0, idx) : string.Empty;
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
                ListViewItem item = new ListViewItem(GetLastPart(name))
                {
                    Tag = f
                };
                item.ImageIndex = f.IsDirectory ? 1 : 0;
                item.SubItems.Add(f.Modified.ToShortDateString() + f.Modified.ToShortTimeString());
                item.SubItems.Add(f.Description);
                listViewFiles.Items.Add(item);
            }
            listViewFiles.EndUpdate();
            if (listViewFiles.Items.Count > 0)
            {
                listViewFiles.Items[0].Selected = true;
            }
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
