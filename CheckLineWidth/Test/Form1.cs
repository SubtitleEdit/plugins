using Nikse.SubtitleEdit.PluginLogic;
using System.Windows.Forms;

namespace WordSpellCheckTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            IPlugin plugin = new Nikse.SubtitleEdit.PluginLogic.CheckLineWidth();
            string subtitleFileName = @"C:\Users\nikse_000\Desktop\CHK\LRLT1.srt";
            plugin.DoAction(null, System.IO.File.ReadAllText(subtitleFileName), 25, "<br />", subtitleFileName, @"C:\Users\Nikse\Desktop\Trailers\Game Of Thrones Season Trailer.mp4", System.IO.File.ReadAllText(subtitleFileName));
        }
    }
}