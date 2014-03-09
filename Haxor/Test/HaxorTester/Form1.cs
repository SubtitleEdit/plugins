using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.PluginLogic;

namespace HaxorTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            InitializeComponent();
            IPlugin plugin = new Nikse.SubtitleEdit.PluginLogic.Haxor();
            string subtitleFileName = @"C:\Users\Nikse\Desktop\spellcheck.srt";
            MessageBox.Show(plugin.DoAction(null, System.IO.File.ReadAllText(subtitleFileName), 25, "<br />", subtitleFileName, @"C:\Users\Nikse\Desktop\Trailers\Game Of Thrones Season Trailer.mp4", System.IO.File.ReadAllText(subtitleFileName)));           
        }
    }
}
