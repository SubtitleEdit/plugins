using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class ExportAllFormats : EntryPointBase
    {

        public ExportAllFormats()
            : base("Export to all formats", "Export to all formats (non binary)", 1.0m, "Export current subtitle to all available text format.", "file", string.Empty)
        {
        }

        public override string DoAction(Form parentForm, string srtText, double frameRate, string uiLineBreak, string file, string videoFile, string rawText)
        {
            // subtitle not loaded
            if (string.IsNullOrWhiteSpace(srtText))
            {
                MessageBox.Show(parentForm, "Empty subtitle... make sure you have load a subtitle file before trying to export!", "Empty subtitle", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return string.Empty;
            }


            string exportLocation = string.Empty;
            using (var folderBrowse = new FolderBrowserDialog()
            {
                Description = "Choose where to export files to"
            })
            {
                if (folderBrowse.ShowDialog() != DialogResult.OK)
                {
                    return string.Empty;
                }
                exportLocation = folderBrowse.SelectedPath;
            }

            // todo: add support to portable version
            // todo: add support to export specific format e.g: xml, txt...
            // todo: add ui which options

            Init(srtText, uiLineBreak, file);

            var type = parentForm.GetType().Assembly.GetType("Nikse.SubtitleEdit.Core.SubtitleFormats.SubtitleFormat");
            var prop = type.GetProperty("AllSubtitleFormats", BindingFlags.Public | BindingFlags.Static);
            var allSubtitleFormats = (IEnumerable<object>)prop.GetValue(default, default);

            var subtitle = parentForm.GetType().GetField("_subtitle", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(parentForm);

            //var parallelOptions = new ParallelOptions();
            //TaskScheduler.FromCurrentSynchronizationContext();

            // run export parallel (faster)
            Parallel.ForEach(allSubtitleFormats, format =>
            {
                try
                {
                    var name = format.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance).GetValue(format, default);
                    string extension = (string)format.GetType().GetProperty("Extension", BindingFlags.Public | BindingFlags.Instance).GetValue(format, default);
                    var mi = format.GetType().GetMethod("ToText", BindingFlags.Public | BindingFlags.Instance /*| BindingFlags.DeclaredOnly*/, default, new Type[] { subtitle.GetType(), typeof(string) }, default);
                    var result = (string)mi.Invoke(format, new[] { subtitle, name });
                    File.WriteAllText(Path.Combine(exportLocation, GetFileName(file, (string)name, extension)), result, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
            });

            MessageBox.Show(parentForm, "Export completed!", "Subtitle exported", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return string.Empty;
        }

        private static string GetFileName(string file, string formatName, string extension)
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
