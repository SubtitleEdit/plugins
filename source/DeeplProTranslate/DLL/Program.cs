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
            sub.Paragraphs.Add(new Paragraph("<b>He was one of my favorite little minor</b>", 0, 2000));
            sub.Paragraphs.Add(new Paragraph("<b>characters in Ulysses,</b>", 2001, 4000));
            sub.Paragraphs.Add(new Paragraph("<b>but that's why I have</b>", 4001, 6000));
            sub.Paragraphs.Add(new Paragraph("<b>the hat on and the jacket.</b>", 6001, 8000));
            Application.Run(new MainForm(sub, "translate", "description", null));
        }
    }
}
