using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public partial class Main : Form
    {
        private readonly Form _parentForm;
        private readonly string _file;
        private string _exportLocation;

        // todo: add support to portable version
        // todo: add support to export specific format e.g: xml, txt...
        // todo: add ui which options

        public Main(Form parentForm, string file)
        {
            if (parentForm is null)
            {
                throw new ArgumentNullException(nameof(parentForm));
            }

            if (string.IsNullOrWhiteSpace(file))
            {
                throw new ArgumentException($"'{nameof(file)}' cannot be null or whitespace", nameof(file));
            }

            InitializeComponent();
            progressBar1.Visible = false;
            _parentForm = parentForm;
            _file = file;

            // event handlers

            buttonOk.Click += delegate
            {
                DialogResult = DialogResult.OK;
            };
            buttonCancel.Click += delegate
            {
                DialogResult = DialogResult.Cancel;
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

                _exportLocation = folderBrowse.SelectedPath;
                textBox1.Text = _exportLocation;
            }
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            // validate output path
            if (!Path.IsPathRooted(_exportLocation))
            {
                MessageBox.Show("Invalid output path", "Invalid output", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // todo: progress bar
            //progressBar1.Style = ProgressBarStyle.Continuous;
            //progressBar1.Visible = true;

            var type = _parentForm.GetType().Assembly.GetType("Nikse.SubtitleEdit.Core.SubtitleFormats.SubtitleFormat");
            var prop = type.GetProperty("AllSubtitleFormats", BindingFlags.Public | BindingFlags.Static);
            var subtitle = _parentForm.GetType().GetField("_subtitle", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_parentForm);
            //var parallelOptions = new ParallelOptions();
            //TaskScheduler.FromCurrentSynchronizationContext();
            // run export parallel (faster)
            Parallel.ForEach((IEnumerable<object>)prop.GetValue(default, default), format =>
            {
                try
                {
                    var name = format.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance).GetValue(format, default);
                    string extension = (string)format.GetType().GetProperty("Extension", BindingFlags.Public | BindingFlags.Instance).GetValue(format, default);

                    // todo: accept extension like: *.txt just .txt
                    // filter by extension
                    if (!comboBoxExtension.SelectedValue.ToString().Equals(extension, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    var mi = format.GetType().GetMethod("ToText", BindingFlags.Public | BindingFlags.Instance /*| BindingFlags.DeclaredOnly*/, default, new Type[] { subtitle.GetType(), typeof(string) }, default);
                    var result = (string)mi.Invoke(format, new[] { subtitle, name });
                    File.WriteAllText(Path.Combine(_exportLocation, GetExportFileName(_file, (string)name, extension)), result, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
            });

            // todo: progressbar
            //progressBar1.Style = ProgressBarStyle.Blocks;
            //progressBar1.Visible = false;

            MessageBox.Show(_parentForm, "Export completed!", "Subtitle exported", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static string GetExportFileName(string file, string formatName, string extension)
        {
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
