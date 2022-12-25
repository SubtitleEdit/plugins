using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Nikse.SubtitleEdit.PluginLogic;

namespace Dropbox.Api
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var fileName = string.Empty;
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                fileName = args[1];
            }

            var form = new PluginForm("DropBox Load", "Load subtitle from DropBox");
            form.ShowDialog();
        }
    }
}
