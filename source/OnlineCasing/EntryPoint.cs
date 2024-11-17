using OnlineCasing.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class OnlineCasing : IPlugin
    {
        public string Name => "Online casing";

        public string Text => "Online casing";

        public decimal Version => 1.4m;

        public string Description => "Do casing using online TMDB api";

        public string ActionType => "spellcheck";

        public string Shortcut => string.Empty;

        public string DoAction(Form parentForm, string srtText, double frameRate, string UILineBreak, string file, string videoFile, string rawText)
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomain_ReflectionOnlyAssemblyResolve;
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyUtils.CurrentDomainAssemblyResolve;
            // BuildFixCasing(parentForm, new List<string>());
            var subRip = new SubRip();
            var subtitle = new Subtitle(subRip);
            subRip.LoadSubtitle(subtitle, rawText.SplitToLines(), file);

            // Application.UserAppDataRegistry
            using (var mainForm = new Main(subtitle, UILineBreak))
            {
                if (mainForm.ShowDialog(parentForm) == DialogResult.OK)
                {
                    return mainForm.Subtitle;
                }
            }

            // hack SE through parentForm
            // Nikse.SubtitleEdit.Core.FixCasing.cs is the file to hack
            // var formType = parentForm.GetType();
            // var fixCasingType = formType.Assembly.GetType("Nikse.SubtitleEdit.Core.FixCasing");
            // 
            // Activator.CreateInstance(fixCasingType, BindingFlags.Instance | BindingFlags.Public, parentForm);

            return string.Empty;
        }

        private Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            System.Diagnostics.Trace.WriteLine("CurrentDomain_ReflectionOnlyAssemblyResolve invoked!");
            return null;
        }

        private void BuildFixCasing(Form mainForm, List<string> names)
        {
            // return only AssemblyName
            //var libse = Assembly.GetEntryAssembly().GetReferencedAssemblies().FirstOrDefault(s => s.Name.Equals("libse"));

            // note: when SubtitlEdit is running in installer mode we won't be 
            // able to retrive "libse.dll" using this approche because the assembly is merged into SubtitleEdit.exe
            var ass = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(s => s.FullName.Contains("libse"));

            // if libse is not present the class will be available in SubtilteEdit
            ass = ass ?? AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.Contains("subtitleedit", StringComparison.OrdinalIgnoreCase));

            // running in installed mode
            if (ass == null)
            {
                throw new InvalidOperationException();
            }

            var fixCasingT = ass.GetType("Nikse.SubtitleEdit.Core.FixCasing");
            var obj = Activator.CreateInstance(fixCasingT, "en");
            var field = fixCasingT.GetField("_names", BindingFlags.Instance | BindingFlags.NonPublic);
            var namesList = field.GetValue(obj) as List<string>;
            //foreach (var item in namesList)
            //{
            //    Debug.WriteLine(item);
            //}
            // INFO: ALL WORKING!
            //var namesField = fixCasingType.GetField("_names", BindingFlags.Instance | BindingFlags.NonPublic);
#if false
            // # FIRST TRY TO GET LIBSE TYPES

            using (var file = new FileStream($"d:\\se-assembly-{Path.GetRandomFileName()}.txt", FileMode.OpenOrCreate, FileAccess.Write))
            using (var sw = new StreamWriter(file))
            {
                var mainType = mainForm.GetType();
                var assembly = mainType.Assembly;

                foreach (var kvp in assembly.GetTypes().GroupBy(t => t.Namespace))
                {
                    sw.WriteLine(kvp.Key);
                    foreach (Type type in kvp)
                    {
                        sw.WriteLine(type.Name);
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                }

                // INFO: this would work for installed version of SE because all the assembly are merged in one place,
                // for portable version the libse.dll is not included in SubtilteEdit.exe
                //Type fixCasingType = assembly.GetType("Nikse.SubtitleEdit.Core.FixCasing");

                //MessageBox.Show(fixCasingType != null ? "found" : "Not found!");

                //var obj = Activator.CreateInstance(fixCasingType, "en");
                //var namesField = fixCasingType.GetField("_names", BindingFlags.Instance | BindingFlags.NonPublic);

                ////namesField.SetValue(obj, namesField);
                //var fixMethod = fixCasingType.GetMethod("Fix", BindingFlags.Public | BindingFlags.Instance);

                //var subtitle = new Subtitle(new SubRip());
                //fixMethod.Invoke(obj, new[] { subtitle });

                //fixMethod.Invoke(obj, BindingFlags.Instance | BindingFlags.Public
                //fixMethod.Invoke()
            } 
#endif
        }
    }
}
