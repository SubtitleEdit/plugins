using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    // dll file name must "<classname>.dll" - e.g. "Haxor.dll"
    public class HI2UC : IPlugin
    {
        #region Metadata

        public string ActionType => "tool";

        public string Description { get; }

        public string Name { get; }

        public string Shortcut { get; }

        // the text that will be displaying when subtitle edit context-menu
        public string Text { get; }

        public decimal Version { get; }

        #endregion

        public HI2UC()
        {
            // get metadata from assembly
            //var descriptionAttribute = Assembly.GetExecutingAssembly()
            //.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)
            //.OfType<AssemblyDescriptionAttribute>().FirstOrDefault();

            var thisAssembly = Assembly.GetExecutingAssembly();
            var descriptionAttribute = AssemblyUtils.GetCustomAttribute<AssemblyDescriptionAttribute>(thisAssembly);
            //var assemblyVersion = AssemblyUtils.GetCustomAttribute<AssemblyVersionAttribute>(thisAssembly); why null?
            AssemblyName assemblyName = thisAssembly.GetName();

            // var AssemblyTitleAttribute = AssemblyUtils.GetCustomAttribute<AssemblyTitleAttribute>(thisAssembly);
            // Name = AssemblyTitleAttribute.Title;
            // Trace.WriteLine(Name);

            Name = assemblyName.Name;
            Text = assemblyName.Name;

            Description = descriptionAttribute.Description;

            // use invariant-culture due to some culture uses "," as decimal separator
            Version = decimal.Parse(assemblyName.Version.ToString(2), System.Globalization.CultureInfo.InvariantCulture);
            Shortcut = string.Empty;
        }


        public string DoAction(Form parentForm, string srtText, double frame, string uiLineBreak, string file,
            string videoFile, string rawText)
        {
            // Make sure subtitle isn't null or empty
            if (string.IsNullOrWhiteSpace(srtText))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }

            // Use custom separator for list view new lines
            if (!string.IsNullOrEmpty(uiLineBreak))
            {
                Options.UiLineBreak = uiLineBreak;
            }

            // Get subtitle raw lines
            var list = new List<string>(srtText.SplitToLines());
            var srt = new SubRip();
            var sub = new Subtitle(srt);

            // Load raws subtitle lines into Subtitle object
            srt.LoadSubtitle(sub, list, file);

            IPlugin hi2Uc = this;
            using (var form = new PluginForm(sub, hi2Uc.Name, hi2Uc.Description))
            {
                if (form.ShowDialog(parentForm) == DialogResult.OK)
                {
                    return form.Subtitle;
                }
            }

            return string.Empty;
        }

    }
}