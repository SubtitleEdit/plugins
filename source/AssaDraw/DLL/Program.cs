using System;
using System.Windows.Forms;

namespace AssaDraw
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

            Application.Run(new FormAssaDrawMain("standalone", null, null, fileName));
        }
    }
}
