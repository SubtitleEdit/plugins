using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.PluginLogic;

namespace SubtitleEdit
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

            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph("Hallo world!", 0, 20000));
            //Subtitle sub, string title, string description, Form parentForm
            Application.Run(new MainForm(sub, "translate", "description", null));
        }
    }
}
