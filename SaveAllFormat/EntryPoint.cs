using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class SaveAllFormat : EntryPointBase
    {

        public SaveAllFormat()
            : base("Save all format", "Save all format", 1.0m, "Save current subtitle to all available text format.", "tool", string.Empty)
        {
        }

        public override string DoAction(Form parentForm, string srtText, double frameRate, string uiLineBreak, string file, string videoFile, string rawText)
        {
            Init(srtText, uiLineBreak, file);
            var type = parentForm.GetType().Assembly.GetType("Nikse.SubtitleEdit.Core.SubtitleFormats.SubtitleFormat");
            var prop = type.GetProperty("AllSubtitleFormats", BindingFlags.Public | BindingFlags.Static);
            var allSubtitleFormats = (IEnumerable<object>)prop.GetValue(default, default);

            // get subtilte instance
            var subtitle = parentForm.GetType().GetField("_subtitle", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(parentForm);

            foreach (var format in allSubtitleFormats)
            {
                try
                {
                    var name = format.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance).GetValue(format, default);
                    string extension = (string)format.GetType().GetProperty("Extension", BindingFlags.Public | BindingFlags.Instance).GetValue(format, default);
                    var mi = format.GetType().GetMethod("ToText", BindingFlags.Public | BindingFlags.Instance /*| BindingFlags.DeclaredOnly*/, default, new Type[] { subtitle.GetType(), typeof(string) }, default);
                    var result = (string)mi.Invoke(format, new[] { subtitle, name });
                    File.WriteAllText(GetFileName(file, (string)name, extension), result, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
            }

            MessageBox.Show("Done!");
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
