using Octokit;
using Octokit.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Plugin_Updater
{
    public partial class UploadToGithub : Form
    {
        private GitHubClient _client;
        private const string owner = "subtitleedit";
        private const string name = "plugins";
        private string _tokenFile = "token.txt";
        public UploadToGithub()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterParent;

            // hook event handlers

            buttonOK.Click += (sender, e) =>
            {
                DialogResult = DialogResult.OK;
            };

            buttonCancel.Click += (sender, e) =>
            {
                DialogResult = DialogResult.Cancel;
            };

            string token = string.Empty;
            _tokenFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "token.txt");

            // look for token in same location as the app 1st
            if (File.Exists(_tokenFile))
            {
                token = File.ReadAllText(_tokenFile).Trim();
            }
            else if (File.Exists("token.txt")) // look at the root directory
            {
                _tokenFile = "token.txt";
                token = File.ReadAllText(_tokenFile).Trim();
            }

            // token found
            if (!string.IsNullOrEmpty(token))
            {
                textBoxToken.Text = token;
                InitClient();
            }

        }

        public void InitClient()
        {
            var inMemoryCredentials = new InMemoryCredentialStore(new Credentials(textBoxToken.Text.Trim()));
            _client = new GitHubClient(new ProductHeaderValue("GithubTool.ConsoleTest"), inMemoryCredentials);

            buttonUpload.Enabled = true;
        }

        private async void ButtonUpload_Click(object sender, EventArgs e)
        {
            //if (comboBoxPath.Items.Count == 0)
            //{
            //    return;
            //}
            progressBar1.Visible = true;
            progressBar1.Style = ProgressBarStyle.Marquee;

            Stream zipFileStream = null;

            //string tempFile = Path.GetTempFileName();
            string tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // should zip?
            if (comboBoxPath.Items.Count > 0)
            {
                CleanTemp(tempFile);

                using (var zipArchive = ZipFile.Open(tempFile, ZipArchiveMode.Create))
                {
                    foreach (string file in comboBoxPath.Items)
                    {
                        if (!File.Exists(file))
                        {
                            continue;
                        }
                        // unrooted path
                        if (!Path.IsPathRooted(file))
                        {
                            MessageBox.Show("Invalid path");
                            return;
                        }
                        // only allow zipping .dll files
                        if (!file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        using (var fs = new FileStream(file, System.IO.FileMode.Open))
                        {
                            //zipFileStream = new MemoryStream();
                            //var zipArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Create, true);
                            var entry = zipArchive.CreateEntry(Path.GetFileName(file));
                            Stream entryStream = entry.Open();
                            await fs.CopyToAsync(entryStream);
                            entryStream.Close();
                            Debug.WriteLine($"File added to zip-archive: {Path.GetFileName(file)}");
                        }
                    }
                }

                zipFileStream = new FileStream(tempFile, System.IO.FileMode.Open);
            }
            else
            {
                // it's already zipped
                zipFileStream = new FileStream(comboBoxPath.Items[0].ToString(), System.IO.FileMode.Open);
            }

            //zipFileStream.Position = 0;
            IReadOnlyList<Release> releases = await _client.Repository.Release.GetAll(owner, name); // .ConfigureAwait(false);
            Release dotnetFour = releases.FirstOrDefault(r => r.Name.Equals(".NET 4+", StringComparison.OrdinalIgnoreCase));
            if (dotnetFour == null)
            {
                return;
            }

            // the main file name will be the selected file in combobox
            string archiveFileName = Path.GetFileNameWithoutExtension(comboBoxPath.Text) + ".zip";
            ReleaseAssetUpload releaseAssetUpload = new ReleaseAssetUpload(archiveFileName, "application/zip", zipFileStream, null);

            // note: if file already exists it will throw apiexception
            // delete asset if already exits online
            IReadOnlyList<ReleaseAsset> result = await _client.Repository.Release.GetAllAssets(owner, name, dotnetFour.Id); //.ConfigureAwait(false);
            ReleaseAsset oldAsset = result.FirstOrDefault(a => a.Name.Equals(releaseAssetUpload.FileName, StringComparison.Ordinal));
            if (oldAsset != null)
            {
                await _client.Repository.Release.DeleteAsset(owner, name, oldAsset.Id);//.ConfigureAwait(false);
            }

            ReleaseAsset releaseAsset = await _client.Repository.Release.UploadAsset(dotnetFour, releaseAssetUpload);//.ConfigureAwait(false);

            // close the temp zip stream
            zipFileStream.Dispose();

            MessageBox.Show("Plugin uploaded with sucess!");

            CleanTemp(tempFile);

            progressBar1.Style = ProgressBarStyle.Blocks;
            progressBar1.Visible = false;
        }

        private static void CleanTemp(string tempFile)
        {
            // cleanup temp file
            if (!File.Exists(tempFile))
            {
                return;
            }
            try
            {
                File.Delete(tempFile);
            }
            catch
            {
                // ignore error
            }
        }

        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            using (var fileDialog = new OpenFileDialog())
            {
                string location = Assembly.GetExecutingAssembly().Location;

                //Directory.SetCurrentDirectory(path + @"\..\..\..\");
                string startupLocation = Directory.GetParent(location).Parent.Parent.Parent.FullName;
                fileDialog.InitialDirectory = startupLocation;
                fileDialog.Filter = "Dll files|*.dll|Zip files|*.zip|All files|*.*";
                fileDialog.Multiselect = true;

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    comboBoxPath.BeginUpdate();

                    for (int i = fileDialog.FileNames.Length - 1; i >= 0; i--)
                    {
                        string name = fileDialog.FileNames[i];
                        // todo: optimize
                        if (!comboBoxPath.Items.Contains(name))
                        {
                            comboBoxPath.Items.Insert(0, name);
                        }
                    }
                    ControlItemsLimit();

                    if (comboBoxPath.Items.Count > 0)
                    {
                        comboBoxPath.SelectedIndex = 0;
                    }

                    comboBoxPath.EndUpdate();
                }
            }
        }

        private void ControlItemsLimit()
        {
            if (comboBoxPath.Items.Count >= 10)
            {
                int dif = comboBoxPath.Items.Count - Math.Abs(comboBoxPath.Items.Count - 10);
                for (int i = comboBoxPath.Items.Count - 1; i >= dif; i--)
                {
                    comboBoxPath.Items.RemoveAt(i);
                }
            }
        }

        private void buttonSaveToken_Click(object sender, EventArgs e)
        {
            string token = textBoxToken.Text.Trim();

            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            File.WriteAllText(_tokenFile, token);
            InitClient();
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
