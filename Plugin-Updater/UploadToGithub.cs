using Octokit;
using Octokit.Internal;
using System;
using System.Collections.Generic;
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
        private const string _tokenFile = "token.txt";
        public UploadToGithub()
        {
            InitializeComponent();

            // hook event handlers

            buttonOK.Click += (sender, e) =>
            {
                DialogResult = DialogResult.OK;
            };

            buttonCancel.Click += (sender, e) =>
            {
                DialogResult = DialogResult.Cancel;
            };


            if (!File.Exists(_tokenFile))
            {
                return;
            }
            var token = File.ReadAllText(_tokenFile).Trim();
            textBoxToken.Text = token;

            InitClient();

        }

        public void InitClient()
        {
            var inMemoryCredentials = new InMemoryCredentialStore(new Credentials(textBoxToken.Text.Trim()));
            _client = new GitHubClient(new ProductHeaderValue("GithubTool.ConsoleTest"), inMemoryCredentials);

            buttonUpload.Enabled = true;
        }

        private async void ButtonUpload_Click(object sender, EventArgs e)
        {
            if (comboBoxPath.Items.Count == 0)
            {
                return;
            }
            Stream zipFileStream = null;

            //string tempFile = Path.GetTempFileName();
            string tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            // should zip?
            if (comboBoxPath.Items.Count > 1)
            {
                CleanTemp(tempFile);

                using (var zipArchive = ZipFile.Open(tempFile, ZipArchiveMode.Create))
                {
                    foreach (string file in comboBoxPath.Items)
                    {
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
                await _client.Repository.Release.DeleteAsset(owner, name, oldAsset.Id).ConfigureAwait(false);
            }

            ReleaseAsset releaseAsset = await _client.Repository.Release.UploadAsset(dotnetFour, releaseAssetUpload);//.ConfigureAwait(false);
            zipFileStream.Dispose();

            MessageBox.Show("Plugin uploaded with sucess!");

            CleanTemp(tempFile);
        }

        private static void CleanTemp(string tempFile)
        {
            // cleanup temp file
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
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
                    comboBoxPath.Items.AddRange(fileDialog.FileNames);

                    if (comboBoxPath.Items.Count > 0)
                    {
                        comboBoxPath.SelectedIndex = 0;
                    }
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
    }
}
