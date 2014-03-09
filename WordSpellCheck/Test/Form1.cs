using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.PluginLogic;

namespace WordSpellCheckTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            IPlugin plugin = new Nikse.SubtitleEdit.PluginLogic.WordSpellCheck();
            string subtitleFileName = @"C:\Users\Nikse\Desktop\spellcheck.srt";
            plugin.DoAction(null, System.IO.File.ReadAllText(subtitleFileName), 25, "<br />", subtitleFileName, @"C:\Users\Nikse\Desktop\Trailers\Game Of Thrones Season Trailer.mp4", System.IO.File.ReadAllText(subtitleFileName));

        }
    }
}
