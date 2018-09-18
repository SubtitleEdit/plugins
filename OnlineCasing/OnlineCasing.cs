using OnlineCasing.Forms;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class OnlineCasing : IPlugin
    {
        public string Name => "Online casing";

        public string Text => "Online casing";

        public decimal Version => .1m;

        public string Description => "Change casing using online names from IMBD";

        public string ActionType => "spellcheck";

        public string Shortcut => string.Empty;

        public string DoAction(Form parentForm, string srtText, double frameRate, string UILineBreak, string file, string videoFile, string rawText)
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomain_ReflectionOnlyAssemblyResolve;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var subRip = new SubRip();
            var subtitle = new Subtitle(subRip);
            subRip.LoadSubtitle(subtitle, rawText.SplitToLines(), file);
            //Application.UserAppDataRegistry
            using (var mainForm = new Main(subtitle))
            {
                if (mainForm.ShowDialog(parentForm) == DialogResult.OK)
                {
                    return mainForm.Subtitle;
                }
            }
            // hack SE through parentForm
            // Nikse.SubtitleEdit.Core.FixCasing.cs is the file to hack

            var formType = parentForm.GetType();

            var fixCasingType = formType.Assembly.GetType("Nikse.SubtitleEdit.Core.FixCasing");

            // 
            //Activator.CreateInstance(fixCasingType, BindingFlags.Instance | BindingFlags.Public, parentForm);

            return string.Empty;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            path = Path.Combine(path, "Plugins");
            path = Path.Combine(path, $"{args.Name.Split(',').First()}.dll");
            return Assembly.Load(File.ReadAllBytes(path));
        }

        private Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
