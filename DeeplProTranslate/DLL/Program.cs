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

            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph("Hallo my little friend who has all the", 0, 2000));
            sub.Paragraphs.Add(new Paragraph("right things in this very own coat pocket", 2001, 4000));
            sub.Paragraphs.Add(new Paragraph("and the pocket is really really big!", 4001, 6000));
            Application.Run(new MainForm(sub, "translate", "description", null));
        }
    }
}
