using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public partial class Main : Form
    {
        private readonly Form _parentForm;
        private readonly Subtitle _subtitle;
        private readonly string _file;
        private string _exportLocation;

        // todo: add support to portable version
        // todo: add support to export specific format e.g: xml, txt...
        // todo: add ui with options

        public Main(Form parentForm, Subtitle subtitle)
        {
            InitializeComponent();

            if (parentForm is null)
            {
                throw new ArgumentNullException(nameof(parentForm));
            }

            _parentForm = parentForm;
            _subtitle = subtitle;

            // event handlers
            buttonCancel.Click += delegate { DialogResult = DialogResult.Cancel; };

            textBoxLocation.DoubleClick += delegate
            {
                if (!Path.IsPathRooted(textBoxLocation.Text))
                {
                    MessageBox.Show("Invalid path", "Invalid path", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Process.Start($"{textBoxLocation.Text}");
            };
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            using (var folderBrowse = new FolderBrowserDialog() { Description = "Choose where to export files to" })
            {
                // point to current
                folderBrowse.SelectedPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "downloads");
                if (folderBrowse.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                textBoxLocation.Text = _exportLocation = folderBrowse.SelectedPath;
            }
        }

        private async void buttonExport_Click(object sender, EventArgs e)
        {
            if (!Path.IsPathRooted(_exportLocation))
            {
                MessageBox.Show("Invalid output path", "Invalid output", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            await Task.Run(() =>
            {
                // TODO: Use Parallel.ForeachAsync in .NET >= 6
                ParallelLoopResult parallelLoopResult = Parallel.ForEach(SubtitleFormat.AllSubtitleFormats, exportFormat =>
                {
                    try
                    {
                        var outputFileName = Path.Combine(_exportLocation, GetExportFileName(_subtitle.FileName, exportFormat.Name, exportFormat.Extension));
                        var content = exportFormat.ToText(_subtitle, _subtitle.FileName);
                        File.WriteAllText(outputFileName, content, Encoding.UTF8);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }
                });
                SpinWait.SpinUntil(() => parallelLoopResult.IsCompleted);
            }).ConfigureAwait(true);

            Process.Start("explorer", $"\"{_exportLocation}\"");
            MessageBox.Show(_parentForm, "Export completed!", "Subtitle exported", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static string GetExportFileName(string file, string formatName, string extension)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                return Path.GetRandomFileName() + extension;
            }

            string fileName = Path.GetFileNameWithoutExtension(file);
            string newName = $"{fileName}_{formatName}{extension}";
            foreach (var ch in Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()))
            {
                newName = newName.Replace(ch.ToString(), "_");
            }

            newName = newName.Replace(" ", "-");
            newName = newName.Replace(" ", "-");
            return newName;
        }
    }
}