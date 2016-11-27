namespace PluginCoreLibTest.Console
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Windows.Forms;

    class Program
    {
        private static readonly DirectoryInfo _pluginDirectory = new DirectoryInfo(@"..\..\..\HI2UC\bin\Debug");

        private static void Main(string[] args)
        {
            // TODO: Create an app-domain only for plugins.

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            string pluginfile = Path.Combine(_pluginDirectory.FullName, "HI2UC.dll");
            string pluginfile2 = Path.Combine(_pluginDirectory.FullName, "HIColorer.dll");

            if (!File.Exists(pluginfile))
            {
                throw new InvalidOperationException();
            }

            byte[] assemblyBuffer = File.ReadAllBytes(pluginfile);
            byte[] assemblyBuffer2 = File.ReadAllBytes(pluginfile2);

            Assembly assembly = Assembly.Load(assemblyBuffer);
            Assembly assembly2 = Assembly.Load(assemblyBuffer2);

            Type hi2ucType = assembly.GetType("Nikse.SubtitleEdit.PluginLogic.HI2UC");
            Type hIColorerType = assembly2.GetType("Nikse.SubtitleEdit.PluginLogic.HIColorer");

            object hi2ucObj = Activator.CreateInstance(hi2ucType);
            object hIColorerObj = Activator.CreateInstance(hIColorerType);

            //MethodInfo runMi = hi2ucType.GetMethod("DoAction");
            //var parentForm = new Form();
            //parentForm.Text = AppDomain.CurrentDomain.FriendlyName;
            //MethodBase arg_E3_0 = runMi;
            //object arg_E3_1 = hi2ucObj;
            //object[] expr_D5 = new object[7];
            //expr_D5[0] = parentForm;
            //expr_D5[2] = 0;
            //arg_E3_0.Invoke(arg_E3_1, expr_D5);
            //Console.ReadLine();
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string pluginCoreLibFile = Path.Combine(_pluginDirectory.FullName, "PluginCoreLib.dll");
            if (!File.Exists(pluginCoreLibFile)) return null;
            byte[] assemblyBuffer = File.ReadAllBytes(pluginCoreLibFile);
            return Assembly.Load(assemblyBuffer);
        }

    }
}

// https://msdn.microsoft.com/en-us/library/dd153782.aspx#Consider Switching to the Default Load Context